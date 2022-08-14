using HedgeR.Shared.Streaming;

namespace HedgeR.Pricer.StreamProviders
{
    public class SpotStreamProvider : BackgroundService
    {
        private readonly ILogger<SpotStreamProvider> _logger;
        private readonly IStreamingSubscriber _subscriber;

        public SpotStreamProvider(ILogger<SpotStreamProvider> logger, IStreamingSubscriber subscriber)
        {
            _logger = logger;
            _subscriber = subscriber;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return _subscriber.SubscribeAsync<CurrencyPairSpot>("SpotFeed", (spot) =>
            {
                _logger.LogInformation($"Pricing received new spot for {spot.CurrencyPair} wih value {spot.Value:F} at {spot.Timestamp}");
            });
        }
    }
}
