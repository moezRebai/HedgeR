using HedgeR.Shared.Mongo;
using HedgeR.Shared.Redis.Streaming;
using HedgeR.Shared.Serializer;
using HedgeR.Shared.Streaming;
using HedgeR.Shared.Swagger;
using HedgeR.Spread.BackgroundServices;
using HedgeR.Spread.Entities;
using HedgeR.Spread.Options;

var builder = WebApplication.CreateBuilder(args);

AddServices(builder.Services, builder.Configuration);

var app = builder.Build();
app.UseCustomSwagger();

app.MapGet("/", () => "Hello HedgeR Spread !")
.WithName("GreetingSpread").WithTags("Getters");

app.Run();

void AddServices(IServiceCollection services, IConfiguration configuration)
{
    services.AddHostedService<SpreadStreamProvider>();
    services.AddSingleton<SpreadCalculator>();
    services.Configure<SpreadOptions>(configuration.GetSection(nameof(SpreadOptions)));
    services.AddSerializer();
    services.AddSwagger();

    services.AddStreaming()
             .AddRedis(builder.Configuration)
             .AddRedisStreaming();

    services.AddMongo(builder.Configuration)
            .AddMongoRepository<CurrencyPair>("CurrrencyPair");
}
