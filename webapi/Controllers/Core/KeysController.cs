using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using webapi.Attributes;
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
    [Route("api/core/keys")]
    [ApiController]
    [Authorize]
    public class KeysController : ControllerBase
    {
        #region fields and constructor

        private readonly IRepository<KeyModel> _keyRepository;
        private readonly IConfiguration _configuration;
        private readonly IGenerate _generate;
        private readonly IRedisCache _redisCache;
        private readonly IRedisKeys _redisKeys;
        private readonly IUserInfo _userInfo;
        private readonly IValidation _validation;
        private readonly ICypherKey _decryptKey;
        private readonly ICypherKey _encryptKey;
        private readonly byte[] secretKey;

        public KeysController(
            IRepository<KeyModel> keyRepository,
            IConfiguration configuration,
            IGenerate generate,
            IRedisCache redisCache,
            IRedisKeys redisKeys,
            IUserInfo userInfo,
            IValidation validation,
            [FromKeyedServices("Encrypt")] ICypherKey encrypt,
            [FromKeyedServices("Decrypt")] ICypherKey decrypt)
        {
            _keyRepository = keyRepository;
            _configuration = configuration;
            _generate = generate;
            _redisCache = redisCache;
            _redisKeys = redisKeys;
            _userInfo = userInfo;
            _validation = validation;
            _decryptKey = decrypt;
            _encryptKey = encrypt;
            secretKey = Convert.FromBase64String(_configuration[App.ENCRYPTION_KEY]!);
        }

        #endregion

        [HttpGet("all")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(typeof(object), 404)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> GetAllKeys()
        {
            try
            {
                var keys = new KeyModel();
                var cacheKey = $"{ImmutableData.KEYS_PREFIX}{_userInfo.UserId}";
                var cache = await _redisCache.GetCachedData(cacheKey);
                if (cache is null)
                {
                    keys = await _keyRepository.GetByFilter(query => query.Where(k => k.user_id.Equals(_userInfo.UserId)));
                    if (keys is null)
                        return StatusCode(404, new { message = Message.NOT_FOUND });

                    await _redisCache.CacheData(cacheKey, keys, TimeSpan.FromMinutes(10));
                }
                else
                    keys = JsonConvert.DeserializeObject<KeyModel>(cache);

                keys!.private_key = await _decryptKey.CypherKeyAsync(keys.private_key, secretKey);
                keys.internal_key = keys.internal_key is not null ? await _decryptKey.CypherKeyAsync(keys.internal_key, secretKey) : null;
                keys.received_key = keys.received_key is not null ? "hidden" : null;

                return StatusCode(200, new { keys = new { keys.private_key, keys.internal_key, keys.received_key } });
            }
            catch (OperationCanceledException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPut("private")]
        [ValidateAntiForgeryToken]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(typeof(object), 404)]
        public async Task<IActionResult> UpdatePrivateKey([FromQuery] string? key, [FromQuery] bool auto)
        {
            try
            {
                key = SetNewKey(key, auto);
                await UpdateKey(key, FileType.Private);

                await ClearData(_userInfo.UserId, _redisKeys.PrivateKey);

                return StatusCode(200, new { message = Message.UPDATED });
            }
            catch (EntityNotUpdatedException ex)
            {
                return StatusCode(404, new { message = ex.Message });
            }
        }

        [HttpPut("internal")]
        [ValidateAntiForgeryToken]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(typeof(object), 404)]
        public async Task<IActionResult> UpdatePersonalInternalKey([FromQuery] string? key, [FromQuery] bool auto)
        {
            try
            {
                key = SetNewKey(key, auto);
                await UpdateKey(key, FileType.Internal);

                await ClearData(_userInfo.UserId, _redisKeys.InternalKey);

                return StatusCode(200, new { message = Message.UPDATED });
            }
            catch (EntityNotUpdatedException ex)
            {
                return StatusCode(404, new { message = ex.Message });
            }
        }

        [HttpPut("received/clean")]
        [ValidateAntiForgeryToken]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(typeof(object), 409)]
        [ProducesResponseType(typeof(object), 404)]
        public async Task<IActionResult> CleanReceivedInternalKey()
        {
            try
            {
                var keys = await _keyRepository.GetByFilter(query => query.Where(k => k.user_id.Equals(_userInfo.UserId)));
                if (keys.received_key is null)
                    return StatusCode(409, new { message = Message.CONFLICT });

                keys.received_key = null;
                await _keyRepository.Update(keys);

                await ClearData(_userInfo.UserId, _redisKeys.ReceivedKey);

                return StatusCode(200, new { message = Message.UPDATED });
            }
            catch (EntityNotUpdatedException ex)
            {
                return StatusCode(404, new { message = ex.Message });
            }
        }

        [Helper]
        private string SetNewKey(string? key, bool auto)
        {
            if (!auto)
            {
                if (string.IsNullOrWhiteSpace(key) || !_validation.IsBase64String(key) || !Regex.IsMatch(key, Validation.EncryptionKey))
                    throw new ArgumentException(Message.INVALID_FORMAT);
                else
                    return key;
            }
            else
                return _generate.GenerateKey();
        }

        [Helper]
        private async Task UpdateKey(string key, FileType type)
        {
            try
            {
                var keys = await _keyRepository.GetByFilter(query => query.Where(k => k.user_id.Equals(_userInfo.UserId)));

                if (type.Equals(FileType.Private))
                    keys.private_key = await _encryptKey.CypherKeyAsync(key, secretKey);
                else if (type.Equals(FileType.Internal))
                    keys.internal_key = await _encryptKey.CypherKeyAsync(key, secretKey);
                else
                    throw new EntityNotUpdatedException();

                await _keyRepository.Update(keys);
            }
            catch (EntityNotUpdatedException)
            {
                throw;
            }
        }

        [Helper]
        private async Task ClearData(int userId, string key)
        {
            await _redisCache.DeteteCacheByKeyPattern($"{ImmutableData.KEYS_PREFIX}{userId}");
            await _redisCache.DeleteCache(key);
        }
    }
}
