using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
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
        private readonly IRepository<KeyModel> _keyRepository;
        private readonly IConfiguration _configuration;
        private readonly IGenerateKey _generateKey;
        private readonly IRedisCache _redisCache;
        private readonly IRedisKeys _redisKeys;
        private readonly IUserInfo _userInfo;
        private readonly ITokenService _tokenService;
        private readonly IValidation _validation;
        private readonly IDecryptKey _decryptKey;
        private readonly IEncryptKey _encryptKey;
        private readonly byte[] secretKey;

        public KeysController(
            IRepository<KeyModel> keyRepository,
            IConfiguration configuration,
            IGenerateKey generateKey,
            IRedisCache redisCache,
            IRedisKeys redisKeys,
            IUserInfo userInfo,
            ITokenService tokenService,
            IValidation validation,
            IDecryptKey decryptKey,
            IEncryptKey encryptKey)
        {
            _keyRepository = keyRepository;
            _configuration = configuration;
            _generateKey = generateKey;
            _redisCache = redisCache;
            _redisKeys = redisKeys;
            _userInfo = userInfo;
            _tokenService = tokenService;
            _validation = validation;
            _decryptKey = decryptKey;
            _encryptKey = encryptKey;
            secretKey = Convert.FromBase64String(_configuration["FileCryptKey"]!);
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllKeys()
        {
            try
            {
                var cacheKey = $"Keys_{_userInfo.UserId}";

                var keys = new KeyModel();
                var cacheKeys = await _redisCache.GetCachedData(cacheKey);

                if (cacheKeys is null)
                {
                    keys = await _keyRepository.GetByFilter(query => query.Where(k => k.user_id.Equals(_userInfo.UserId)));
                    if (keys is null)
                        return StatusCode(404);

                    await _redisCache.CacheData(cacheKey, keys, TimeSpan.FromMinutes(10));
                }
                else
                    keys = JsonConvert.DeserializeObject<KeyModel>(cacheKeys);

                if (keys is null)
                    return StatusCode(404);

                string? privateKey = keys.private_key is not null ? await _decryptKey.DecryptionKeyAsync(keys.private_key, secretKey) : null;
                string? internalKey = keys.internal_key is not null ? await _decryptKey.DecryptionKeyAsync(keys.internal_key, secretKey) : null;
                string? receivedKey = keys.received_key is not null ? "hidden" : null;

                return StatusCode(200, new { keys = new { privateKey, internalKey, receivedKey } });
            }
            catch (OperationCanceledException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPut("private")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdatePrivateKey([FromQuery] string? key, [FromQuery] bool auto)
        {
            try
            {
                if (!auto)
                {
                    if (string.IsNullOrWhiteSpace(key) || !_validation.IsBase64String(key) || !Regex.IsMatch(key, Validation.EncryptionKey))
                        return StatusCode(400, new { message = ErrorMessage.InvalidKey });
                }
                else
                    key = _generateKey.GenerateKey();

                var keys = await _keyRepository.GetByFilter(query => query.Where(k => k.user_id.Equals(_userInfo.UserId)));
                keys.private_key = await _encryptKey.EncryptionKeyAsync(key, secretKey);

                await _keyRepository.Update(keys);

                await _redisCache.DeteteCacheByKeyPattern($"{ImmutableData.KEYS_PREFIX}{_userInfo.UserId}");
                await _redisCache.DeleteCache(_redisKeys.PrivateKey);

                return StatusCode(200, new { message = AccountSuccessMessage.KeyUpdated });
            }
            catch (EntityNotUpdatedException ex)
            {
                return StatusCode(404, new { message = ex.Message });
            }
        }

        [HttpPut("internal")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdatePersonalInternalKey([FromQuery] string? key, [FromQuery] bool auto)
        {
            try
            {
                if (!auto)
                {
                    if (string.IsNullOrWhiteSpace(key) || !_validation.IsBase64String(key) || !Regex.IsMatch(key, Validation.EncryptionKey))
                        return StatusCode(400, new { message = ErrorMessage.InvalidKey });
                }
                else
                    key = _generateKey.GenerateKey();

                var keys = await _keyRepository.GetByFilter(query => query.Where(k => k.user_id.Equals(_userInfo.UserId)));
                keys.internal_key = await _encryptKey.EncryptionKeyAsync(key, secretKey);

                await _keyRepository.Update(keys);

                await _redisCache.DeteteCacheByKeyPattern($"{ImmutableData.KEYS_PREFIX}{_userInfo.UserId}");
                await _redisCache.DeleteCache(_redisKeys.InternalKey);

                return StatusCode(200, new { message = AccountSuccessMessage.KeyUpdated });
            }
            catch (EntityNotUpdatedException ex)
            {
                return StatusCode(404, new { message = ex.Message });
            }
        }

        [HttpPut("received/clean")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CleanReceivedInternalKey()
        {
            try
            {
                var keys = await _keyRepository.GetByFilter(query => query.Where(k => k.user_id.Equals(_userInfo.UserId)));
                if (keys.received_key is null)
                    return StatusCode(409);
                keys.received_key = null;

                await _keyRepository.Update(keys);

                await _redisCache.DeteteCacheByKeyPattern($"{ImmutableData.KEYS_PREFIX}{_userInfo.UserId}");
                await _redisCache.DeleteCache(_redisKeys.InternalKey);

                return StatusCode(200, new { message = AccountSuccessMessage.KeyUpdated });
            }
            catch (EntityNotUpdatedException ex)
            {
                return StatusCode(404, new { message = ex.Message });
            }
        }
    }
}
