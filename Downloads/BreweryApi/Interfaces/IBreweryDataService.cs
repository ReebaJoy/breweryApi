using BreweryApi.Models;

namespace BreweryApi.Interfaces;

// External data service interface
public interface IBreweryDataService
{
    Task<List<Brewery>> FetchBreweriesFromSourceAsync();
}