using application.Helpers;
using application.Helpers.Localization;
using application.Cache_Handlers;
using domain.Abstractions.Data;
using domain.Exceptions;
using domain.Models;
using application.Abstractions.TP_Services;
using application.Abstractions.Endpoints.Core;

namespace application.Master_Services.Core
{
    public class StorageItemsService(
        IRepository<KeyStorageItemModel> repository,
        ICacheHandler<KeyStorageItemModel> itemCacheHandler,
        ICacheHandler<KeyStorageModel> storageCacheHandler,
        IRedisCache redisCache,
        IHashUtility hashUtility) : IStorageItemService
    {
        public async Task<Response> Add(int userId, int storageId, string code, string name, string value)
        {
            try
            {
                var response = await VerifyAccess(userId, storageId, code);
                if (response.Status != 200)
                    return response;

                await repository.Add(new KeyStorageItemModel
                {
                    storage_id = storageId,
                    key_name = name,
                    key_value = value,
                    created_at = DateTime.UtcNow
                });
                await redisCache.DeleteCache($"{ImmutableData.STORAGE_ITEMS_PREFIX}{userId}");

                return new Response(true) { Status = 201, Message = Message.CREATED };
            }
            catch (EntityException ex)
            {
                return new Response { Status = 500, Message = ex.Message };
            }
            catch (FormatException ex)
            {
                return new Response { Status = 500, Message = ex.Message };
            }
        }

        public async Task<Response> GetOne(int userId, int storageId, int keyId, string code)
        {
            try
            {
                var cacheKey = $"{ImmutableData.STORAGE_ITEMS_PREFIX}{userId}_{keyId}";
                return new Response(true)
                {
                    Status = 200,
                    ObjectData = await itemCacheHandler.CacheAndGet(
                        new StorageItemObject(cacheKey, userId, keyId, storageId, code))
                };
            }
            catch (EntityException ex)
            {
                return new Response { Status = 500, Message = ex.Message };
            }
            catch (FormatException ex)
            {
                return new Response { Status = 500, Message = ex.Message };
            }
        }

        public async Task<Response> GetRange(int userId, int storageId, int skip,
            int count, bool byDesc, string code)
        {
            try
            {
                var cacheKey = $"{ImmutableData.STORAGE_ITEMS_PREFIX}{userId}_{storageId}_{skip}_{count}_{byDesc}";
                return new Response(true)
                {
                    Status = 200,
                    ObjectData = await itemCacheHandler.CacheAndGetRange(
                        new StorageItemRangeObject(cacheKey, userId, storageId, skip, count, byDesc, code))
                };
            }
            catch (EntityException ex)
            {
                return new Response { Status = 500, Message = ex.Message };
            }
            catch (FormatException ex)
            {
                return new Response { Status = 500, Message = ex.Message };
            }
        }

        public async Task<Response> DeleteOne(int userId, int storageId, int keyId, string code)
        {
            try
            {
                var response = await VerifyAccess(userId, storageId, code);
                if (response.Status != 200)
                    return response;

                await repository.Delete(keyId);
                await redisCache.DeleteCache($"{ImmutableData.STORAGE_ITEMS_PREFIX}{userId}");

                return new Response(true) { Status = 204 };
            }
            catch (EntityException ex)
            {
                return new Response { Status = 500, Message = ex.Message };
            }
            catch (FormatException ex)
            {
                return new Response { Status = 500, Message = ex.Message };
            }
        }

        private async Task<Response> VerifyAccess(int userId, int storageId, string code)
        {
            try
            {
                var cacheKey = $"{ImmutableData.STORAGES_PREFIX}{userId}_{storageId}";
                var storage = await storageCacheHandler.CacheAndGet(new StorageObject(cacheKey, userId, storageId));
                if (storage is null)
                    return new Response { Status = 404, Message = Message.NOT_FOUND };

                if (!hashUtility.Verify(code, storage.access_code))
                    return new Response { Status = 403, Message = Message.INCORRECT };

                return new Response(true) { Status = 200 };
            }
            catch (EntityException)
            {
                throw;
            }
            catch (FormatException)
            {
                throw;
            }
        }
    }
}
