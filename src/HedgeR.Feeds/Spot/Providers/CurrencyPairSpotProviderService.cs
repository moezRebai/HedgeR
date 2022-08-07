
using System.Threading.Channels;

internal class CurrencyPairSpotProviderService : BackgroundService
{
    private readonly ICurrencyPairSpotGenerator _currencyPairSpotGenerator;
    private readonly ILogger<CurrencyPairSpotProviderService> _logger;

    public CurrencyPairSpotProviderService(ICurrencyPairSpotGenerator currencyPairSpotGenerator,
        ILogger<CurrencyPairSpotProviderService> logger)
    {
        _currencyPairSpotGenerator = currencyPairSpotGenerator;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _ = _currencyPairSpotGenerator.Start();
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _currencyPairSpotGenerator.Stop();
    }

    protected async override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (stoppingToken.IsCancellationRequested)
        {
            await StopAsync(stoppingToken);
        }

       await StartAsync(stoppingToken);
    }
}
