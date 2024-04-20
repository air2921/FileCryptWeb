using application.Helpers.Localization;
using application.Services.Abstractions;
using domain.Abstractions.Data;
using domain.Exceptions;
using domain.Models;
using domain.Specifications.By_Relation_Specifications;
using domain.Specifications.Sorting_Specifications;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace application.Services.Cache_Handlers
{
    public class Storages(
        IRepository<KeyStorageModel> repository,
        IRedisCache redisCache,
        ILogger<Storages> logger) : ICacheHandler<KeyStorageModel>
    {
        public async Task<KeyStorageModel> CacheAndGet(object dataObject)
        {
            try
            {
                var storageObj = dataObject as StorageObject ?? throw new FormatException(Message.ERROR);
                var storage = new KeyStorageModel();

                var cache = await redisCache.GetCachedData(storageObj.CacheKey);
                if (cache is null)
                {
                    storage = await repository.GetByFilter(
                        new StorageByIdAndRelationSpec(storageObj.StorageId, storageObj.UserId));

                    if (storage is null)
                        return null;

                    await redisCache.CacheData(storageObj.CacheKey, storage, TimeSpan.FromMinutes(10));
                    return storage;
                }

                storage = JsonConvert.DeserializeObject<KeyStorageModel>(cache);
                if (storage is not null)
                    return storage;
                else
                    return null;
            }
            catch (EntityException)
            {
                throw;
            }
            catch (JsonException ex)
            {
                logger.LogCritical(ex.ToString(), nameof(Storages));
                throw new FormatException(Message.ERROR);
            }
        }

        public async Task<IEnumerable<KeyStorageModel>> CacheAndGetRange(object dataObject)
        {
            try
            {
                var storageObj = dataObject as StorageRangeObject ?? throw new FormatException(Message.ERROR);
                var storages = new List<KeyStorageModel>();

                var cache = await redisCache.GetCachedData(storageObj.CacheKey);
                if (cache is null)
                {
                    storages = (List<KeyStorageModel>)await repository.GetAll(
                         new StoragesSortSpec(storageObj.UserId, storageObj.Skip, storageObj.Count, storageObj.ByDesc));

                    await redisCache.CacheData(storageObj.CacheKey, storages, TimeSpan.FromMinutes(5));
                    return storages;
                }

                storages = JsonConvert.DeserializeObject<List<KeyStorageModel>>(cache);
                if (storages is not null)
                    return storages;
                else
                    throw new FormatException(Message.ERROR);
            }
            catch (EntityException)
            {
                throw;
            }
            catch (JsonException ex)
            {
                logger.LogCritical(ex.ToString(), nameof(Storages));
                throw new FormatException(Message.ERROR);
            }
        }
    }

    public record class StorageObject(string CacheKey, int UserId, int StorageId);
    public record class StorageRangeObject(string CacheKey, int UserId, int Skip, int Count, bool ByDesc);
}
