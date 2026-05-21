using System.ComponentModel.DataAnnotations;


namespace InvestTracker.Models;

public class User
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 2,
        ErrorMessage = "Le nom doit faire entre 2 et 100 caractères.")]
    public string FullName { get; set; } = string.Empty;
     [Required]
    [StringLength(50, MinimumLength = 3,
        ErrorMessage = "Le nom d'utilisateur doit faire entre 3 et 50 caractères.")]
    public string Username { get; set; } = string.Empty;

    [Required]
    [EmailAddress(ErrorMessage = "Format email invalide.")]
    [StringLength(100)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    public string PasswordHash { get; set; } = string.Empty;

    [Required]
    [Range(0, double.MaxValue,
        ErrorMessage = "Le budget doit être positif.")]
    public decimal InvestmentBudget { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // ===== NAVIGATION =====
    // Un User a PLUSIEURS transactions
    public ICollection<Transaction> Transactions { get; set; }
        = new List<Transaction>();

    // Un User a UN seul Portfolio
    public Portfolio? Portfolio { get; set; }
}