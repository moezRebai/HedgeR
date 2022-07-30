
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

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _currencyPairSpotGenerator.Start();

        _logger.LogInformation("CurrencyPair spot provider started !");

        return Task.CompletedTask;
    }
}
