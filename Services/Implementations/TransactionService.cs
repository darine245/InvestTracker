using InvestTracker.Data;
using InvestTracker.Models;
using InvestTracker.Models.Enums;
using InvestTracker.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InvestTracker.Services.Implementations;

public class TransactionService : ITransactionService
{
    private readonly AppDbContext _context;
    private readonly IPortfolioService _portfolioService;
    private readonly IUserService _userService;

    public TransactionService(
        AppDbContext context,
        IPortfolioService portfolioService,
        IUserService userService)
    {
        _context = context;
        _portfolioService = portfolioService;
        _userService = userService;
    }

    public async Task<List<Transaction>> GetTransactionsAsync(int userId)
    {
        return await _context.Transactions
            .Include(t => t.Asset)
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.Date)
            .ToListAsync();
    }

    public async Task<Transaction?> GetTransactionByIdAsync(int id)
    {
        return await _context.Transactions
            .Include(t => t.Asset)
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<List<Transaction>> GetTransactionsByTypeAsync(
        int userId, TransactionType type)
    {
        return await _context.Transactions
            .Include(t => t.Asset)
            .Where(t => t.UserId == userId && t.Type == type)
            .OrderByDescending(t => t.Date)
            .ToListAsync();
    }

    public async Task<List<Transaction>> GetTransactionsByAssetAsync(
        int userId, int assetId)
    {
        return await _context.Transactions
            .Include(t => t.Asset)
            .Where(t => t.UserId == userId && t.AssetId == assetId)
            .OrderByDescending(t => t.Date)
            .ToListAsync();
    }

    public async Task<List<Transaction>> GetTransactionsByDateRangeAsync(
        int userId, DateTime from, DateTime to)
    {
        return await _context.Transactions
            .Include(t => t.Asset)
            .Where(t => t.UserId == userId
                && t.Date >= from
                && t.Date <= to)
            .OrderByDescending(t => t.Date)
            .ToListAsync();
    }

    public async Task<bool> BuyAsync(Transaction transaction)
    {
        // 1. Vérifier le budget disponible
        var available = await _userService
            .GetAvailableBudgetAsync(transaction.UserId);
        var cost = transaction.Quantity * transaction.PriceAtTransaction;

        if (cost > available)
            return false; // Budget insuffisant

        // 2. Enregistrer la transaction
        transaction.Date = DateTime.Now;
        transaction.Type = TransactionType.Buy;
        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();

        // 3. Mettre à jour le portfolio
        var portfolio = await _portfolioService
            .GetPortfolioAsync(transaction.UserId);
        if (portfolio == null)
        {
            await _portfolioService.CreatePortfolioAsync(
                transaction.UserId,
                "Mon Portefeuille");
            portfolio = await _portfolioService
                .GetPortfolioAsync(transaction.UserId);
        }

        if (portfolio != null)
        {
            await _portfolioService.UpdatePortfolioLineAsync(
                portfolio.Id,
                transaction.AssetId,
                transaction.Quantity,
                transaction.PriceAtTransaction,
                isBuy: true);
        }

        return true; // Succès
    }

    public async Task<bool> SellAsync(Transaction transaction)
    {
        // 1. Vérifier la quantité disponible dans le portfolio
        var portfolio = await _portfolioService
            .GetPortfolioAsync(transaction.UserId);

        if (portfolio == null) return false;

        var line = portfolio.Lines
            .FirstOrDefault(l => l.AssetId == transaction.AssetId);

        if (line == null || line.Quantity < transaction.Quantity)
            return false; // Quantité insuffisante

        // 2. Enregistrer la transaction
        transaction.Date = DateTime.Now;
        transaction.Type = TransactionType.Sell;
        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();

        // 3. Mettre à jour le portfolio
        await _portfolioService.UpdatePortfolioLineAsync(
            portfolio.Id,
            transaction.AssetId,
            transaction.Quantity,
            transaction.PriceAtTransaction,
            isBuy: false);

        return true; // Succès
    }

    public async Task DeleteTransactionAsync(int id)
    {
        var transaction = await _context.Transactions.FindAsync(id);
        if (transaction != null)
        {
            _context.Transactions.Remove(transaction);
            await _context.SaveChangesAsync();
        }
    }
}