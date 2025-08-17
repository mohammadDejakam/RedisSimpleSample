using IRedisCacheService.Backend.RedisKit.Abstractions;
using RedisSimpleSample;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRedisKit(builder.Configuration.GetConnectionString("Redis")!);

var app = builder.Build();

app.MapGet("/demo", async (IRedisRepository redis) =>
{
    var key = "users:42";
    var user = new { Id = 42, Name = "Dejoo", Email = "Dejoo@example.com" };

    await redis.SetAsync(key, user, TimeSpan.FromMinutes(30), StackExchange.Redis.When.NotExists);

    var exists = await redis.ExistsAsync(key);

    var stored = await redis.GetAsync<dynamic>(key);

    await redis.UpdateAsync<dynamic>(
        key,
        current => new { Id = current?.Id ?? 42, Name = "Dejoo", Email = current?.Email },
        expiry: TimeSpan.FromMinutes(30),
        createIfMissing: true
    );

    await redis.RemoveAsync(key);

    return Results.Ok(new { exists, stored });
});

app.Run();