using HedgeR.Shared.Entities;
using HedgeR.Shared.Streaming;
using HedgeR.Spread.Entities;

namespace HedgeR.Spread.BackgroundServices
{
    public class SpreadStreamProvider : BackgroundService
    {
        private readonly ILogger<SpreadStreamProvider> _logger;
        private readonly IStreamingSubscriber _subscriber;
        private readonly IStreamingPublisher _publisher;
        private readonly SpreadCalculator _spreadCalculator;
        private readonly IRepository<CurrencyPair> _repository;
        private IDictionary<string, decimal> _spreadBpsByPair = new Dictionary<string, decimal>();

        public SpreadStreamProvider(ILogger<SpreadStreamProvider> logger, IStreamingSubscriber subscriber,
            IStreamingPublisher publisher, SpreadCalculator spreadCalculator, IRepository<CurrencyPair> repository)
        {
            _logger = logger;
            _subscriber = subscriber;
            _publisher = publisher;
            _spreadCalculator = spreadCalculator;
            _repository = repository;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var currencyPairs = await _repository.GetAllAsync();

            _spreadBpsByPair = currencyPairs.ToDictionary(c => c.Name, c => c.SpreadBps);

            await _subscriber.SubscribeAsync<CurrencyPairSpot>("SpotFeed", (spot) =>
            {
                var spreadBps = _spreadBpsByPair.TryGetValue(spot.CurrencyPair, out var bps) ? bps : 0;

                var currencyPairSpread = _spreadCalculator.Calculate(spot.CurrencyPair, spot.Value, spreadBps, spot.Timestamp);

                _logger.LogInformation($"CurrencyPair {currencyPairSpread.Pair} has a new spread : bid {currencyPairSpread.Bid:F} ask {currencyPairSpread.Ask:F} at {currencyPairSpread.Timestamp}");

                _ = _publisher.PublishAsync("SpreadFeed", currencyPairSpread);
            });
        }
    }
}
