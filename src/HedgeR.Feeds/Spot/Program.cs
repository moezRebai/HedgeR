using HedgeR.Shared.Redis.Streaming;
using HedgeR.Shared.Serializer;
using HedgeR.Shared.Streaming;
using HedgeR.Shared.Swagger;
using HedgeR.Spot.Requests;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<SpotRequestChannel>();
builder.Services.AddHostedService<CurrencyPairSpotProviderService>();
builder.Services.AddSingleton<ICurrencyPairSpotProvider, CurrencyPairSpotProvider>();
builder.Services.AddSerializer();
builder.Services.AddSwagger();
builder.Services.AddRedis(builder.Configuration);
builder.Services.AddStreaming();
builder.Services.AddRedisStreaming();

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
