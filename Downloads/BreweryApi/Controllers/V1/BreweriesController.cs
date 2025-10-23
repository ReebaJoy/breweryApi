using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BreweryApi.Interfaces;
using BreweryApi.Models;

namespace BreweryApi.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class BreweriesController : ControllerBase
{
    private readonly IBreweryService _breweryService;
    private readonly ILogger<BreweriesController> _logger;

    public BreweriesController(
        IBreweryService breweryService,
        ILogger<BreweriesController> logger)
    {
        _breweryService = breweryService;
        _logger = logger;
    }

    /// <summary>
    /// Get paginated list of breweries with optional filtering and sorting
    /// </summary>
    /// <param name="searchTerm">Search term for name, city, or state</param>
    /// <param name="city">Filter by city</param>
    /// <param name="state">Filter by state</param>
    /// <param name="sortBy">Sort by: name, city, or distance</param>
    /// <param name="ascending">Sort order (true for ascending, false for descending)</param>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Items per page (default: 50)</param>
    /// <param name="userLatitude">User latitude for distance calculation</param>
    /// <param name="userLongitude">User longitude for distance calculation</param>
    /// <returns>Paginated list of breweries</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResponse<BreweryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PaginatedResponse<BreweryDto>>> GetBreweries(
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? city = null,
        [FromQuery] string? state = null,
        [FromQuery] string sortBy = "name",
        [FromQuery] bool ascending = true,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] double? userLatitude = null,
        [FromQuery] double? userLongitude = null)
    {
        var parameters = new BreweryQueryParameters
        {
            SearchTerm = searchTerm,
            City = city,
            State = state,
            SortBy = sortBy,
            Ascending = ascending,
            Page = page,
            PageSize = pageSize,
            UserLatitude = userLatitude,
            UserLongitude = userLongitude
        };

        var result = await _breweryService.GetBreweriesAsync(parameters);
        return Ok(result);
    }

    /// <summary>
    /// Get a specific brewery by ID
    /// </summary>
    /// <param name="id">Brewery ID</param>
    /// <returns>Brewery details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(BreweryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<BreweryDto>> GetBreweryById(string id)
    {
        var brewery = await _breweryService.GetBreweryByIdAsync(id);
        
        if (brewery == null)
        {
            _logger.LogWarning("Brewery with ID {Id} not found", id);
            return NotFound(new { message = $"Brewery with ID {id} not found" });
        }

        return Ok(brewery);
    }

    /// <summary>
    /// Autocomplete search for brewery names
    /// </summary>
    /// <param name="term">Search term</param>
    /// <param name="limit">Maximum number of results (default: 10)</param>
    /// <returns>List of autocomplete suggestions</returns>
    [HttpGet("autocomplete")]
    [ProducesResponseType(typeof(List<AutocompleteResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<List<AutocompleteResult>>> Autocomplete(
        [FromQuery] string term,
        [FromQuery] int limit = 10)
    {
        if (string.IsNullOrWhiteSpace(term))
        {
            return Ok(new List<AutocompleteResult>());
        }

        var results = await _breweryService.GetAutocompleteResultsAsync(term, limit);
        return Ok(results);
    }
}