using Microsoft.EntityFrameworkCore;
using InvestTracker.Models;
using InvestTracker.Models.Enums;

namespace InvestTracker.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // ===== TABLES =====
    public DbSet<User> Users { get; set; }
    public DbSet<Asset> Assets { get; set; }
    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<Portfolio> Portfolios { get; set; }
    public DbSet<PortfolioLine> PortfolioLines { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ===== RELATION User - Portfolio (1-to-1) =====
        modelBuilder.Entity<User>()
            .HasOne(u => u.Portfolio)
            .WithOne(p => p.User)
            .HasForeignKey<Portfolio>(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // ===== RELATION User - Transaction (1-to-N) =====
        modelBuilder.Entity<Transaction>()
            .HasOne(t => t.User)
            .WithMany(u => u.Transactions)
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // ===== RELATION Asset - Transaction (1-to-N) =====
        modelBuilder.Entity<Transaction>()
            .HasOne(t => t.Asset)
            .WithMany(a => a.Transactions)
            .HasForeignKey(t => t.AssetId)
            .OnDelete(DeleteBehavior.Restrict);

        // ===== RELATION Portfolio - PortfolioLine (1-to-N) =====
        modelBuilder.Entity<PortfolioLine>()
            .HasOne(pl => pl.Portfolio)
            .WithMany(p => p.Lines)
            .HasForeignKey(pl => pl.PortfolioId)
            .OnDelete(DeleteBehavior.Cascade);

        // ===== RELATION Asset - PortfolioLine (1-to-N) =====
        modelBuilder.Entity<PortfolioLine>()
            .HasOne(pl => pl.Asset)
            .WithMany(a => a.PortfolioLines)
            .HasForeignKey(pl => pl.AssetId)
            .OnDelete(DeleteBehavior.Restrict);

        // ===== ENUM stockés comme string dans MySQL =====
        modelBuilder.Entity<Asset>()
            .Property(a => a.Type)
            .HasConversion<string>();

        modelBuilder.Entity<Transaction>()
            .Property(t => t.Type)
            .HasConversion<string>();

        // ===== EMAIL UNIQUE =====
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        // ===== SYMBOL UNIQUE =====
        modelBuilder.Entity<Asset>()
            .HasIndex(a => a.Symbol)
            .IsUnique();
    }
}