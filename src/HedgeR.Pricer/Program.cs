using HedgeR.Shared.Swagger;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRedis(builder.Configuration);

var app = builder.Build();

app.MapGet("/", () => "HedgeR Pricer !");

app.Run();
