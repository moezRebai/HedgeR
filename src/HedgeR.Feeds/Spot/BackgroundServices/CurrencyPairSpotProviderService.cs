
using HedgeR.Shared.Streaming;
using HedgeR.Spot.Requests;

internal class CurrencyPairSpotProviderService : BackgroundService
{
    private readonly ICurrencyPairSpotProvider _currencyPairSpotGenerator;
    private readonly SpotRequestChannel _channel;
    private readonly ILogger<CurrencyPairSpotProviderService> _logger;
    private readonly IStreamingPublisher _streamingPublisher;
    private int _startFeeder;

    public CurrencyPairSpotProviderService(ICurrencyPairSpotProvider currencyPairSpotGenerator, SpotRequestChannel channel,
        ILogger<CurrencyPairSpotProviderService> logger, IStreamingPublisher streamingPublisher)
    {
        _currencyPairSpotGenerator = currencyPairSpotGenerator;
        _channel = channel;
        _logger = logger;
        _streamingPublisher = streamingPublisher;
    }

    protected async override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var request in _channel.Requests.Reader.ReadAllAsync(stoppingToken))
        {
            var _ = request switch
            {
                RequestStartSpotFeeder => StartSpotFeeder(request.Frequency),
                RequestStopSpotFeeder => StopSpotFeeder(),
                _ => Task.CompletedTask,
            };
        }
    }

    private Task StopSpotFeeder()
    {
        _logger.LogInformation("Request stop SpotFeeder  !");

        if (Interlocked.Exchange(ref _startFeeder, 0) == 0)
        {
            _logger.LogWarning("SpotFeeder already stopped !");
            return Task.CompletedTask;
        }

        return _currencyPairSpotGenerator.StopAsync();
    }

    private async Task StartSpotFeeder(int frequency)
    {
        _logger.LogInformation("Request start SpotFeeder  !");

        if (Interlocked.Exchange(ref _startFeeder, 1) == 1)
        {
            _logger.LogWarning("SpotFeeder already started !");
            return ;
        }

        await foreach (var currencyPair in _currencyPairSpotGenerator.StartStreamingAsync(frequency))
        {
            _logger.LogInformation("Publishing the currencyPair spot ....");

            await _streamingPublisher.PublishAsync(currencyPair.CurrencyPair, currencyPair);
        }
    }
}
