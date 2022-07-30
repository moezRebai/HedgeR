
internal class CurrencyPairSpotGenerator : ICurrencyPairSpotGenerator
{
    private readonly ILogger<CurrencyPairSpotGenerator> _logger;

    public CurrencyPairSpotGenerator(ILogger<CurrencyPairSpotGenerator> logger)
    {
        this._logger = logger;
    }

    public Task Start()
    {
        throw new NotImplementedException();
    }

    public Task Stop()
    {
        throw new NotImplementedException();
    }
}