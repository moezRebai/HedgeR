using HedgeR.Shared.Models;
using HedgeR.Shared.Streaming;

namespace HedgeR.Pricer.StreamProviders
{
    public class SpreadStreamProvider : BackgroundService
    {
        private readonly ILogger<SpreadStreamProvider> _logger;
        private readonly IStreamingSubscriber _subscriber;

        public SpreadStreamProvider(ILogger<SpreadStreamProvider> logger, IStreamingSubscriber subscriber)
        {
            _logger = logger;
            _subscriber = subscriber;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return _subscriber.SubscribeAsync<CurrencyPairSpread>("SpreadFeed", (spread) =>
            {
                _logger.LogInformation($"Pricing received new spread for {spread.Pair} with bid {spread.Bid:F} and ask {spread.Ask:F} at {spread.Timestamp}");
            });
        }
    }
}
