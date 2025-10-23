using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BreweryApi.Interfaces;
using BreweryApi.Models;

namespace BreweryApi.Controllers.V2;

[ApiController]
[ApiVersion("2.0")]
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
    /// Get paginated list of breweries (V2 - Enhanced response format)
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResponse<BreweryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedResponse<BreweryDto>>> GetBreweries(
        [FromQuery] BreweryQueryParameters parameters)
    {
        var result = await _breweryService.GetBreweriesAsync(parameters);
        
        // V2 adds additional metadata in headers
        Response.Headers.Add("X-Total-Count", result.TotalCount.ToString());
        Response.Headers.Add("X-Page-Count", result.TotalPages.ToString());
        Response.Headers.Add("X-Current-Page", result.Page.ToString());
        
        return Ok(result);
    }

    /// <summary>
    /// Get a specific brewery by ID (V2)
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(BreweryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BreweryDto>> GetBreweryById(string id)
    {
        var brewery = await _breweryService.GetBreweryByIdAsync(id);
        
        if (brewery == null)
        {
            return NotFound(new { 
                message = $"Brewery with ID {id} not found",
                timestamp = DateTime.UtcNow
            });
        }

        return Ok(brewery);
    }

    /// <summary>
    /// Enhanced autocomplete with highlighting (V2)
    /// </summary>
    [HttpGet("autocomplete")]
    [ProducesResponseType(typeof(List<AutocompleteResult>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<AutocompleteResult>>> Autocomplete(
        [FromQuery] string term,
        [FromQuery] int limit = 10)
    {
        if (string.IsNullOrWhiteSpace(term))
        {
            return Ok(new List<AutocompleteResult>());
        }

        var results = await _breweryService.GetAutocompleteResultsAsync(term, limit);
        
        // V2 adds match scoring
        Response.Headers.Add("X-Match-Count", results.Count.ToString());
        
        return Ok(results);
    }
}