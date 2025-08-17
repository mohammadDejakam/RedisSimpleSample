using IRedisCacheService.Backend.RedisKit;
using IRedisCacheService.Backend.RedisKit.Abstractions;
using StackExchange.Redis;

namespace RedisSimpleSample;

public static class RedisKitServiceCollectionExtensions
{
    public static IServiceCollection AddRedisKit(
        this IServiceCollection services,
        string configuration,
        int database = -1)
    {
        services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(configuration));
        services.AddSingleton<IRedisRepository>(sp =>
        {
            var mux = sp.GetRequiredService<IConnectionMultiplexer>();
            return new RedisRepository(mux, database);
        });

        return services;
    }
}