using InvestTracker.Data;
using InvestTracker.Models;
using InvestTracker.Models.Enums;
using InvestTracker.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InvestTracker.Services.Implementations;

public class AssetService : IAssetService
{
    private readonly AppDbContext _context;

    public AssetService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Asset>> GetAssetsAsync()
    {
        return await _context.Assets
            .OrderBy(a => a.Name)
            .ToListAsync();
    }

    public async Task<Asset?> GetAssetByIdAsync(int id)
    {
        return await _context.Assets.FindAsync(id);
    }

    public async Task<List<Asset>> GetAssetsByTypeAsync(AssetType type)
    {
        return await _context.Assets
            .Where(a => a.Type == type)
            .OrderBy(a => a.Name)
            .ToListAsync();
    }

    public async Task<List<Asset>> SearchAssetsAsync(string keyword)
    {
        // Recherche insensible à la casse sur Name ou Symbol
        return await _context.Assets
            .Where(a => a.Name.Contains(keyword)
                || a.Symbol.Contains(keyword))
            .OrderBy(a => a.Name)
            .ToListAsync();
    }

    public async Task AddAssetAsync(Asset asset)
    {
        asset.LastUpdated = DateTime.Now;
        _context.Assets.Add(asset);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAssetAsync(Asset asset)
    {
        asset.LastUpdated = DateTime.Now;
        _context.Assets.Update(asset);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAssetAsync(int id)
    {
        var asset = await _context.Assets.FindAsync(id);
        if (asset != null)
        {
            _context.Assets.Remove(asset);
            await _context.SaveChangesAsync();
        }
    }
}