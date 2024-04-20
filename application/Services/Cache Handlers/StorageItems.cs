using application.Abstractions.Services.TP_Services;
using application.Helpers;
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
    public class StorageItems(
        IRepository<KeyStorageItemModel> repository,
        IRedisCache redisCache,
        ICacheHandler<KeyStorageModel> cacheHandler,
        IHashUtility hashUtility,
        ILogger<StorageItems> logger) : ICacheHandler<KeyStorageItemModel>
    {
        public async Task<KeyStorageItemModel> CacheAndGet(object dataObject)
        {
            try
            {
                var itemObj = dataObject as StorageItemObject ?? throw new FormatException(Message.ERROR);
                var item = new KeyStorageItemModel();

                if (!await IsAllowAccess(itemObj.UserId, itemObj.StorageId, itemObj.AccessCode))
                    throw new EntityException(Message.INCORRECT);

                var cache = await redisCache.GetCachedData(itemObj.CacheKey);
                if (cache is null)
                {
                    if (!await IsAllowAccess(itemObj.UserId, itemObj.StorageId, itemObj.AccessCode))
                        throw new EntityException(Message.INCORRECT);

                    item = await repository.GetByFilter(
                        new StorageKeyByIdAndRelationSpec(itemObj.StorageItemId, itemObj.StorageId));

                    if (item is null)
                        return null;

                    await redisCache.CacheData(itemObj.CacheKey, item, TimeSpan.FromMinutes(10));
                    return item;
                }

                item = JsonConvert.DeserializeObject<KeyStorageItemModel>(cache);
                if (item is not null)
                    return item;
                else
                    return null;
            }
            catch (EntityException)
            {
                throw;
            }
            catch (JsonException ex)
            {
                logger.LogCritical(ex.ToString(), nameof(StorageItems));
                throw new FormatException(Message.ERROR);
            }
        }

        public async Task<IEnumerable<KeyStorageItemModel>> CacheAndGetRange(object dataObject)
        {
            try
            {
                var itemObj = dataObject as StorageItemRangeObject ?? throw new FormatException(Message.ERROR);
                var items = new List<KeyStorageItemModel>();

                if (!await IsAllowAccess(itemObj.UserId, itemObj.StorageId, itemObj.AccessCode))
                    throw new EntityException(Message.INCORRECT);

                var cache = await redisCache.GetCachedData(itemObj.CacheKey);
                if (cache is null)
                {
                    if (!await IsAllowAccess(itemObj.UserId, itemObj.StorageId, itemObj.AccessCode))
                        throw new EntityException(Message.INCORRECT);

                    items = (List<KeyStorageItemModel>)await repository.GetAll(
                        new StorageItemsSortSpec(itemObj.StorageId, itemObj.Skip, itemObj.Count, itemObj.ByDesc));

                    await redisCache.CacheData(itemObj.CacheKey, items, TimeSpan.FromMinutes(5));
                    return items;
                }

                items = JsonConvert.DeserializeObject<List<KeyStorageItemModel>>(cache);
                if (items is not null)
                    return items;
                else
                    throw new FormatException(Message.ERROR);
            }
            catch (EntityException)
            {
                throw;
            }
            catch (JsonException ex)
            {
                logger.LogCritical(ex.ToString(), nameof(StorageItems));
                throw new FormatException(Message.ERROR);
            }
        }

        private async Task<bool> IsAllowAccess(int userId, int storageId, string accessCode)
        {
            try
            {
                var cacheKey = $"{ImmutableData.STORAGES_PREFIX}{userId}_{storageId}";
                var storage = await cacheHandler.CacheAndGet(
                    new StorageObject(cacheKey, userId, storageId)) ?? throw new EntityException(Message.NOT_FOUND);

                return hashUtility.Verify(accessCode, storage.access_code);
            }
            catch (FormatException ex)
            {
                throw new EntityException(ex.Message);
            }
            catch (EntityException)
            {
                throw;
            }
        }
    }

    public record class StorageItemObject(string CacheKey, int UserId, int StorageItemId, int StorageId, string AccessCode);
    public record class StorageItemRangeObject(string CacheKey, int UserId, int StorageId,
        int Skip, int Count, bool ByDesc, string AccessCode);
}
