
using HedgeR.Spot.Requests;

internal class CurrencyPairSpotProviderService : BackgroundService
{
    private readonly ICurrencyPairSpotGenerator _currencyPairSpotGenerator;
    private readonly SpotRequestChannel _channel;
    private readonly ILogger<CurrencyPairSpotProviderService> _logger;
    // 0 : stop; 1 : start
    private int startFeeder = 0;

    public CurrencyPairSpotProviderService(ICurrencyPairSpotGenerator currencyPairSpotGenerator, SpotRequestChannel channel,
        ILogger<CurrencyPairSpotProviderService> logger)
    {
        _currencyPairSpotGenerator = currencyPairSpotGenerator;
        _channel = channel;
        _logger = logger;
    }

    protected async override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var request in _channel.Requests.Reader.ReadAllAsync(stoppingToken))
        {
            var _ = request switch
            {
                RequestStartSpotFeeder => StartSpotFeeder(),
                RequestStopSpotFeeder => StopSpotFeeder(),
                _ => Task.CompletedTask,
            };
        }
    }

    private Task StopSpotFeeder()
    {
        _logger.LogInformation("Request stop Spot Feeder  !");

        if (Interlocked.Exchange(ref startFeeder, 0) == 0)
        {
            _logger.LogWarning("Spot Feeder already stopped !");
            return Task.CompletedTask;
        }

        return _currencyPairSpotGenerator.StopAsync();
    }

    private Task StartSpotFeeder()
    {
        _logger.LogInformation("Request start Spot Feeder  !");

        if (Interlocked.Exchange(ref startFeeder, 1) == 1)
        {
            _logger.LogWarning("Spot Feeder already started !");
            return Task.CompletedTask;
        }

        return _currencyPairSpotGenerator.StartAsync();
    }
}
