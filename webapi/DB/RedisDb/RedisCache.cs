using Newtonsoft.Json;
using webapi.Exceptions;
using webapi.Interfaces.Cryptography;
using webapi.Interfaces.Redis;
using webapi.Localization.Exceptions;
using webapi.Models;
using webapi.Services;

namespace webapi.DB.RedisDb
{
    public class RedisCache : IRedisCache
    {
        private readonly IRedisDbContext _context;
        private readonly IRedisKeys _redisKeys;
        private readonly IConfiguration _configuration;
        private readonly IDecryptKey _decrypt;
        private readonly byte[] secretKey;

        public RedisCache(
            IRedisDbContext context,
            IRedisKeys redisKeys,
            IConfiguration configuration,
            IDecryptKey decrypt)
        {
            _context = context;
            _redisKeys = redisKeys;
            _configuration = configuration;
            _decrypt = decrypt;
            secretKey = Convert.FromBase64String(_configuration[App.ENCRYPTION_KEY]!);
        }

        public async Task<string> CacheKey(string key, Func<Task<KeyModel>> readKeyFunction)
        {
            try
            {
                var db = _context.GetDatabase();
                var value = await db.StringGetAsync(key);

                if (value.HasValue)
                {
                    var encryptionKey = await _decrypt.DecryptionKeyAsync(value!, secretKey);

                    return encryptionKey;
                }
                else
                {
                    var keys = await readKeyFunction();

                    string? encryptionKey = null;

                    if (key == _redisKeys.PrivateKey)
                    {
                        if (string.IsNullOrEmpty(keys.internal_key))
                            throw new KeyException(ExceptionKeyMessages.KeyNotFound);

                        encryptionKey = keys.private_key;
                    }
                    else if (key == _redisKeys.InternalKey)
                    {
                        if (string.IsNullOrEmpty(keys.internal_key))
                            throw new KeyException(ExceptionKeyMessages.KeyNotFound);

                        encryptionKey = keys.internal_key;
                    }
                    else if (key == _redisKeys.ReceivedKey)
                    {
                        if (string.IsNullOrEmpty(keys.received_key))
                            throw new KeyException(ExceptionKeyMessages.KeyNotFound);

                        encryptionKey = keys.received_key;
                    }
                    else
                    {
                        throw new ArgumentException();
                    }

                    await db.StringSetAsync(key, encryptionKey, TimeSpan.FromMinutes(30));

                    var decryptedKey = await _decrypt.DecryptionKeyAsync(encryptionKey!, secretKey);
                    return decryptedKey;
                }
            }
            catch (UserException)
            {
                throw;
            }
        }

        public async Task CacheData(string key, object value, TimeSpan expire)
        {
            var db = _context.GetDatabase();
            var redisValue = await db.StringGetAsync(key);
            if (redisValue.HasValue)
                await db.KeyDeleteAsync(key);

            var settings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };

            await db.StringSetAsync(key, JsonConvert.SerializeObject(value, settings), expire);
        }

        public async Task<string> GetCachedData(string key)
        {
            var db = _context.GetDatabase();
            var redisValue = await db.StringGetAsync(key);
            if (redisValue.HasValue)
                return redisValue!;

            return null;
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
