
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using InvestTracker.Models.Enums;

namespace InvestTracker.Models;

public class Asset
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 1,
        ErrorMessage = "Le nom est obligatoire.")]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(10, MinimumLength = 1,
        ErrorMessage = "Le symbole est obligatoire (ex: BTC, AAPL).")]
    public string Symbol { get; set; } = string.Empty;

    [Required]
    public AssetType Type { get; set; }

    [Required]
    [Range(0, double.MaxValue,
        ErrorMessage = "Le prix doit être positif.")]
    [Column(TypeName = "decimal(18,6)")]
    public decimal CurrentPrice { get; set; }

    public DateTime LastUpdated { get; set; } = DateTime.Now;

    // ===== NAVIGATION =====
    // Un Asset apparaît dans PLUSIEURS transactions
    public ICollection<Transaction> Transactions { get; set; }
        = new List<Transaction>();

    // Un Asset apparaît dans PLUSIEURS lignes de portefeuille
    public ICollection<PortfolioLine> PortfolioLines { get; set; }
        = new List<PortfolioLine>();
}