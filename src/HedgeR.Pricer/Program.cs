using HedgeR.Pricer.StreamProviders;
using HedgeR.Shared.Redis.Streaming;
using HedgeR.Shared.Serializer;
using HedgeR.Shared.Streaming;
using HedgeR.Shared.Swagger;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHostedService<SpotStreamProvider>();
builder.Services.AddSwagger();
builder.Services.AddSerializer();
builder.Services.AddRedis(builder.Configuration);
builder.Services.AddStreaming();
builder.Services.AddRedisStreaming();

var app = builder.Build();

app.MapGet("/", () => "HedgeR Pricer !");

app.Run();
