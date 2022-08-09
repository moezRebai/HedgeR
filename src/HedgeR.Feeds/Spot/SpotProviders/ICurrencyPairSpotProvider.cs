
internal interface ICurrencyPairSpotProvider
{
    Task StartAsync(int frequency);

    Task StopAsync();
}
