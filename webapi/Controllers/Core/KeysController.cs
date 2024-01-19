using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json;
using Pipelines.Sockets.Unofficial;
using System.Text.RegularExpressions;
using webapi.DB;
using webapi.Exceptions;
using webapi.Interfaces.Cryptography;
using webapi.Interfaces.Redis;
using webapi.Interfaces.Services;
using webapi.Interfaces.SQL;
using webapi.Localization;
using webapi.Models;
using webapi.Services;

namespace webapi.Controllers.Core
{
    [Route("api/core/keys")]
    [ApiController]
    [Authorize]
    public class KeysController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IRead<KeyModel> _readKeys;
        private readonly IUpdate<KeyModel> _updateKeys;
        private readonly FileCryptDbContext _dbContext;
        private readonly IGenerateKey _generateKey;
        private readonly IRedisCache _redisCache;
        private readonly IRedisKeys _redisKeys;
        private readonly IUserInfo _userInfo;
        private readonly ITokenService _tokenService;
        private readonly IValidation _validation;
        private readonly IDecryptKey _decryptKey;
        private readonly byte[] secretKey;

        public KeysController(
            IConfiguration configuration,
            IRead<KeyModel> readKeys,
            IUpdate<KeyModel> updateKeys,
            FileCryptDbContext dbContext,
            IGenerateKey generateKey,
            IRedisCache redisCache,
            IRedisKeys redisKeys,
            IUserInfo userInfo,
            ITokenService tokenService,
            IValidation validation,
            IDecryptKey decryptKey)
        {
            _configuration = configuration;
            _readKeys = readKeys;
            _updateKeys = updateKeys;
            _dbContext = dbContext;
            _generateKey = generateKey;
            _redisCache = redisCache;
            _redisKeys = redisKeys;
            _userInfo = userInfo;
            _tokenService = tokenService;
            _validation = validation;
            _decryptKey = decryptKey;
            secretKey = Convert.FromBase64String(_configuration["FileCryptKey"]!);
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllKeys()
        {
            var cacheKey = $"Keys_{_userInfo.UserId}";

            try
            {
                var keys = new KeyModel();
                var cacheKeys = await _redisCache.GetCachedData(cacheKey);
                bool clearCache = HttpContext.Session.GetString(Constants.CACHE_KEYS) is not null ? bool.Parse(HttpContext.Session.GetString(Constants.CACHE_KEYS)) : true;

                if (clearCache)
                {
                    await _redisCache.DeleteCache(cacheKey);
                    HttpContext.Session.SetString(Constants.CACHE_KEYS, false.ToString());
                }

                if (cacheKeys is not null)
                {
                    keys = JsonConvert.DeserializeObject<KeyModel>(cacheKeys);
                }
                else
                {
                    keys = await _readKeys.ReadById(_userInfo.UserId, true);

                    await _redisCache.CacheData(cacheKey, keys, TimeSpan.FromMinutes(10));
                }

                if (keys is null)
                    return StatusCode(404);

                string? privateKey = keys.private_key is not null ? await _decryptKey.DecryptionKeyAsync(keys.private_key, secretKey) : null;
                string? internalKey = keys.internal_key is not null ? await _decryptKey.DecryptionKeyAsync(keys.internal_key, secretKey) : null;
                string? receivedKey = keys.received_key is not null ? await _decryptKey.DecryptionKeyAsync(keys.received_key, secretKey) : null;

                return StatusCode(200, new { keys = new { privateKey, internalKey, receivedKey }});
            }
            catch (UserException ex)
            {
                _tokenService.DeleteTokens();
                return StatusCode(404, new { message = ex.Message });
            }
        }

        [HttpPut("private")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdatePrivateKey([FromBody] string? key, [FromQuery] bool auto)
        {
            try
            {
                if (auto)
                {
                    key = _generateKey.GenerateKey();
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(key) || !_validation.IsBase64String(key) || !Regex.IsMatch(key, Validation.EncryptionKey))
                        return StatusCode(400, new { message = ErrorMessage.InvalidKey });
                }

                var existingUser = await _readKeys.ReadById(_userInfo.UserId, true);

                await _updateKeys.Update(new KeyModel { user_id = _userInfo.UserId, private_key = key, received_key = existingUser.received_key }, true);
                HttpContext.Session.SetString(Constants.CACHE_KEYS, true.ToString());
                await _redisCache.DeleteCache(_redisKeys.PrivateKey);

                return StatusCode(200, new { message = AccountSuccessMessage.KeyUpdated });
            }
            catch (UserException ex)
            {
                _tokenService.DeleteTokens();
                return StatusCode(404, new { message = ex.Message });
            }
        }

        [HttpPut("internal")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdatePersonalInternalKey([FromBody] string? key, [FromQuery] bool auto)
        {
            try
            {
                if (auto)
                {
                    key = _generateKey.GenerateKey();
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(key) || !_validation.IsBase64String(key) || !Regex.IsMatch(key, Validation.EncryptionKey))
                        return StatusCode(400, new { message = ErrorMessage.InvalidKey });
                }

                var existingUser = await _readKeys.ReadById(_userInfo.UserId, true);

                var keyModel = new KeyModel
                {
                    user_id = _userInfo.UserId,
                    private_key = await _decryptKey.DecryptionKeyAsync(existingUser.private_key, secretKey),
                    internal_key = key,
                    received_key = existingUser.received_key,
                };

                await _updateKeys.Update(keyModel, true);
                HttpContext.Session.SetString(Constants.CACHE_KEYS, true.ToString());
                await _redisCache.DeleteCache(_redisKeys.InternalKey);

                return StatusCode(200, new { message = AccountSuccessMessage.KeyUpdated });
            }
            catch (UserException ex)
            {
                _tokenService.DeleteTokens();
                return StatusCode(404, new { message = ex.Message });
            }
        }

        [HttpPut("received/clean")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CleanReceivedInternalKey()
        {
            try
            {
                var existingUser = await _readKeys.ReadById(_userInfo.UserId, true);

                var keyModel = new KeyModel
                {
                    user_id = _userInfo.UserId,
                    private_key = await _decryptKey.DecryptionKeyAsync(existingUser.private_key, secretKey),
                    received_key = null
                };

                await _updateKeys.Update(keyModel, true);
                HttpContext.Session.SetString(Constants.CACHE_KEYS, true.ToString());
                await _redisCache.DeleteCache(_redisKeys.InternalKey);

                return StatusCode(200, new { message = AccountSuccessMessage.KeyUpdated });
            }
            catch (UserException ex)
            {
                _tokenService.DeleteTokens();
                return StatusCode(404, new { message = ex.Message });
            }
        }
    }
}
