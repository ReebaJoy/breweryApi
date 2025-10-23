using BreweryApi.Models;

namespace BreweryApi.Interfaces;

// Service layer interface (business logic)
public interface IBreweryService
{
    Task<PaginatedResponse<BreweryDto>> GetBreweriesAsync(BreweryQueryParameters parameters);
    Task<BreweryDto?> GetBreweryByIdAsync(string id);
    Task<List<AutocompleteResult>> GetAutocompleteResultsAsync(string searchTerm, int limit = 10);
    Task RefreshBreweryDataAsync();
}

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

// External data service interface
public interface IBreweryDataService
{
    Task<List<Brewery>> FetchBreweriesFromSourceAsync();
}