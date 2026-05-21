using InvestTracker.Models;
using InvestTracker.Models.Enums;

namespace InvestTracker.Services.Interfaces;

public interface ITransactionService
{
    // Récupérer toutes les transactions d'un utilisateur
    Task<List<Transaction>> GetTransactionsAsync(int userId);

    // Récupérer une transaction par Id
    Task<Transaction?> GetTransactionByIdAsync(int id);

    // Filtrer par type (Buy/Sell)
    Task<List<Transaction>> GetTransactionsByTypeAsync(
        int userId, TransactionType type);

    // Filtrer par actif
    Task<List<Transaction>> GetTransactionsByAssetAsync(
        int userId, int assetId);

    // Filtrer par période
    Task<List<Transaction>> GetTransactionsByDateRangeAsync(
        int userId, DateTime from, DateTime to);

    // Enregistrer un achat (vérifie le budget disponible)
    Task<bool> BuyAsync(Transaction transaction);

    // Enregistrer une vente (vérifie la quantité disponible)
    Task<bool> SellAsync(Transaction transaction);

    // Supprimer une transaction
    Task DeleteTransactionAsync(int id);
}