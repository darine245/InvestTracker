using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InvestTracker.Models;

public class PortfolioLine
{
    [Key]
    public int Id { get; set; }

    // FK vers Portfolio
    [Required]
    public int PortfolioId { get; set; }

    // FK vers Asset
    [Required]
    public int AssetId { get; set; }

    // Quantité actuellement détenue
    [Required]
    [Range(0, double.MaxValue,
        ErrorMessage = "La quantité doit être positive.")]
    [Column(TypeName = "decimal(18,6)")]
    public decimal Quantity { get; set; }

    // Prix moyen d'achat (recalculé à chaque achat)
    [Required]
    [Range(0, double.MaxValue)]
    [Column(TypeName = "decimal(18,6)")]
    public decimal AverageBuyPrice { get; set; }

    // ===== NAVIGATION =====
    public Portfolio Portfolio { get; set; } = null!;
    public Asset Asset { get; set; } = null!;

    // ===== PROPRIÉTÉS CALCULÉES (pas en base) =====
    // Valeur actuelle de la ligne = Quantité × Prix actuel de l'actif
    [NotMapped]
    public decimal CurrentValue => Quantity * Asset?.CurrentPrice ?? 0;

    // Gain ou perte = Valeur actuelle − (Quantité × Prix moyen d'achat)
    [NotMapped]
    public decimal GainLoss => CurrentValue - (Quantity * AverageBuyPrice);

    // Gain/perte en pourcentage
    [NotMapped]
    public decimal GainLossPercent =>
        AverageBuyPrice > 0
        ? ((Asset?.CurrentPrice ?? 0 - AverageBuyPrice) / AverageBuyPrice) * 100
        : 0;
}