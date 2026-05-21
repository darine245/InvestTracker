using InvestTracker.Models;

namespace InvestTracker.Services.Interfaces;

public interface IUserService
{
    // Récupérer tous les utilisateurs
    Task<List<User>> GetUsersAsync();

    // Récupérer un utilisateur par son Id
    Task<User?> GetUserByIdAsync(int id);

    // Ajouter un utilisateur
    Task AddUserAsync(User user);

    // Modifier un utilisateur
    Task UpdateUserAsync(User user);

    // Supprimer un utilisateur
    Task DeleteUserAsync(int id);

    // Calculer le budget restant disponible
    // Budget total - total des achats + total des ventes
    Task<decimal> GetAvailableBudgetAsync(int userId);
    // Authentification : cherche un user par username + vérifie le mot de passe
    Task<User?> AuthenticateAsync(string username, string password);
}