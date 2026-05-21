using System.ComponentModel.DataAnnotations;

namespace InvestTracker.Models;

public class Portfolio
{
    [Key]
    public int Id { get; set; }

    // FK vers User (relation 1-to-1)
    [Required]
    public int UserId { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 1,
        ErrorMessage = "Le nom du portefeuille est obligatoire.")]
    public string Name { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // ===== NAVIGATION =====
    public User User { get; set; } = null!;

    // Un Portfolio contient PLUSIEURS lignes
    public ICollection<PortfolioLine> Lines { get; set; }
        = new List<PortfolioLine>();
}