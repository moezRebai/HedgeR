
internal interface ICurrencyPairSpotGenerator
{
    Task StartAsync(int frequency);

    Task StopAsync();
}
