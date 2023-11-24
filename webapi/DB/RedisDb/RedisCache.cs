using webapi.Exceptions;
using webapi.Interfaces.Cryptography;
using webapi.Interfaces.Redis;

namespace webapi.DB.RedisDb
{
    public class RedisCache : IRedisCache
    {
        private readonly IRedisDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<RedisCache> _logger;
        private readonly IDecryptKey _decrypt;
        private readonly byte[] secretKey;

        public RedisCache(
            IRedisDbContext context,
            IConfiguration configuration,
            ILogger<RedisCache> logger,
            IDecryptKey decrypt)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
            _decrypt = decrypt;
            secretKey = Convert.FromBase64String(_configuration["FileCryptKey"]!);
        }

        public async Task<string> CacheKey(string key, Func<Task<string>> readKeyFunction)
        {
            try
            {
                var db = _context.GetDatabase();
                var value = await db.StringGetAsync(key);
                if (value.HasValue)
                {
                    var encryptionKey = await _decrypt.DecryptionKeyAsync(value, secretKey);

                    return encryptionKey;
                }
                else
                {
                    var encryptionKey = await readKeyFunction();
                    await db.StringSetAsync(key, encryptionKey, TimeSpan.FromMinutes(30));

                    var decryptedKey = await _decrypt.DecryptionKeyAsync(encryptionKey, secretKey);
                    return decryptedKey;
                }
            }
            catch (UserException)
            {
                throw;
            }
            catch (KeyException)
            {
                throw;
            }
        }

        public async Task CacheData(string key, string value, TimeSpan expire)
        {
            var db = _context.GetDatabase();
            var redisValue = await db.StringGetAsync(key);
            if (redisValue.HasValue)
            {
                await db.KeyDeleteAsync(key);
            }

            await db.StringSetAsync(key, value, expire);
        }

        public async Task<string> GetCachedData(string key)
        {
            var db = _context.GetDatabase();
            var redisValue = await db.StringGetAsync(key)!;
            if (redisValue.HasValue)
                return redisValue!;

            throw new KeyNotFoundException("Data was not found");
        }

        public async Task<string> CacheDbData(string key, Func<Task<string>> read, TimeSpan expire)
        {
            try
            {
                var db = _context.GetDatabase();
                var redisValue = await db.StringGetAsync(key);
                if (redisValue.HasValue)
                {
                    return redisValue;
                }

                var data = await read();
                await db.StringSetAsync(key, data, expire);

                return data;
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex.ToString(), nameof(CacheDbData));
                throw;
            }
        }

        public async Task DeleteCache(string key)
        {
            var db = _context.GetDatabase();
            var value = await db.StringGetAsync(key);
            if (value.HasValue)
                await db.KeyDeleteAsync(key);
        }
    }
}
