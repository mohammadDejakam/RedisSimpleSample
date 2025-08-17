using StackExchange.Redis;

namespace IRedisCacheService.Backend.RedisKit.Abstractions;

public interface IRedisRepository
{
    Task<bool> SetAsync<T>(
        string key,
        T value,
        TimeSpan? expiry = null,
        When when = When.Always); 

    Task<T?> GetAsync<T>(string key);

    Task<bool> ExistsAsync(string key);

    Task<bool> RemoveAsync(string key);

    Task<bool> UpdateAsync<T>(
        string key,
        Func<T?, T> updater,
        TimeSpan? expiry = null,
        bool createIfMissing = false,
        TimeSpan? lockExpiry = null);
}