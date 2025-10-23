using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using BreweryApi.Data;
using BreweryApi.Interfaces;
using BreweryApi.Models;

namespace BreweryApi.Services;

// Repository implementation with caching (Single Responsibility Principle)
public class BreweryRepository : IBreweryRepository
{
    private readonly BreweryDbContext _context;
    private readonly IMemoryCache _cache;
    private readonly ILogger<BreweryRepository> _logger;
    private const string CacheKey = "all_breweries";
    private const string LastUpdateKey = "last_update_time";
    private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(10);

    public BreweryRepository(
        BreweryDbContext context,
        IMemoryCache cache,
        ILogger<BreweryRepository> logger)
    {
        _context = context;
        _cache = cache;
        _logger = logger;
    }

    public async Task<List<Brewery>> GetAllBreweriesAsync()
    {
        if (_cache.TryGetValue(CacheKey, out List<Brewery>? cachedBreweries) && cachedBreweries != null)
        {
            _logger.LogInformation("Returning breweries from cache");
            return cachedBreweries;
        }

        _logger.LogInformation("Cache miss - fetching breweries from database");
        var breweries = await _context.Breweries.ToListAsync();
        
        _cache.Set(CacheKey, breweries, _cacheExpiration);
        return breweries;
    }

    public async Task<Brewery?> GetBreweryByIdAsync(string id)
    {
        var breweries = await GetAllBreweriesAsync();
        return breweries.FirstOrDefault(b => b.Id == id);
    }

    public async Task<List<Brewery>> SearchBreweriesAsync(string searchTerm)
    {
        var breweries = await GetAllBreweriesAsync();
        return breweries
            .Where(b => b.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                       (b.City?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ?? false))
            .ToList();
    }

    public async Task AddBreweriesAsync(List<Brewery> breweries)
    {
        _logger.LogInformation("Adding {Count} breweries to database", breweries.Count);
        
        await _context.Breweries.ExecuteDeleteAsync();
        await _context.Breweries.AddRangeAsync(breweries);
        await _context.SaveChangesAsync();
        
        _cache.Remove(CacheKey);
        _cache.Set(LastUpdateKey, DateTime.UtcNow, _cacheExpiration);
        
        _logger.LogInformation("Breweries successfully added and cache cleared");
    }

    public async Task ClearBreweriesAsync()
    {
        _logger.LogInformation("Clearing all breweries from database");
        await _context.Breweries.ExecuteDeleteAsync();
        _cache.Remove(CacheKey);
        _cache.Remove(LastUpdateKey);
    }

    public Task<DateTime?> GetLastUpdateTimeAsync()
    {
        _cache.TryGetValue(LastUpdateKey, out DateTime? lastUpdate);
        return Task.FromResult(lastUpdate);
    }
}