﻿using Newtonsoft.Json;
using StackExchange.Redis;
using webapi.Helpers;
using webapi.Interfaces;
using webapi.Interfaces.Cryptography;
using webapi.Interfaces.Redis;
using webapi.Interfaces.Services;
using webapi.Localization;
using webapi.Models;

namespace webapi.DB.RedisDb
{
    public class RedisCache : IRedisCache
    {
        private readonly IRepository<KeyModel> _keyRepository;
        private readonly IRedisDbContext _context;
        private readonly IRedisKeys _redisKeys;
        private readonly IConfiguration _configuration;
        private readonly ICypherKey _decryptKey;
        private readonly byte[] secretKey;
        private readonly IDatabase _db;

        public RedisCache(
            IRepository<KeyModel> keyRepository,
            IRedisDbContext context,
            IRedisKeys redisKeys,
            IConfiguration configuration,
            IEnumerable<ICypherKey> cypherKeys,
            IImplementationFinder implementationFinder)
        {
            _keyRepository = keyRepository;
            _context = context;
            _redisKeys = redisKeys;
            _configuration = configuration;
            _decryptKey = implementationFinder.GetImplementationByKey(cypherKeys, ImplementationKey.DECRYPT_KEY);
            secretKey = Convert.FromBase64String(_configuration[App.ENCRYPTION_KEY]!);
            _db = context.GetDatabase();
        }

        public async Task<string> CacheKey(string key, int userId)
        {
            try
            {
                var value = await _db.StringGetAsync(key);

                if (value.HasValue)
                    return value!;

                var keys = await _keyRepository.GetByFilter(query => query.Where(k => k.user_id.Equals(userId)));
                if (keys is null)
                    throw new ArgumentNullException();

                string? encryptionKey = null;

                if (key == _redisKeys.PrivateKey)
                {
                    if (string.IsNullOrEmpty(keys.private_key))
                        throw new ArgumentNullException(Message.NOT_FOUND);

                    encryptionKey = keys.private_key;
                }
                else if (key == _redisKeys.InternalKey)
                {
                    if (string.IsNullOrEmpty(keys.internal_key))
                        throw new ArgumentNullException(Message.NOT_FOUND);

                    encryptionKey = keys.internal_key;
                }
                else if (key == _redisKeys.ReceivedKey)
                {
                    if (string.IsNullOrEmpty(keys.received_key))
                        throw new ArgumentNullException(Message.NOT_FOUND);

                    encryptionKey = keys.received_key;
                }
                else
                {
                    throw new ArgumentException();
                }

                var decryptedKey = await _decryptKey.CypherKeyAsync(encryptionKey!, secretKey);
                await _db.StringSetAsync(key, decryptedKey, TimeSpan.FromMinutes(10));

                return decryptedKey;
            }
            catch (OperationCanceledException ex)
            {
                throw new ArgumentNullException(ex.Message);
            }
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
