using System.Text.Json;
using BreweryApi.Interfaces;
using BreweryApi.Models;

namespace BreweryApi.Services;

// External data service implementation (Single Responsibility: API integration)
public class BreweryDataService : IBreweryDataService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<BreweryDataService> _logger;
    private const string BaseUrl = "https://api.openbrewerydb.org/v1/breweries";

    public BreweryDataService(HttpClient httpClient, ILogger<BreweryDataService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<List<Brewery>> FetchBreweriesFromSourceAsync()
    {
        try
        {
            _logger.LogInformation("Fetching breweries from OpenBreweryDB API");
            
            var allBreweries = new List<Brewery>();
            var page = 1;
            var perPage = 200;
            var hasMoreData = true;

            while (hasMoreData)
            {
                var url = $"{BaseUrl}?page={page}&per_page={perPage}";
                var response = await _httpClient.GetAsync(url);
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Failed to fetch breweries. Status: {Status}", response.StatusCode);
                    break;
                }

                var content = await response.Content.ReadAsStringAsync();
                var sourceBreweries = JsonSerializer.Deserialize<List<OpenBreweryDbResponse>>(content, 
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (sourceBreweries == null || sourceBreweries.Count == 0)
                {
                    hasMoreData = false;
                    break;
                }

                // Map/Transform source data to generic data model
                var mappedBreweries = sourceBreweries.Select(MapToBrewery).ToList();
                allBreweries.AddRange(mappedBreweries);
                
                _logger.LogInformation("Fetched page {Page} with {Count} breweries", page, mappedBreweries.Count);
                
                if (sourceBreweries.Count < perPage)
                {
                    hasMoreData = false;
                }
                
                page++;
                
                // Safety limit to prevent infinite loops
                if (page > 100)
                {
                    _logger.LogWarning("Reached maximum page limit");
                    hasMoreData = false;
                }
            }

            _logger.LogInformation("Successfully fetched {Total} breweries", allBreweries.Count);
            return allBreweries;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching breweries from source API");
            throw;
        }
    }

    // Mapping method to transform source API data to generic model
    private Brewery MapToBrewery(OpenBreweryDbResponse source)
    {
        return new Brewery
        {
            Id = source.Id,
            Name = source.Name ?? string.Empty,
            City = source.City ?? string.Empty,
            State = source.State_Province ?? source.State,
            Country = source.Country,
            PostalCode = source.Postal_Code,
            Phone = CleanPhoneNumber(source.Phone),
            WebsiteUrl = source.Website_Url,
            BreweryType = source.Brewery_Type,
            Latitude = ParseDouble(source.Latitude),
            Longitude = ParseDouble(source.Longitude),
            LastUpdated = DateTime.UtcNow
        };
    }

    private string? CleanPhoneNumber(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            return null;
        
        return phone.Trim();
    }

    private double? ParseDouble(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        if (double.TryParse(value, out var result))
            return result;

        return null;
    }
}