using InvestTracker.Models;

namespace InvestTracker.Services.Interfaces;

public interface IPortfolioService
{
    // Récupérer le portfolio d'un utilisateur avec ses lignes
    Task<Portfolio?> GetPortfolioAsync(int userId);

    // Récupérer toutes les lignes du portfolio
    Task<List<PortfolioLine>> GetPortfolioLinesAsync(int userId);

    // Valeur totale actuelle du portfolio
    // Somme de (Quantité × Prix actuel) pour chaque ligne
    Task<decimal> GetTotalValueAsync(int userId);

    // Gain/perte global en montant
    // Valeur actuelle - total investi
    Task<decimal> GetTotalGainLossAsync(int userId);

    // Gain/perte global en pourcentage
    Task<decimal> GetTotalGainLossPercentAsync(int userId);

    // Répartition par type d'actif (pour le graphique)
    // Retourne : { "Stock": 60.5, "Crypto": 39.5 } en %
    Task<Dictionary<string, decimal>> GetAllocationByTypeAsync(int userId);

    // Performance par actif (gain/perte par ligne)
    Task<List<PortfolioLine>> GetPerformanceAsync(int userId);

    // Créer le portfolio initial d'un utilisateur
    Task CreatePortfolioAsync(int userId, string name);

    // Mettre à jour une ligne après transaction
    Task UpdatePortfolioLineAsync(
        int portfolioId, int assetId,
        decimal quantity, decimal price,
        bool isBuy);
}