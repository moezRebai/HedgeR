using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using System;

namespace HedgeR.Shared.Swagger
{
    public static class RedisExtenisons
    {
        public static IServiceCollection AddRedis(this IServiceCollection services, IConfiguration configuration)
        {
            var section = configuration.GetRequiredSection("redis");
            var options = new RedisOptions();
            section.Bind(options);
            services.Configure<RedisOptions>(section);
            //TODO : Add retry policy to reconnect redis if it's down
            services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(options.ConnectionString));

            return services;
        }
    }

    public class RedisOptions
    {
        public string? ConnectionString { get; set; }
    }

}
