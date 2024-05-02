using domain.Abstractions.Data;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace data_access.Redis
{
    public class RedisCache(IRedisDbContext context, ILogger<RedisCache> logger) : IRedisCache
    {
        private readonly IDatabase _db = context.GetDatabase();

        public async Task CacheData(string key, object value, TimeSpan expires)
        {
            try
            {
                var redisValue = await _db.StringGetAsync(key);
                if (redisValue.HasValue)
                    await _db.KeyDeleteAsync(key);

                var settings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                };

                var dataToSave = JsonConvert.SerializeObject(value, settings);
                await _db.StringSetAsync(key, dataToSave, expires);

                // FormatException at this line, idk whi it happens.
                // No one variable int this log not null.
                // Maybe it because of '\n' in log message
                logger.LogInformation($"Request to save data in redis cluster\n" +
                    $"Information about saved data:\n" +
                    $"Key: {key}\nValue: {dataToSave ?? "NULL"}\nExpires: {expires}");
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex.ToString());
            }
        }

        public async Task<string> GetCachedData(string key)
        {
            try
            {
                var value = await _db.StringGetAsync(key);

                // FormatException at this line, idk whi it happens.
                // No one variable int this log not null.
                // Maybe it because of '\n' in log message
                logger.LogInformation($"Request to get data from redis cluster\n" +
                    $"Information about the requested data:\n" +
                    $"Key: {key}\nValue: {GetStringValue(value)}");

                return value;
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex.ToString());
                return default;
            }
        }

        public async Task DeleteCache(string key)
        {
            try
            {
                var value = await _db.StringGetAsync(key);
                if (value.HasValue)
                    await _db.KeyDeleteAsync(key);

                // FormatException at this line, idk whi it happens.
                // No one variable int this log not null.
                // Maybe it because of '\n' in log message
                logger.LogInformation($"Request to delete data by key from redis cluster\n" +
                    $"Information about deleted data\n" +
                    $"Key: {key}\nValue: {GetStringValue(value)}");
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex.ToString());
            }
        }

        public async Task DeteteCacheByKeyPattern(string pattern)
        {
            try
            {
                var redisKeys = await _db.ExecuteAsync("KEYS", "*");
                var result = redisKeys.ToDictionary().Keys;

                if (result is null)
                    return;

                //FormatException at this line, idk whi it happens.
                // No one variable int this log not null.
                // Maybe it because of '\n' in log message
                logger.LogInformation($"Request to delete data by pattern from redis cluster\n" +
                    $"Pattern: {pattern}");

                var deletedKeys = new List<string>();
                var partsOfPattern = pattern.Split('_');

                var tasks = result.Where(x => x.StartsWith(pattern))
                    .ToHashSet()
                    .Select(async match =>
                    {
                        if (partsOfPattern[0].Equals(match[0]) && partsOfPattern[1].Equals(match[1]))
                        {
                            _db.KeyDeleteAsync(match);
                            deletedKeys.Add(match);
                        }
                    });

                await Task.WhenAll(tasks);

                logger.LogInformation(string.Join(", ", deletedKeys));
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex.ToString());
            }
        }

        public async Task DeleteRedisCache<T>(IEnumerable<T> data, string prefix, Func<T, int> getUserId) where T : class
        {
            try
            {
                var users = new HashSet<int>();
                var deletedKeys = new List<string>();

                foreach (var item in data)
                    users.Add(getUserId(item));

                foreach (var user in users)
                {
                    var key = $"{prefix}{user}";
                    await DeteteCacheByKeyPattern(key);
                    deletedKeys.Add(key);
                }

                logger.LogInformation(string.Join(", ", deletedKeys));
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex.ToString());
            }
        }

        private static string GetStringValue(RedisValue? redisValue)
        {
            string? dataToReturn = null;

            if (redisValue.HasValue)
                dataToReturn = redisValue;

            var temp = dataToReturn is not null ? dataToReturn : "Requested value is null";
            return temp;
        }
    }
}
