using BreweryApi.Interfaces;
using BreweryApi.Models;

namespace BreweryApi.Services;

// Business logic service (Open/Closed Principle - open for extension)
public class BreweryService : IBreweryService
{
    private readonly IBreweryRepository _repository;
    private readonly ILogger<BreweryService> _logger;

    public BreweryService(
        IBreweryRepository repository,
        ILogger<BreweryService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<PaginatedResponse<BreweryDto>> GetBreweriesAsync(BreweryQueryParameters parameters)
    {
        try
        {
            _logger.LogInformation("Getting breweries with parameters: {@Parameters}", parameters);
            
            var breweries = await _repository.GetAllBreweriesAsync();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(parameters.SearchTerm))
            {
                breweries = breweries
                    .Where(b => b.Name.Contains(parameters.SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                               (b.City?.Contains(parameters.SearchTerm, StringComparison.OrdinalIgnoreCase) ?? false) ||
                               (b.State?.Contains(parameters.SearchTerm, StringComparison.OrdinalIgnoreCase) ?? false))
                    .ToList();
            }

            // Apply city filter
            if (!string.IsNullOrWhiteSpace(parameters.City))
            {
                breweries = breweries
                    .Where(b => b.City.Equals(parameters.City, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            // Apply state filter
            if (!string.IsNullOrWhiteSpace(parameters.State))
            {
                breweries = breweries
                    .Where(b => b.State != null && b.State.Equals(parameters.State, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            // Convert to DTOs and calculate distance if user location is provided
            var breweryDtos = breweries.Select(b => MapToDto(b, parameters.UserLatitude, parameters.UserLongitude)).ToList();

            // Apply sorting
            breweryDtos = ApplySorting(breweryDtos, parameters.SortBy, parameters.Ascending);

            // Calculate pagination
            var totalCount = breweryDtos.Count;
            var totalPages = (int)Math.Ceiling(totalCount / (double)parameters.PageSize);
            
            var paginatedData = breweryDtos
                .Skip((parameters.Page - 1) * parameters.PageSize)
                .Take(parameters.PageSize)
                .ToList();

            return new PaginatedResponse<BreweryDto>
            {
                Data = paginatedData,
                Page = parameters.Page,
                PageSize = parameters.PageSize,
                TotalCount = totalCount,
                TotalPages = totalPages,
                HasPreviousPage = parameters.Page > 1,
                HasNextPage = parameters.Page < totalPages
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving breweries");
            throw;
        }
    }

    public async Task<BreweryDto?> GetBreweryByIdAsync(string id)
    {
        try
        {
            var brewery = await _repository.GetBreweryByIdAsync(id);
            return brewery != null ? MapToDto(brewery, null, null) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving brewery with id {Id}", id);
            throw;
        }
    }

    public async Task<List<AutocompleteResult>> GetAutocompleteResultsAsync(string searchTerm, int limit = 10)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return new List<AutocompleteResult>();

            _logger.LogInformation("Getting autocomplete results for: {SearchTerm}", searchTerm);
            
            var breweries = await _repository.SearchBreweriesAsync(searchTerm);
            
            return breweries
                .Take(limit)
                .Select(b => new AutocompleteResult
                {
                    Id = b.Id,
                    Name = b.Name,
                    City = b.City,
                    DisplayText = $"{b.Name} - {b.City}, {b.State}"
                })
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting autocomplete results");
            throw;
        }
    }

    public async Task RefreshBreweryDataAsync()
    {
        _logger.LogInformation("Manual refresh of brewery data requested");
        // This will be handled by the background service
        await Task.CompletedTask;
    }

    private BreweryDto MapToDto(Brewery brewery, double? userLat, double? userLon)
    {
        double? distance = null;

        if (userLat.HasValue && userLon.HasValue && 
            brewery.Latitude.HasValue && brewery.Longitude.HasValue)
        {
            distance = CalculateDistance(userLat.Value, userLon.Value, 
                                        brewery.Latitude.Value, brewery.Longitude.Value);
        }

        return new BreweryDto
        {
            Id = brewery.Id,
            Name = brewery.Name,
            City = brewery.City,
            Phone = brewery.Phone,
            State = brewery.State,
            Country = brewery.Country,
            WebsiteUrl = brewery.WebsiteUrl,
            BreweryType = brewery.BreweryType,
            Distance = distance
        };
    }

    private List<BreweryDto> ApplySorting(List<BreweryDto> breweries, string sortBy, bool ascending)
    {
        return sortBy.ToLower() switch
        {
            "name" => ascending 
                ? breweries.OrderBy(b => b.Name).ToList() 
                : breweries.OrderByDescending(b => b.Name).ToList(),
            
            "city" => ascending 
                ? breweries.OrderBy(b => b.City).ToList() 
                : breweries.OrderByDescending(b => b.City).ToList(),
            
            "distance" => ascending 
                ? breweries.OrderBy(b => b.Distance ?? double.MaxValue).ToList() 
                : breweries.OrderByDescending(b => b.Distance ?? double.MinValue).ToList(),
            
            _ => breweries.OrderBy(b => b.Name).ToList()
        };
    }

    // Haversine formula for distance calculation
    private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371; // Earth's radius in kilometers

        var dLat = ToRadians(lat2 - lat1);
        var dLon = ToRadians(lon2 - lon1);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return R * c;
    }

    private double ToRadians(double degrees)
    {
        return degrees * Math.PI / 180;
    }
}