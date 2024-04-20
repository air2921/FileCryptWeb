using application.Abstractions.Services.TP_Services;
using application.Helpers;
using application.Helpers.Localization;
using application.Cache_Handlers;
using domain.Abstractions.Data;
using domain.Exceptions;
using domain.Models;
using domain.Specifications.By_Relation_Specifications;

namespace application.Master_Services.Core
{
    public class StoragesService(
        IRepository<KeyStorageModel> repository,
        ICacheHandler<KeyStorageModel> cacheHandler,
        IRedisCache redisCache,
        IHashUtility hashUtility)
    {
        public async Task<Response> Add(string storageName, string accessCode, int userId)
        {
            try
            {
                await repository.Add(new KeyStorageModel
                {
                    storage_name = storageName,
                    access_code = hashUtility.Hash(accessCode),
                    user_id = userId,
                    last_time_modified = DateTime.UtcNow
                });

                await redisCache.DeteteCacheByKeyPattern($"{ImmutableData.STORAGES_PREFIX}{userId}");
                return new Response { Status = 201, Message = Message.CREATED };
            }
            catch (EntityException ex)
            {
                return new Response { Status = 500, Message = ex.Message };
            }
        }

        public async Task<Response> GetOne(int storageId, int userId)
        {
            try
            {
                var cacheKey = $"{ImmutableData.STORAGES_PREFIX}{userId}_{storageId}";
                var storage = await cacheHandler.CacheAndGet(new StorageObject(cacheKey, userId, storageId));
                if (storage is null)
                    return new Response { Status = 404, Message = Message.NOT_FOUND };

                storage.access_code = string.Empty;
                return new Response { Status = 200, ObjectData =  storage };
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

        public async Task<Response> GetRange(int userId, int skip, int count, bool byDesc)
        {
            try
            {
                var cacheKey = $"{ImmutableData.STORAGES_PREFIX}{userId}_{skip}_{count}_{byDesc}";
                var storages = await cacheHandler.CacheAndGetRange(
                    new StorageRangeObject(cacheKey, userId, skip, count, byDesc));

                foreach (var storage in storages)
                    storage.access_code = string.Empty;

                return new Response { Status = 200, ObjectData = storages };
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

        public async Task<Response> DeleteOne(int userId, int storageId, string accessCode)
        {
            try
            {
                var storage = await repository.GetByFilter(new StorageByIdAndRelationSpec(storageId, userId));
                if (storage is null)
                    return new Response { Status = 404, Message = Message.NOT_FOUND };

                if (!hashUtility.Verify(accessCode, storage.access_code))
                    return new Response { Status = 403, Message = Message.INCORRECT };

                await repository.Delete(storageId);
                return new Response { Status = 204 };
            }
            catch (EntityException ex)
            {
                return new Response { Status = 500, Message = ex.Message };
            }
        }
    }
}
