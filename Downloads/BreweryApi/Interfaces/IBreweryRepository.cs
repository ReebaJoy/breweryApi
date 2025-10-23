using BreweryApi.Models;

namespace BreweryApi.Interfaces;

// Repository interface (data access)
public interface IBreweryRepository
{
    Task<List<Brewery>> GetAllBreweriesAsync();
    Task<Brewery?> GetBreweryByIdAsync(string id);
    Task<List<Brewery>> SearchBreweriesAsync(string searchTerm);
    Task AddBreweriesAsync(List<Brewery> breweries);
    Task ClearBreweriesAsync();
    Task<DateTime?> GetLastUpdateTimeAsync();
}