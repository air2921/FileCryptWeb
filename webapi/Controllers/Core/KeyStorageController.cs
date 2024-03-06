using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using webapi.Attributes;
using webapi.DTO;
using webapi.Exceptions;
using webapi.Helpers;
using webapi.Interfaces;
using webapi.Interfaces.Cryptography;
using webapi.Interfaces.Redis;
using webapi.Interfaces.Services;
using webapi.Localization;
using webapi.Models;

namespace webapi.Controllers.Core
{
    [Route("api/core")]
    [ApiController]
    [Authorize]
    public class KeyStorageController : ControllerBase
    {
        #region fields and constructor

        private readonly IRepository<KeyStorageModel> _storageRepository;
        private readonly IRepository<KeyStorageItemModel> _storageItemRepository;
        private readonly IRedisCache _redisCache;
        private readonly ICypherKey _decryptKey;
        private readonly ICypherKey _encryptKey;
        private readonly IPasswordManager _passwordManager;
        private readonly IValidation _validation;
        private readonly IUserInfo _userInfo;
        private readonly IConfiguration _configuration;
        private readonly byte[] secretKey;

        public KeyStorageController(
            IRepository<KeyStorageModel> storageRepository,
            IRepository<KeyStorageItemModel> storageItemRepository,
            IRedisCache redisCache,
            IEnumerable<ICypherKey> cypherKeys,
            IImplementationFinder implementationFinder,
            IPasswordManager passwordManager,
            IValidation validation,
            IUserInfo userInfo,
            IConfiguration configuration)
        {
            _storageRepository = storageRepository;
            _storageItemRepository = storageItemRepository;
            _redisCache = redisCache;
            _decryptKey = implementationFinder.GetImplementationByKey(cypherKeys, ImplementationKey.DECRYPT_KEY);
            _encryptKey = implementationFinder.GetImplementationByKey(cypherKeys, ImplementationKey.ENCRYPT_KEY);
            _passwordManager = passwordManager;
            _validation = validation;
            _userInfo = userInfo;
            _configuration = configuration;
            secretKey = Convert.FromBase64String(_configuration[App.ENCRYPTION_KEY]!);
        }

        #endregion

        [HttpPost("storage")]
        [XSRFProtection]
        [ProducesResponseType(201)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> AddStorage([FromBody] StorageDTO storageDTO)
        {
            try
            {
                await _storageRepository.Add(new KeyStorageModel
                {
                    user_id = _userInfo.UserId,
                    storage_name = storageDTO.storage_name,
                    last_time_modified = DateTime.UtcNow,
                    access_code = _passwordManager.HashingPassword(storageDTO.storage_code.ToString()),
                    encrypt = storageDTO.encrypt
                });

                await _redisCache.DeteteCacheByKeyPattern($"{ImmutableData.STORAGES_PREFIX}{_userInfo.UserId}");
                return StatusCode(201);
            }
            catch (EntityNotCreatedException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPut("storage/{storageId}")]
        [XSRFProtection]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(typeof(object), 403)]
        [ProducesResponseType(typeof(object), 404)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> UpdateStorage([FromRoute] int storageId, [FromQuery] int code,
            [FromBody] UpdateStorageDTO storageDTO)
        {
            try
            {
                var storage = await GetAndValidateStorage(storageId, _userInfo.UserId, code);
                await DbUpdate(storageDTO, storage);

                await _redisCache.DeteteCacheByKeyPattern($"{ImmutableData.STORAGES_PREFIX}{_userInfo.UserId}");
                return StatusCode(200, new { message = Message.UPDATED });
            }
            catch (EntityNotUpdatedException ex)
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
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
        }

        [HttpDelete("storage/{storageId}")]
        [XSRFProtection]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(object), 403)]
        [ProducesResponseType(typeof(object), 404)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> DeleteStorage([FromRoute] int storageId, [FromQuery] int code)
        {
            try
            {
                var storage = await GetAndValidateStorage(storageId, _userInfo.UserId, code);
                await _storageRepository.Delete(storageId);

                await _redisCache.DeteteCacheByKeyPattern($"{ImmutableData.STORAGES_PREFIX}{_userInfo.UserId}");
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
            catch (UnauthorizedAccessException ex)
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
                var storage = await GetAndValidateStorage(storageId, _userInfo.UserId, code);
                storage.access_code = string.Empty;

                var cacheKey = $"{ImmutableData.STORAGES_PREFIX}{_userInfo.UserId}_{storageId}_{storage.encrypt}";

                var cache = await _redisCache.GetCachedData(cacheKey);
                if (cache is not null)
                    return StatusCode(200, new { storage, keys = JsonConvert.DeserializeObject<IEnumerable<KeyStorageItemModel>>(cache) });

                var keys = await _storageItemRepository.GetAll(query => query
                    .Where(s => s.storage_id.Equals(storage.storage_id)));

                if (storage.encrypt)
                    keys = await CypherKeys(keys, false);

                await _redisCache.CacheData(cacheKey, keys, TimeSpan.FromMinutes(10));

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
            catch (UnauthorizedAccessException ex)
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
                    storages = await _storageRepository
                        .GetAll(query => query.Where(s => s.user_id.Equals(_userInfo.UserId)))
                });
            }
            catch (OperationCanceledException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost("key/{storageId}")]
        [XSRFProtection]
        [ProducesResponseType(201)]
        [ProducesResponseType(typeof(object), 403)]
        [ProducesResponseType(typeof(object), 404)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> AddKey([FromRoute] int storageId, [FromQuery] int code, [FromBody] KeyDTO keyDTO)
        {
            try
            {
                if (!IsValidKey(keyDTO.key_value))
                    return StatusCode(422, new { message = Message.INVALID_FORMAT });

                var storage = await GetAndValidateStorage(storageId, _userInfo.UserId, code);

                var key = await Cypher(storage.encrypt, keyDTO.key_value);

                await _storageItemRepository.Add(new KeyStorageItemModel
                {
                    key_name = keyDTO.key_name,
                    key_value = key,
                    storage_id = storageId,
                    created_at = DateTime.UtcNow
                });

                await _redisCache.DeteteCacheByKeyPattern($"{ImmutableData.STORAGES_PREFIX}{_userInfo.UserId}");
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
            catch (UnauthorizedAccessException ex)
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
                var storage = await GetAndValidateStorage(storageId, _userInfo.UserId, code);

                var key = await _storageItemRepository.GetByFilter(query => query
                    .Where(s => s.key_id.Equals(keyId) && s.storage_id.Equals(storageId)));

                if (key is null)
                    return StatusCode(404, new { message = Message.NOT_FOUND });

                return StatusCode(200, new { key = _decryptKey.CypherKeyAsync(key.key_value, secretKey) });
            }
            catch (OperationCanceledException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
            catch (ArgumentNullException ex)
            {
                return StatusCode(404, new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
            catch (CryptographicException)
            {
                return StatusCode(500, new { message = Message.ERROR });
            }
        }

        [HttpDelete("key/{storageId}/{keyId}")]
        [XSRFProtection]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> DeleteKey([FromRoute] int storageId, [FromRoute] int keyId, [FromQuery] int code)
        {
            try
            {
                var storage = await GetAndValidateStorage(storageId, _userInfo.UserId, code);

                await _storageItemRepository.DeleteByFilter(query => query
                        .Where(s => s.key_id.Equals(keyId) && s.storage_id.Equals(storage.storage_id)));

                await _redisCache.DeteteCacheByKeyPattern($"{ImmutableData.STORAGES_PREFIX}{_userInfo.UserId}");
                return StatusCode(204);
            }
            catch (EntityNotDeletedException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [Helper]
        private async Task<string> Cypher(bool encrypt, string key)
        {
            if (encrypt)
                return await _encryptKey.CypherKeyAsync(key, secretKey);
            else
                return key;
        }

        [Helper]
        private bool IsValidKey(string key)
        {
            return !string.IsNullOrWhiteSpace(key) && _validation.IsBase64String(key) && Regex.IsMatch(key, Validation.EncryptionKey);
        }

        [Helper]
        private async Task<KeyStorageModel> GetAndValidateStorage(int storageId, int userId, int code)
        {
            try
            {
                var storage = await _storageRepository.GetByFilter(query => query
                    .Where(s => s.user_id.Equals(userId) && s.storage_id.Equals(storageId)));

                if (storage is null)
                    throw new ArgumentNullException(Message.NOT_FOUND);

                if (!CheckCode(code, storage.access_code))
                    throw new UnauthorizedAccessException(Message.INCORRECT);

                return storage;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
        }

        [Helper]
        private bool CheckCode(int input, string correct)
        {
            return _passwordManager.CheckPassword(input.ToString(), correct);
        }

        [Helper]
        private async Task<IEnumerable<KeyStorageItemModel>> CypherKeys(IEnumerable<KeyStorageItemModel> keys, bool encrypt)
        {
            foreach (var key in keys)
            {
                try
                {
                    if (encrypt)
                        key.key_value = await _encryptKey.CypherKeyAsync(key.key_value, secretKey);
                    else
                        key.key_value = await _decryptKey.CypherKeyAsync(key.key_value, secretKey);
                }
                catch (CryptographicException)
                {
                    continue;
                }
            }

            return keys;
        }

        [Helper]
        private async Task DbUpdate(UpdateStorageDTO storageDTO, KeyStorageModel keyStorageModel)
        {
            var keys = new List<KeyStorageItemModel>();

            try
            {
                if (storageDTO.encrypt is not null)
                {
                    bool shouldEncrypt = storageDTO.encrypt.Value;
                    if (keyStorageModel.encrypt != shouldEncrypt)
                    {
                        keys = (List<KeyStorageItemModel>)await CypherKeys(await _storageItemRepository
                            .GetAll(query => query.Where(s => s.storage_id.Equals(keyStorageModel.storage_id))), shouldEncrypt);
                    }
                }

                if (!string.IsNullOrWhiteSpace(storageDTO.storage_name) && !storageDTO.storage_name.Equals(keyStorageModel.storage_name))
                    keyStorageModel.storage_name = storageDTO.storage_name;

                if (storageDTO.encrypt.HasValue && !storageDTO.encrypt.Equals(keyStorageModel.encrypt))
                    keyStorageModel.encrypt = storageDTO.encrypt.Value;

                if (storageDTO.access_code.HasValue && _validation.IsSixDigit(storageDTO.access_code.Value))
                    keyStorageModel.access_code = _passwordManager.HashingPassword(storageDTO.access_code.Value.ToString());

                keyStorageModel.last_time_modified = DateTime.UtcNow;

                await _storageRepository.Update(keyStorageModel);
            }
            catch (EntityNotUpdatedException)
            {
                throw;
            }
        }
    }
}
