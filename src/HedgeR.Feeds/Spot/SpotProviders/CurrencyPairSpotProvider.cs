﻿
internal class CurrencyPairSpotProvider : ICurrencyPairSpotProvider
{
    private readonly ILogger<CurrencyPairSpotProvider> _logger;
    private readonly IDictionary<string, decimal> _currencyPairs;
    private bool _isStopped;
    private double _updateSpotFrerquencyInSeconds = 1;
    private readonly Random _random = new();

    public CurrencyPairSpotProvider(ILogger<CurrencyPairSpotProvider> logger)
    {
        _logger = logger;

        //todo : retrieve currencyPairs from mongoDb 
        _currencyPairs = new Dictionary<string, decimal>
        {
            { "USDEUR", 0.9805m },
            { "USDGBP", 0.8215m },
            { "USDJPY", 134.52m },
            { "USDCAD", 1.281m },
            { "USDCHF", 0.9542m }
        };
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