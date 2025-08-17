using System.Text.Json;
using IRedisCacheService.Backend.RedisKit.Abstractions;
using StackExchange.Redis;

namespace IRedisCacheService.Backend.RedisKit;

 public class RedisRepository : IRedisRepository
    {
        private readonly IConnectionMultiplexer _mux;
        private readonly IDatabase _db;
        private readonly JsonSerializerOptions _jsonOptions;

        public RedisRepository(IConnectionMultiplexer mux, int database = -1, JsonSerializerOptions? jsonOptions = null)
        {
            _mux = mux ?? throw new ArgumentNullException(nameof(mux));
            _db = _mux.GetDatabase(database);
            _jsonOptions = jsonOptions ?? new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            };
        }

        public async Task<bool> SetAsync<T>(string key, T value, TimeSpan? expiry = null, When when = When.Always)
        {
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentNullException(nameof(key));
            var payload = JsonSerializer.SerializeToUtf8Bytes(value, _jsonOptions);
            return await _db.StringSetAsync(key, payload, expiry, when);
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentNullException(nameof(key));
            var val = await _db.StringGetAsync(key);
            if (val.IsNullOrEmpty) return default;

            return JsonSerializer.Deserialize<T>(val, _jsonOptions);
        }

        public Task<bool> ExistsAsync(string key)
        {
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentNullException(nameof(key));
            return _db.KeyExistsAsync(key);
        }

        public Task<bool> RemoveAsync(string key)
        {
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentNullException(nameof(key));
            return _db.KeyDeleteAsync(key);
        }

        public async Task<bool> UpdateAsync<T>(
            string key,
            Func<T?, T> updater,
            TimeSpan? expiry = null,
            bool createIfMissing = false,
            TimeSpan? lockExpiry = null)
        {
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentNullException(nameof(key));
            if (updater is null) throw new ArgumentNullException(nameof(updater));

            var lockKey = $"{key}:lock";
            var token = Guid.NewGuid().ToString("N");
            var lockTtl = lockExpiry ?? TimeSpan.FromSeconds(5);

            var acquired = await _db.LockTakeAsync(lockKey, token, lockTtl);
            if (!acquired)
                return false;

            try
            {
                T? current = default;
                var exists = await _db.KeyExistsAsync(key);
                if (!exists && !createIfMissing) return false;

                if (exists)
                {
                    var raw = await _db.StringGetAsync(key);
                    if (!raw.IsNullOrEmpty)
                        current = JsonSerializer.Deserialize<T>(raw!, _jsonOptions);
                }

                var updated = updater(current);
                var payload = JsonSerializer.SerializeToUtf8Bytes(updated, _jsonOptions);

                var when = createIfMissing ? When.Always : When.Exists;
                return await _db.StringSetAsync(key, payload, expiry, when);
            }
            finally
            {
                await _db.LockReleaseAsync(lockKey, token);
            }
        }
    }