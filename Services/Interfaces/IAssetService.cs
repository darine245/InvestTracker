using InvestTracker.Models;
using InvestTracker.Models.Enums;

namespace InvestTracker.Services.Interfaces;

public interface IAssetService
{
    // Récupérer tous les actifs
    Task<List<Asset>> GetAssetsAsync();

    // Récupérer un actif par son Id
    Task<Asset?> GetAssetByIdAsync(int id);

    // Filtrer par type (Stock / Crypto)
    Task<List<Asset>> GetAssetsByTypeAsync(AssetType type);

    // Recherche par nom ou symbole
    Task<List<Asset>> SearchAssetsAsync(string keyword);

    // Ajouter un actif
    Task AddAssetAsync(Asset asset);

    // Modifier un actif (dont le prix courant)
    Task UpdateAssetAsync(Asset asset);

    // Supprimer un actif
    Task DeleteAssetAsync(int id);
}