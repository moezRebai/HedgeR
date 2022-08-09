
internal interface ICurrencyPairSpotProvider
{
    IAsyncEnumerable<CurrencyPairSpot> StartStreamingAsync(int frequency);

    Task StopAsync();
}
