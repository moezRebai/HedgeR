var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => "HedgeR Price Aggregator !");

app.Run();
