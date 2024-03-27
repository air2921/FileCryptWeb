using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Security.Cryptography;
using webapi.DTO;
using webapi.Exceptions;
using webapi.Helpers;
using webapi.Interfaces;
using webapi.Interfaces.Controllers.Services;
using webapi.Interfaces.Cryptography;
using webapi.Interfaces.Redis;
using webapi.Interfaces.Services;
using webapi.Localization;
using webapi.Models;
using webapi.Services.Core;

namespace webapi.Controllers.Core
{
    [Route("api/core")]
    [ApiController]
    [Authorize]
    public class KeyStorageController(
        IStorageHelpers helpers,
        [FromKeyedServices(ImplementationKey.CORE_KEY_STORAGE_SERVICE)] IValidator validator,
        IRepository<KeyStorageModel> storageRepository,
        IRepository<KeyStorageItemModel> storageItemRepository,
        IMapper mapper,
        IRedisCache redisCache,
        [FromKeyedServices("Decrypt")] ICypherKey decryptKey,
        [FromKeyedServices("Encrypt")] ICypherKey encryptKey,
        IPasswordManager passwordManager,
        IUserInfo userInfo,
        IConfiguration configuration) : ControllerBase
    {
        private readonly byte[] secretKey = Convert.FromBase64String(configuration[App.ENCRYPTION_KEY]!);

        [HttpPost("storage")]
        [ValidateAntiForgeryToken]
        [ProducesResponseType(201)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> AddStorage([FromBody] StorageDTO storageDTO)
        {
            try
            {
                var keyStorageModel = mapper.Map<StorageDTO, KeyStorageModel>(storageDTO);
                keyStorageModel.user_id = userInfo.UserId;
                keyStorageModel.last_time_modified = DateTime.UtcNow;
                keyStorageModel.access_code = passwordManager.HashingPassword(storageDTO.access_code.ToString());

                await storageRepository.Add(keyStorageModel);

                await redisCache.DeteteCacheByKeyPattern($"{ImmutableData.STORAGES_PREFIX}{userInfo.UserId}");
                return StatusCode(201);
            }
            catch (EntityNotCreatedException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpDelete("storage/{storageId}")]
        [ValidateAntiForgeryToken]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(object), 403)]
        [ProducesResponseType(typeof(object), 404)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> DeleteStorage([FromRoute] int storageId, [FromQuery] int code)
        {
            try
            {
                var storage = await helpers.GetAndValidateStorage(storageId, userInfo.UserId, code);
                await storageRepository.Delete(storageId);

                await redisCache.DeteteCacheByKeyPattern($"{ImmutableData.STORAGES_PREFIX}{userInfo.UserId}");
                return StatusCode(204);
            }
            catch (EntityNotDeletedException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
            catch (OperationCanceledException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
            catch (ArgumentNullException ex)
            {
                return StatusCode(404, new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
        }

        [HttpGet("storage/{storageId}")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(typeof(object), 403)]
        [ProducesResponseType(typeof(object), 404)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> GetStorageAndItems([FromRoute] int storageId, [FromQuery] int code)
        {
            try
            {
                var storage = await helpers.GetAndValidateStorage(storageId, userInfo.UserId, code);
                storage.access_code = string.Empty;

                var cacheKey = $"{ImmutableData.STORAGES_PREFIX}{userInfo.UserId}_{storageId}_{storage.encrypt}";

                var cache = await redisCache.GetCachedData(cacheKey);
                if (cache is not null)
                    return StatusCode(200, new { storage, keys = JsonConvert.DeserializeObject<IEnumerable<KeyStorageItemModel>>(cache) });

                var keys = await storageItemRepository.GetAll(query => query
                    .Where(s => s.storage_id.Equals(storage.storage_id)));

                if (storage.encrypt)
                    keys = await helpers.CypherKeys(keys, false);

                await redisCache.CacheData(cacheKey, keys, TimeSpan.FromMinutes(10));

                return StatusCode(200, new { storage, keys });
            }
            catch (OperationCanceledException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
            catch (ArgumentNullException ex)
            {
                return StatusCode(404, new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
        }

        [HttpGet("storage/all")]
        [ProducesResponseType(typeof(IEnumerable<KeyStorageModel>), 200)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> GetStorages()
        {
            try
            {
                return StatusCode(200, new {
                    storages = await storageRepository.GetAll(query => query.Where(s => s.user_id.Equals(userInfo.UserId)))});
            }
            catch (OperationCanceledException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost("key/{storageId}")]
        [ValidateAntiForgeryToken]
        [ProducesResponseType(201)]
        [ProducesResponseType(typeof(object), 403)]
        [ProducesResponseType(typeof(object), 404)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> AddKey([FromRoute] int storageId, [FromQuery] int code, [FromBody] KeyDTO keyDTO)
        {
            try
            {
                if (!validator.IsValid(keyDTO.key_value))
                    return StatusCode(422, new { message = Message.INVALID_FORMAT });

                var storage = await helpers.GetAndValidateStorage(storageId, userInfo.UserId, code);

                var keyItemModel = mapper.Map<KeyDTO, KeyStorageItemModel>(keyDTO);
                keyItemModel.key_value = storage.encrypt ? await encryptKey.CypherKeyAsync(keyDTO.key_value, secretKey) : keyDTO.key_value;
                keyItemModel.storage_id = storageId;
                keyItemModel.created_at = DateTime.UtcNow;

                await storageItemRepository.Add(keyItemModel);

                await redisCache.DeteteCacheByKeyPattern($"{ImmutableData.STORAGES_PREFIX}{userInfo.UserId}");
                return StatusCode(201);
            }
            catch (EntityNotCreatedException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
            catch (OperationCanceledException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
            catch (ArgumentNullException ex)
            {
                return StatusCode(404, new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
        }

        [HttpGet("key/{storageId}/{keyId}")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(typeof(object), 403)]
        [ProducesResponseType(typeof(object), 404)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> GetKey([FromRoute] int storageId, [FromRoute] int keyId, [FromQuery] int code)
        {
            try
            {
                var storage = await helpers.GetAndValidateStorage(storageId, userInfo.UserId, code);

                var key = await storageItemRepository.GetByFilter(query => query
                    .Where(s => s.key_id.Equals(keyId) && s.storage_id.Equals(storageId)));

                if (key is null)
                    return StatusCode(404, new { message = Message.NOT_FOUND });

                return StatusCode(200, new { key = decryptKey.CypherKeyAsync(key.key_value, secretKey) });
            }
            catch (OperationCanceledException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
            catch (ArgumentNullException ex)
            {
                return StatusCode(404, new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
            catch (CryptographicException)
            {
                return StatusCode(500, new { message = Message.ERROR });
            }
        }

        [HttpDelete("key/{storageId}/{keyId}")]
        [ValidateAntiForgeryToken]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> DeleteKey([FromRoute] int storageId, [FromRoute] int keyId, [FromQuery] int code)
        {
            try
            {
                var storage = await helpers.GetAndValidateStorage(storageId, userInfo.UserId, code);

                await storageItemRepository.DeleteByFilter(query => query
                        .Where(s => s.key_id.Equals(keyId) && s.storage_id.Equals(storage.storage_id)));

                await redisCache.DeteteCacheByKeyPattern($"{ImmutableData.STORAGES_PREFIX}{userInfo.UserId}");
                return StatusCode(204);
            }
            catch (EntityNotDeletedException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
            catch (ArgumentNullException ex)
            {
                return StatusCode(404, new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
        }
    }
}
