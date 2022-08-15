
using HedgeR.Shared.Entities;
using HedgeR.Spot.Entities;

internal class CurrencyPairSpotProvider : ICurrencyPairSpotProvider
{
    private readonly ILogger<CurrencyPairSpotProvider> _logger;
    private readonly IRepository<CurrencyPair> _repository;
    private readonly IDictionary<string, decimal> _currencyPairs;
    private bool _isStopped;
    private double _updateSpotFrerquencyInSeconds = 1;
    private readonly Random _random = new();

    public CurrencyPairSpotProvider(ILogger<CurrencyPairSpotProvider> logger, IRepository<CurrencyPair> repository)
    {
        _logger = logger;

        _repository = repository;

        _currencyPairs = GetCurencyPairs();
    }

    private IDictionary<string, decimal> GetCurencyPairs()
    {
        var curencyPairs = _repository.GetAllAsync().Result;

        return curencyPairs.ToDictionary(c => c.Name, c => c.DefaultValue ?? 0);
    }

    public async IAsyncEnumerable<CurrencyPairSpot> StartStreamingAsync(int frequency)
    {
        _isStopped = false;

        _updateSpotFrerquencyInSeconds = frequency > 0 ? frequency : _updateSpotFrerquencyInSeconds;

        _logger.LogInformation($"Start Spot Feeder with update frequency : {_updateSpotFrerquencyInSeconds}");


        while (!_isStopped)
        {
            foreach (var (symbol, value) in _currencyPairs)
            {
                if (_isStopped)
                {
                    yield break;
                }

                var latestSpotValue = GetNextSpotValue(value);

                _currencyPairs[symbol] = latestSpotValue;

                var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

                _logger.LogInformation($"CurrencyPair {symbol} has a new spot value : {latestSpotValue:F} at {timestamp}");

                var currencyPairSpot = new CurrencyPairSpot(symbol, latestSpotValue, timestamp);

                yield return currencyPairSpot;

                await Task.Delay(TimeSpan.FromSeconds(_updateSpotFrerquencyInSeconds));
            }
        }
    }

    public Task StopAsync()
    {
        _isStopped = true;
        return Task.CompletedTask;
    }

    private decimal GetNextSpotValue(decimal value)
    {
        var sign = _random.Next(0, 2) == 0 ? -1 : 1;
        var tick = (_random.NextDouble() * sign) / 20;
        return value + (decimal)tick;
    }
}