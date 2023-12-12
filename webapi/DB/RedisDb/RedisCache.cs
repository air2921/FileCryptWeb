using webapi.Exceptions;
using webapi.Interfaces.Cryptography;
using webapi.Interfaces.Redis;
using webapi.Models;
using webapi.Services;

namespace webapi.DB.RedisDb
{
    public class RedisCache : IRedisCache
    {
        private readonly IRedisDbContext _context;
        private readonly IRedisKeys _redisKeys;
        private readonly IConfiguration _configuration;
        private readonly ILogger<RedisCache> _logger;
        private readonly IDecryptKey _decrypt;
        private readonly byte[] secretKey;

        public RedisCache(
            IRedisDbContext context,
            IRedisKeys redisKeys,
            IConfiguration configuration,
            ILogger<RedisCache> logger,
            IDecryptKey decrypt)
        {
            _context = context;
            _redisKeys = redisKeys;
            _configuration = configuration;
            _logger = logger;
            _decrypt = decrypt;
            secretKey = Convert.FromBase64String(_configuration[App.appKey]!);
        }

        public async Task<string> CacheKey(string key, Func<Task<KeyModel>> readKeyFunction)
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
                    var keys = await readKeyFunction();
                    string encryptionKey = null;

                    if (key == _redisKeys.PrivateKey)
                    {
                        encryptionKey = keys.private_key;
                    }
                    else if (key == _redisKeys.PersonalInternalKey)
                    {
                        encryptionKey = keys.person_internal_key;
                    }
                    else if (key == _redisKeys.ReceivedInternalKey)
                    {
                        encryptionKey = keys.received_internal_key;
                    }
                    else
                    {
                        throw new ArgumentException();
                    }

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

        public async Task DeleteCache(string key)
        {
            var db = _context.GetDatabase();
            var value = await db.StringGetAsync(key);
            if (value.HasValue)
                await db.KeyDeleteAsync(key);
        }
    }
}
