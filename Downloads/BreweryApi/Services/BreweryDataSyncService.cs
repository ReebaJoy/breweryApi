using BreweryApi.Interfaces;

namespace BreweryApi.Services;

// Background service for automatic data refresh (10-minute cache)
public class BreweryDataSyncService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<BreweryDataSyncService> _logger;
    private readonly TimeSpan _syncInterval = TimeSpan.FromMinutes(10);

    public BreweryDataSyncService(
        IServiceProvider serviceProvider,
        ILogger<BreweryDataSyncService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Brewery Data Sync Service starting");

        // Initial sync on startup
        await SyncBreweryDataAsync();

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(_syncInterval, stoppingToken);
                await SyncBreweryDataAsync();
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Brewery Data Sync Service is stopping");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Brewery Data Sync Service");
            }
        }
    }

    private async Task SyncBreweryDataAsync()
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var dataService = scope.ServiceProvider.GetRequiredService<IBreweryDataService>();
            var repository = scope.ServiceProvider.GetRequiredService<IBreweryRepository>();

            _logger.LogInformation("Starting brewery data sync");
            
            var breweries = await dataService.FetchBreweriesFromSourceAsync();
            await repository.AddBreweriesAsync(breweries);
            
            _logger.LogInformation("Brewery data sync completed successfully. Synced {Count} breweries", breweries.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to sync brewery data");
        }
    }
}