﻿using Newtonsoft.Json;
using webapi.Interfaces;
using webapi.Interfaces.Redis;
using webapi.Localization;
using webapi.Models;

namespace webapi.Services.Core.Data_Handlers
{
    public class Keys(
        IRepository<KeyModel> keyRepository,
        IRedisCache redisCache,
        ILogger<Keys> logger) : ICacheHandler<KeyModel>
    {
        public async Task<KeyModel> CacheAndGet(object dataObject)
        {
            try
            {
                var keys = new KeyModel();
                var keyObj = dataObject as KeyObject ?? throw new FormatException(Message.ERROR);
                var cache = await redisCache.GetCachedData(keyObj.CacheKey);
                if (cache is null)
                {
                    keys = await keyRepository.GetByFilter(query => query.Where(k => k.user_id.Equals(keyObj.UserId)));

                    if (keys is null)
                        return null;

                    await redisCache.CacheData(keyObj.CacheKey, keys, TimeSpan.FromMinutes(10));
                    return keys;
                }

                keys = JsonConvert.DeserializeObject<KeyModel>(cache);
                if (keys is null)
                    throw new FormatException(Message.ERROR);
                else
                    return keys;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (FormatException)
            {
                return null;
            }
            catch (JsonException ex)
            {
                logger.LogCritical(ex.ToString(), nameof(CacheAndGet));
                throw new FormatException(Message.ERROR);
            }
        }

        public Task<IEnumerable<KeyModel>> CacheAndGetRange(object dataObject) => throw new NotImplementedException();
    }

    public record class KeyObject(string CacheKey, int UserId);
}
