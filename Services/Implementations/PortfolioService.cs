using InvestTracker.Data;
using InvestTracker.Models;
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
        // Include + ThenInclude = charge les lignes ET les actifs liés
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
        // Valeur actuelle = somme (Quantité × Prix actuel)
        return lines.Sum(l => l.Quantity * l.Asset.CurrentPrice);
    }

    public async Task<decimal> GetTotalGainLossAsync(int userId)
    {
        var lines = await GetPortfolioLinesAsync(userId);
        var currentValue = lines.Sum(l =>
            l.Quantity * l.Asset.CurrentPrice);
        var totalCost = lines.Sum(l =>
            l.Quantity * l.AverageBuyPrice);
        // Gain/perte = Valeur actuelle - Coût total
        return currentValue - totalCost;
    }

    public async Task<decimal> GetTotalGainLossPercentAsync(int userId)
    {
        var lines = await GetPortfolioLinesAsync(userId);
        var totalCost = lines.Sum(l =>
            l.Quantity * l.AverageBuyPrice);
        if (totalCost == 0) return 0;
        var gainLoss = await GetTotalGainLossAsync(userId);
        return Math.Round((gainLoss / totalCost) * 100, 2);
    }

    public async Task<Dictionary<string, decimal>> GetAllocationByTypeAsync(
        int userId)
    {
        var lines = await GetPortfolioLinesAsync(userId);
        var totalValue = lines.Sum(l =>
            l.Quantity * l.Asset.CurrentPrice);

        if (totalValue == 0)
            return new Dictionary<string, decimal>
            {
                { "Stock", 0 }, { "Crypto", 0 }
            };

        // Grouper par type et calculer % de chaque type
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
        // Trier par gain/perte décroissant
        return lines
            .OrderByDescending(l =>
                (l.Quantity * l.Asset.CurrentPrice)
                - (l.Quantity * l.AverageBuyPrice))
            .ToList();
    }

    public async Task CreatePortfolioAsync(int userId, string name)
    {
        // Vérifier qu'il n'en a pas déjà un
        var existing = await _context.Portfolios
            .FirstOrDefaultAsync(p => p.UserId == userId);
        if (existing != null) return;

        var portfolio = new Portfolio
        {
            UserId = userId,
            Name = name,
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
                && l.AssetId == assetId);

        if (isBuy)
        {
            if (line == null)
            {
                // Nouvelle ligne → premier achat de cet actif
                line = new PortfolioLine
                {
                    PortfolioId = portfolioId,
                    AssetId = assetId,
                    Quantity = quantity,
                    AverageBuyPrice = price
                };
                _context.PortfolioLines.Add(line);
            }
            else
            {
                // Recalcul du prix moyen d'achat
                // (ancQté × ancPrix + nvQté × nvPrix) ÷ (ancQté + nvQté)
                var totalCost = (line.Quantity * line.AverageBuyPrice)
                    + (quantity * price);
                line.Quantity += quantity;
                line.AverageBuyPrice = totalCost / line.Quantity;
                _context.PortfolioLines.Update(line);
            }
        }
        else // Vente
        {
            if (line != null)
            {
                line.Quantity -= quantity;
                if (line.Quantity <= 0)
                    // Supprimer la ligne si plus rien
                    _context.PortfolioLines.Remove(line);
                else
                    _context.PortfolioLines.Update(line);
            }
        }
        await _context.SaveChangesAsync();
    }
}