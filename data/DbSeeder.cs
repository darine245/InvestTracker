using InvestTracker.Models;
using InvestTracker.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace InvestTracker.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        if (await context.Users.AnyAsync()) return;

        Console.WriteLine("--- Injection des données de test ---");

        // 1. ACTIFS
        var bitcoin = new Asset
        {
            Name = "Bitcoin",
            Symbol = "BTC",
            Type = AssetType.Crypto,
            CurrentPrice = 62000.00m,
            LastUpdated = DateTime.Now
        };
        var ethereum = new Asset
        {
            Name = "Ethereum",
            Symbol = "ETH",
            Type = AssetType.Crypto,
            CurrentPrice = 3200.00m,
            LastUpdated = DateTime.Now
        };
        var apple = new Asset
        {
            Name = "Apple Inc.",
            Symbol = "AAPL",
            Type = AssetType.Stock,
            CurrentPrice = 175.50m,
            LastUpdated = DateTime.Now
        };
        var tesla = new Asset
        {
            Name = "Tesla Inc.",
            Symbol = "TSLA",
            Type = AssetType.Stock,
            CurrentPrice = 245.00m,
            LastUpdated = DateTime.Now
        };

        context.Assets.AddRange(bitcoin, ethereum, apple, tesla);
        await context.SaveChangesAsync();
        Console.WriteLine($"✅ {await context.Assets.CountAsync()} actifs créés.");

        // 2. UTILISATEUR
        var user = new User
        {
            FullName = "Ahmed Ben Ali",
            Email = "ahmed@investtracker.com",
            PasswordHash = "hashed_password_123",
            InvestmentBudget = 50000.00m,
            CreatedAt = DateTime.Now
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();
        Console.WriteLine($"✅ Utilisateur créé : {user.FullName}");

        // 3. PORTFOLIO
        var portfolio = new Portfolio
        {
            UserId = user.Id,
            Name = "Mon Portefeuille Principal",
            CreatedAt = DateTime.Now
        };
        context.Portfolios.Add(portfolio);
        await context.SaveChangesAsync();
        Console.WriteLine($"✅ Portfolio créé : {portfolio.Name}");

        // 4. TRANSACTIONS
        var tx1 = new Transaction
        {
            UserId = user.Id,
            AssetId = bitcoin.Id,
            Type = TransactionType.Buy,
            Quantity = 0.5m,
            PriceAtTransaction = 60000.00m,
            Date = DateTime.Now.AddDays(-30),
            Notes = "Premier achat Bitcoin"
        };
        var tx2 = new Transaction
        {
            UserId = user.Id,
            AssetId = ethereum.Id,
            Type = TransactionType.Buy,
            Quantity = 2.0m,
            PriceAtTransaction = 3000.00m,
            Date = DateTime.Now.AddDays(-20),
            Notes = "Achat Ethereum"
        };
        var tx3 = new Transaction
        {
            UserId = user.Id,
            AssetId = apple.Id,
            Type = TransactionType.Buy,
            Quantity = 10.0m,
            PriceAtTransaction = 170.00m,
            Date = DateTime.Now.AddDays(-15),
            Notes = "Achat Apple"
        };
        var tx4 = new Transaction
        {
            UserId = user.Id,
            AssetId = bitcoin.Id,
            Type = TransactionType.Sell,
            Quantity = 0.1m,
            PriceAtTransaction = 62000.00m,
            Date = DateTime.Now.AddDays(-5),
            Notes = "Prise de bénéfices BTC"
        };

        context.Transactions.AddRange(tx1, tx2, tx3, tx4);
        await context.SaveChangesAsync();
        Console.WriteLine($"✅ {await context.Transactions.CountAsync()} transactions créées.");

        // 5. LIGNES PORTFOLIO
        var lineBtc = new PortfolioLine
        {
            PortfolioId = portfolio.Id,
            AssetId = bitcoin.Id,
            Quantity = 0.4m,
            AverageBuyPrice = 60000.00m
        };
        var lineEth = new PortfolioLine
        {
            PortfolioId = portfolio.Id,
            AssetId = ethereum.Id,
            Quantity = 2.0m,
            AverageBuyPrice = 3000.00m
        };
        var lineAapl = new PortfolioLine
        {
            PortfolioId = portfolio.Id,
            AssetId = apple.Id,
            Quantity = 10.0m,
            AverageBuyPrice = 170.00m
        };

        context.PortfolioLines.AddRange(lineBtc, lineEth, lineAapl);
        await context.SaveChangesAsync();
        Console.WriteLine($"✅ {await context.PortfolioLines.CountAsync()} lignes créées.");
        Console.WriteLine("--- Seeding terminé ! ---");
    }
}