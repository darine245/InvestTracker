using InvestTracker.Data;
using InvestTracker.Models;
using InvestTracker.Models.Enums;
using InvestTracker.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InvestTracker.Services.Implementations;

public class PortfolioService : IPortfolioService
{
    private readonly AppDbContext _context;

    public PortfolioService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Portfolio?> GetPortfolioAsync(int userId)
    {
        return await _context.Portfolios
            .Include(p => p.Lines)
                .ThenInclude(l => l.Asset)
            .FirstOrDefaultAsync(p => p.UserId == userId);
    }

    public async Task<List<PortfolioLine>> GetPortfolioLinesAsync(int userId)
    {
        var portfolio = await GetPortfolioAsync(userId);
        return portfolio?.Lines.ToList() ?? new List<PortfolioLine>();
    }

    public async Task<decimal> GetTotalValueAsync(int userId)
    {
        var lines = await GetPortfolioLinesAsync(userId);
        return lines.Sum(l => l.Quantity * l.Asset.CurrentPrice);
    }

    public async Task<decimal> GetTotalGainLossAsync(int userId)
    {
        var lines = await GetPortfolioLinesAsync(userId);
        var currentValue = lines.Sum(l => l.Quantity * l.Asset.CurrentPrice);
        var totalCost    = lines.Sum(l => l.Quantity * l.AverageBuyPrice);
        return currentValue - totalCost;
    }

    public async Task<decimal> GetTotalGainLossPercentAsync(int userId)
    {
        var lines = await GetPortfolioLinesAsync(userId);
        var totalCost = lines.Sum(l => l.Quantity * l.AverageBuyPrice);
        if (totalCost == 0) return 0;
        var gainLoss = await GetTotalGainLossAsync(userId);
        return Math.Round((gainLoss / totalCost) * 100, 2);
    }

    public async Task<Dictionary<string, decimal>> GetAllocationByTypeAsync(int userId)
    {
        var lines = await GetPortfolioLinesAsync(userId);
        var totalValue = lines.Sum(l => l.Quantity * l.Asset.CurrentPrice);

        if (totalValue == 0)
            return new Dictionary<string, decimal>
            {
                { "Stock", 0 }, { "Crypto", 0 }
            };

        return lines
            .GroupBy(l => l.Asset.Type.ToString())
            .ToDictionary(
                g => g.Key,
                g => Math.Round(
                    (g.Sum(l => l.Quantity * l.Asset.CurrentPrice)
                    / totalValue) * 100, 2)
            );
    }

    public async Task<List<PortfolioLine>> GetPerformanceAsync(int userId)
    {
        var lines = await GetPortfolioLinesAsync(userId);
        return lines
            .OrderByDescending(l =>
                (l.Quantity * l.Asset.CurrentPrice)
                - (l.Quantity * l.AverageBuyPrice))
            .ToList();
    }

    public async Task CreatePortfolioAsync(int userId, string name)
    {
        var existing = await _context.Portfolios
            .FirstOrDefaultAsync(p => p.UserId == userId);
        if (existing != null) return;

        var portfolio = new Portfolio
        {
            UserId    = userId,
            Name      = name,
            CreatedAt = DateTime.Now
        };
        _context.Portfolios.Add(portfolio);
        await _context.SaveChangesAsync();
    }

    public async Task UpdatePortfolioLineAsync(
        int portfolioId, int assetId,
        decimal quantity, decimal price,
        bool isBuy)
    {
        var line = await _context.PortfolioLines
            .FirstOrDefaultAsync(l =>
                l.PortfolioId == portfolioId
                && l.AssetId  == assetId);

        if (isBuy)
        {
            if (line == null)
            {
                // Premier achat de cet actif → nouvelle ligne
                line = new PortfolioLine
                {
                    PortfolioId    = portfolioId,
                    AssetId        = assetId,
                    Quantity       = quantity,
                    AverageBuyPrice = price
                };
                _context.PortfolioLines.Add(line);
            }
            else
            {
                // Recalcul prix moyen pondéré
                var totalCost = (line.Quantity * line.AverageBuyPrice)
                              + (quantity * price);
                line.Quantity       += quantity;
                line.AverageBuyPrice = totalCost / line.Quantity;
                _context.PortfolioLines.Update(line);
            }
        }
        else // Vente ou annulation d'achat
        {
            if (line != null)
            {
                line.Quantity -= quantity;
                if (line.Quantity <= 0)
                    _context.PortfolioLines.Remove(line);
                else
                    _context.PortfolioLines.Update(line);
            }
        }

        await _context.SaveChangesAsync();
    }

    // ─────────────────────────────────────────────────────────────
    // REBUILD : repart de zéro et rejoue toutes les transactions
    // existantes pour recalculer chaque ligne du portefeuille.
    // ─────────────────────────────────────────────────────────────
    public async Task RebuildPortfolioAsync(int userId)
    {
        // 1. Récupérer ou créer le portfolio
        var portfolio = await _context.Portfolios
            .Include(p => p.Lines)
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (portfolio == null)
        {
            portfolio = new Portfolio
            {
                UserId    = userId,
                Name      = "Mon Portefeuille",
                CreatedAt = DateTime.Now
            };
            _context.Portfolios.Add(portfolio);
            await _context.SaveChangesAsync();
        }

        // 2. Supprimer toutes les lignes actuelles
        _context.PortfolioLines.RemoveRange(portfolio.Lines);
        await _context.SaveChangesAsync();

        // 3. Rejouer toutes les transactions encore en base,
        //    dans l'ordre chronologique
        var transactions = await _context.Transactions
            .Where(t => t.UserId == userId)
            .OrderBy(t => t.Date)
            .ToListAsync();

        // Dictionnaire temporaire : assetId → (quantité, coût total)
        var holdings = new Dictionary<int, (decimal qty, decimal totalCost)>();

        foreach (var tx in transactions)
        {
            if (!holdings.ContainsKey(tx.AssetId))
                holdings[tx.AssetId] = (0m, 0m);

            var (qty, cost) = holdings[tx.AssetId];

            if (tx.Type == TransactionType.Buy)
            {
                var newQty  = qty  + tx.Quantity;
                var newCost = cost + (tx.Quantity * tx.PriceAtTransaction);
                holdings[tx.AssetId] = (newQty, newCost);
            }
            else // Sell
            {
                var newQty  = qty  - tx.Quantity;
                // On garde le coût moyen (le coût total diminue
                // proportionnellement à la quantité vendue)
                var avgPrice = qty > 0 ? cost / qty : 0m;
                var newCost  = newQty > 0 ? newQty * avgPrice : 0m;
                holdings[tx.AssetId] = (newQty < 0 ? 0m : newQty, newCost);
            }
        }

        // 4. Créer les lignes finales pour tout ce qui reste > 0
        foreach (var (assetId, (qty, totalCost)) in holdings)
        {
            if (qty <= 0) continue;

            var avgBuyPrice = qty > 0 ? totalCost / qty : 0m;

            _context.PortfolioLines.Add(new PortfolioLine
            {
                PortfolioId     = portfolio.Id,
                AssetId         = assetId,
                Quantity        = qty,
                AverageBuyPrice = avgBuyPrice
            });
        }

        await _context.SaveChangesAsync();
        Console.WriteLine($"[RebuildPortfolio] Portefeuille reconstruit pour userId={userId}");
    }
}