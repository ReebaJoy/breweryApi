using System.ComponentModel.DataAnnotations;

namespace BreweryApi.Models;

// Generic data model (transformed from source API)
public class Brewery
{
    [Key]
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? PostalCode { get; set; }
    public string? Phone { get; set; }
    public string? WebsiteUrl { get; set; }
    public string? BreweryType { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}

// DTO for API responses
public class BreweryDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? WebsiteUrl { get; set; }
    public string? BreweryType { get; set; }
    public double? Distance { get; set; } // Calculated distance from user location
}

// Source API response model
public class OpenBreweryDbResponse
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Brewery_Type { get; set; }
    public string? Address_1 { get; set; }
    public string? Address_2 { get; set; }
    public string? Address_3 { get; set; }
    public string? City { get; set; }
    public string? State_Province { get; set; }
    public string? Postal_Code { get; set; }
    public string? Country { get; set; }
    public string? Longitude { get; set; }
    public string? Latitude { get; set; }
    public string? Phone { get; set; }
    public string? Website_Url { get; set; }
    public string? State { get; set; }
    public string? Street { get; set; }
}

// Query parameters for filtering and sorting
public class BreweryQueryParameters
{
    public string? SearchTerm { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string SortBy { get; set; } = "name"; // name, city, distance
    public bool Ascending { get; set; } = true;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
    public double? UserLatitude { get; set; }
    public double? UserLongitude { get; set; }
    
    // Autocomplete specific
    public int AutocompleteLimit { get; set; } = 10;
}

// Paginated response wrapper
public class PaginatedResponse<T>
{
    public List<T> Data { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public bool HasPreviousPage { get; set; }
    public bool HasNextPage { get; set; }
}

// Autocomplete response
public class AutocompleteResult
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string DisplayText { get; set; } = string.Empty;
}