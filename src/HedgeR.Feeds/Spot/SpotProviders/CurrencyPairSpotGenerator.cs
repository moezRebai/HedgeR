
internal class CurrencyPairSpotGenerator : ICurrencyPairSpotGenerator
{
    private readonly ILogger<CurrencyPairSpotGenerator> _logger;
    private readonly IDictionary<string, decimal> _currencyPairs;
    private bool _isStopped;
    private double UpdateSpotFrerquencyInSeconds = 2;
    private readonly Random _random = new();

    public CurrencyPairSpotGenerator(ILogger<CurrencyPairSpotGenerator> logger)
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

    public async Task StartAsync()
    {
        _isStopped = false;

        while (!_isStopped)
        {
            foreach (var (symbol, value) in _currencyPairs)
            {
                if (_isStopped)
                {
                    return;
                }

                var latestSpotValue = GetNextSpotValue(value);

                _currencyPairs[symbol] = latestSpotValue;

                var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

                _logger.LogInformation($"CurrencyPair {symbol} has a new spot value : {latestSpotValue:F} at {timestamp}");

                var currencyPairSpot = new CurrencyPairSpot(symbol, latestSpotValue, timestamp);

                await Task.Delay(TimeSpan.FromSeconds(UpdateSpotFrerquencyInSeconds));
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