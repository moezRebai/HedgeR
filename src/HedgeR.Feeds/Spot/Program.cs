var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => "HedgeR Spot !");

app.Run();
