using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Reflection;
using System.Text.RegularExpressions;
using webapi.Attributes;
using webapi.Cryptography;
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
            IEnumerable<ICypherKey> cypherKeys)
        {
            _keyRepository = keyRepository;
            _configuration = configuration;
            _generate = generate;
            _redisCache = redisCache;
            _redisKeys = redisKeys;
            _userInfo = userInfo;
            _validation = validation;
            _decryptKey = cypherKeys.FirstOrDefault(k => k.GetType().GetCustomAttribute<ImplementationKeyAttribute>()?.Key == "Decrypt");
            _encryptKey = cypherKeys.FirstOrDefault(k => k.GetType().GetCustomAttribute<ImplementationKeyAttribute>()?.Key == "Encrypt");
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
                var keys = await GetKeys();
                if (keys is null)
                    return StatusCode(404, new { message = Message.NOT_FOUND });

                var keyValues = await SetKeys(keys);

                return StatusCode(200, new { keys = new { keyValues.privateKey, keyValues.internalKey, keyValues.receivedKey } });
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
                SetNewKey(ref key, auto);
                await UpdateKey(key!, keys => keys.private_key);

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
                SetNewKey(ref key, auto);
                await UpdateKey(key!, keys => keys.internal_key);

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

                await ClearData(_userInfo.UserId, _redisKeys.InternalKey);

                return StatusCode(200, new { message = Message.UPDATED });
            }
            catch (EntityNotUpdatedException ex)
            {
                return StatusCode(404, new { message = ex.Message });
            }
        }

        [Helper]
        private async Task<KeyModel> GetKeys()
        {
            var cacheKey = $"Keys_{_userInfo.UserId}";
            var cacheKeys = await _redisCache.GetCachedData(cacheKey);

            if (cacheKeys is null)
            {
                var keys = await _keyRepository.GetByFilter(query => query.Where(k => k.user_id.Equals(_userInfo.UserId)));
                if (keys is null)
                    return null;

                await _redisCache.CacheData(cacheKey, keys, TimeSpan.FromMinutes(10));
                return keys;
            }
            else
                return JsonConvert.DeserializeObject<KeyModel>(cacheKeys);
        }

        [Helper]
        private async Task<KeyObject> SetKeys(KeyModel keyModel)
        {
            string? privateKey = keyModel.private_key is not null ? await _decryptKey.CypherKeyAsync(keyModel.private_key, secretKey) : null;
            string? internalKey = keyModel.internal_key is not null ? await _decryptKey.CypherKeyAsync(keyModel.internal_key, secretKey) : null;
            string? receivedKey = keyModel.received_key is not null ? "hidden" : null;

            return new KeyObject
            {
                privateKey = privateKey,
                internalKey = internalKey,
                receivedKey = receivedKey,
            };
        }

        [Helper]
        private void SetNewKey(ref string? key, bool auto)
        {
            if (!auto)
            {
                if (string.IsNullOrWhiteSpace(key) || !_validation.IsBase64String(key) || !Regex.IsMatch(key, Validation.EncryptionKey))
                    throw new ArgumentException(Message.INVALID_FORMAT);
            }
            else
                key = _generate.GenerateKey();
        }

        [Helper]
        private async Task UpdateKey(string key, Func<KeyModel, string> fieldSelector)
        {
            try
            {
                var keys = await _keyRepository.GetByFilter(query => query.Where(k => k.user_id.Equals(_userInfo.UserId)));

                string fieldValue = fieldSelector(keys);

                fieldValue = await _encryptKey.CypherKeyAsync(key!, secretKey);

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

        [AuxiliaryObject]
        private class KeyObject
        {
            public string? privateKey { get; set; }
            public string? internalKey { get; set; }
            public string? receivedKey { get; set; }
        }
    }
}
