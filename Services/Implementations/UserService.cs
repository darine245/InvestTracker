using InvestTracker.Data;
using InvestTracker.Models;
using InvestTracker.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace InvestTracker.Services.Implementations;

public class UserService : IUserService
{
    private readonly AppDbContext _context;

    public UserService(AppDbContext context)
    {
        _context = context;
    }
  // ===== NOUVEAU : hash SHA-256==
    public static string HashPassword(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToHexString(bytes).ToLower();
    }

    // ===== NOUVEAU : authentification =====
    public async Task<User?> AuthenticateAsync(string username, string password)
    {
        var hash = HashPassword(password);
        return await _context.Users
            .FirstOrDefaultAsync(u =>
                u.Username == username &&
                u.PasswordHash == hash);
    }
    public async Task<List<User>> GetUsersAsync()
    {
        return await _context.Users
            .OrderBy(u => u.FullName)
            .ToListAsync();
    }

    public async Task<User?> GetUserByIdAsync(int id)
    {
        return await _context.Users.FindAsync(id);
    }

    public async Task AddUserAsync(User user)
    {
        user.CreatedAt = DateTime.Now;
        user.PasswordHash = HashPassword(user.PasswordHash);
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateUserAsync(User user)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteUserAsync(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user != null)
        {
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

        }
    }

    public async Task<decimal> GetAvailableBudgetAsync(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return 0;

        // Total dépensé en achats
        var totalBought = await _context.Transactions
            .Where(t => t.UserId == userId
                && t.Type == Models.Enums.TransactionType.Buy)
            .SumAsync(t => t.Quantity * t.PriceAtTransaction);

        // Total récupéré en ventes
        var totalSold = await _context.Transactions
            .Where(t => t.UserId == userId
                && t.Type == Models.Enums.TransactionType.Sell)
            .SumAsync(t => t.Quantity * t.PriceAtTransaction);

        // Budget disponible = Budget initial - Achats + Ventes
        return user.InvestmentBudget - totalBought + totalSold;
    }
}