using HedgeR.Shared.Mongo;
using HedgeR.Shared.Redis.Streaming;
using HedgeR.Shared.Serializer;
using HedgeR.Shared.Streaming;
using HedgeR.Shared.Swagger;
using HedgeR.Spot.Entities;
using HedgeR.Spot.Requests;

var builder = WebApplication.CreateBuilder(args);

AddServices(builder.Services, builder.Configuration);

var app = builder.Build();
app.UseCustomSwagger();

app.MapGet("/", () => "Hello HedgeR Spot !")
.WithName("GreetingSpotFeeder").WithTags("Getters"); ;

app.MapPost("/spot/start/{frequency}", async (int frequency, SpotRequestChannel channel) =>
{
    await channel.Requests.Writer.WriteAsync(new RequestStartSpotFeeder() { Frequency = frequency});
    return Results.Ok();

}).Produces(StatusCodes.Status200OK)
.WithName("StartSpotFeeder").WithTags("Setters");

app.MapPost("/spot/stop", async (SpotRequestChannel channel) => 
{
    await channel.Requests.Writer.WriteAsync(new RequestStopSpotFeeder());
    return Results.Ok();

}).Produces(StatusCodes.Status200OK)
.WithName("StopSpotFeeder").WithTags("Setters");


app.Run();

void AddServices(IServiceCollection services, IConfiguration configuration)
{
   services.AddSingleton<SpotRequestChannel>();
   services.AddHostedService<CurrencyPairSpotProviderService>();
   services.AddSingleton<ICurrencyPairSpotProvider, CurrencyPairSpotProvider>();
   services.AddSerializer();
   services.AddSwagger();

   services.AddStreaming()
            .AddRedis(builder.Configuration)
             .AddRedisStreaming();

    services.AddMongo(builder.Configuration)
            .AddMongoRepository<CurrencyPair>("CurrrencyPair");
}