var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => "HedgeR Pricer !");

app.Run();
