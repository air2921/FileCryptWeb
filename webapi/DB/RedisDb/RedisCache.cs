using Newtonsoft.Json;
using StackExchange.Redis;
using webapi.Interfaces.Redis;

namespace webapi.DB.RedisDb
{
    public class RedisCache : IRedisCache
    {
        private readonly IDatabase _db;

        public RedisCache(IRedisDbContext context)
        {
            _db = context.GetDatabase();
        }

        public async Task CacheData(string key, object value, TimeSpan expire)
        {
            var redisValue = await _db.StringGetAsync(key);
            if (redisValue.HasValue)
                await _db.KeyDeleteAsync(key);

            var settings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };

            await _db.StringSetAsync(key, JsonConvert.SerializeObject(value, settings), expire);
        }

        public async Task<string> GetCachedData(string key)
        {
            var redisValue = await _db.StringGetAsync(key);
            if (redisValue.HasValue)
                return redisValue!;

            return null;
        }

        public async Task DeleteCache(string key)
        {
            var value = await _db.StringGetAsync(key);
            if (value.HasValue)
                await _db.KeyDeleteAsync(key);
        }

        public async Task DeteteCacheByKeyPattern(string key)
        {
            var redisKeys = _db.Execute("KEYS", "*");
            var result = (string[])redisKeys;

            var keysContainsPattern = result.Where(str => str.Contains(key));

            foreach(var redisKey in keysContainsPattern)
            {
                await _db.KeyDeleteAsync(redisKey);
            }
        }

        public async Task DeleteRedisCache<T>(IEnumerable<T> data, string prefix, Func<T, int> getUserId) where T : class
        {
            var users = new HashSet<int>();

            foreach (var item in data)
            {
                users.Add(getUserId(item));
            }

            foreach (var user in users)
            {
                await DeteteCacheByKeyPattern($"{prefix}{user}");
            }
        }
    }
}
