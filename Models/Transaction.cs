using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using InvestTracker.Models.Enums;

namespace InvestTracker.Models;

public class Transaction
{
    [Key]
    public int Id { get; set; }

    // FK vers User
    [Required]
    public int UserId { get; set; }

    // FK vers Asset
    [Required]
    [Range(1, int.MaxValue,
        ErrorMessage = "Veuillez sélectionner un actif.")]
    public int AssetId { get; set; }

    [Required]
    public TransactionType Type { get; set; }

    [Required]
    [Range(0.000001, double.MaxValue,
        ErrorMessage = "La quantité doit être supérieure à 0.")]
    [Column(TypeName = "decimal(18,6)")]
    public decimal Quantity { get; set; }

    // Prix au MOMENT de la transaction (historique)
    [Required]
    [Range(0, double.MaxValue,
        ErrorMessage = "Le prix doit être positif.")]
    [Column(TypeName = "decimal(18,6)")]
    public decimal PriceAtTransaction { get; set; }

    public DateTime Date { get; set; } = DateTime.Now;

    [StringLength(255)]
    public string? Notes { get; set; }

    // ===== NAVIGATION =====
    public User User { get; set; } = null!;
    public Asset Asset { get; set; } = null!;
}