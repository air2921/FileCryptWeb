﻿using application.Helpers;
using application.Helpers.Localization;
using application.Cache_Handlers;
using domain.Abstractions.Data;
using domain.Exceptions;
using domain.Models;
using application.Abstractions.TP_Services;
using System.Text.RegularExpressions;
using application.Helper_Services;

namespace application.Master_Services.Core
{
    public class StorageItemsService(
        IRepository<KeyStorageItemModel> repository,
        ITransaction<KeyStorageItemModel> transaction,
        ICacheHandler<KeyStorageItemModel> itemCacheHandler,
        ICacheHandler<KeyStorageModel> storageCacheHandler,
        IRedisCache redisCache,
        IHashUtility hashUtility)
    {
        private bool IsBase64String(string? key)
        {
            if (string.IsNullOrEmpty(key) || key.Length % 4 != 0 || !Regex.IsMatch(key, RegularEx.EncryptionKey))
                return false;

            try
            {
                return Convert.FromBase64String(key).Length.Equals(32);
            }
            catch (FormatException)
            {
                return false;
            }
        }

        public async Task<Response> Add(int userId, int storageId, string code, string name, string value)
        {
            try
            {
                if (!IsBase64String(value))
                    return new Response { Status = 422, Message = Message.INVALID_FORMAT };

                var response = await VerifyAccess(userId, storageId, code);
                if (!response.IsSuccess)
                    return response;

                await transaction.CreateTransaction(new KeyStorageItemModel
                {
                    storage_id = storageId,
                    key_name = name,
                    key_value = value,
                    created_at = DateTime.UtcNow
                }, userId.ToString());
                await redisCache.DeteteCacheByKeyPattern($"{ImmutableData.STORAGE_ITEMS_PREFIX}{userId}");

                return new Response { Status = 201, Message = Message.CREATED };
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
                var key = await itemCacheHandler.CacheAndGet(
                    new StorageItemObject(cacheKey, userId, keyId, storageId, code));

                if (key is null)
                    return new Response { Status = 404, Message = Message.NOT_FOUND };
                else
                    return new Response { Status = 200, ObjectData = key };
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
                return new Response
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
                if (!response.IsSuccess)
                    return response;

                await repository.Delete(keyId);
                await redisCache.DeteteCacheByKeyPattern($"{ImmutableData.STORAGE_ITEMS_PREFIX}{userId}");

                return new Response { Status = 204 };
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

                return new Response { Status = 200 };
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
