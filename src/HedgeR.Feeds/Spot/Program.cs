var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHostedService<CurrencyPairSpotProviderService>();
builder.Services.AddSingleton<ICurrencyPairSpotGenerator, CurrencyPairSpotGenerator>();

var app = builder.Build();

app.MapGet("/", () => "HedgeR Spot !");

app.Run();
