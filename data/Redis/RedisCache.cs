using domain.Abstractions.Data;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace data.Redis
{
    public class RedisCache(IRedisDbContext context) : IRedisCache
    {
        private readonly IDatabase _db = context.GetDatabase();

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

            if (result is null)
                return;

            var keysContainsPattern = result.Where(str => str.Contains(key)).ToArray();
            var partsKeyPattern = key.Split('_');

            foreach (var redisKey in keysContainsPattern)
            {
                try
                {
                    if (redisKey.Split('_')[0] == partsKeyPattern[0] && redisKey.Split('_')[1] == partsKeyPattern[1])
                        await _db.KeyDeleteAsync(redisKey);
                }
                catch (IndexOutOfRangeException)
                {
                    continue;
                }
            }
        }

        public async Task DeleteRedisCache<T>(IEnumerable<T> data, string prefix, Func<T, int> getUserId) where T : class
        {
            var users = new HashSet<int>();

            foreach (var item in data)
                users.Add(getUserId(item));

            foreach (var user in users)
                await DeteteCacheByKeyPattern($"{prefix}{user}");
        }
    }
}
