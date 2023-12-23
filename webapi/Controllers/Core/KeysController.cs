using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using webapi.Exceptions;
using webapi.Interfaces.Cryptography;
using webapi.Interfaces.Redis;
using webapi.Interfaces.Services;
using webapi.Interfaces.SQL;
using webapi.Localization;
using webapi.Models;

namespace webapi.Controllers.Core
{
    [Route("api/core/keys")]
    [ApiController]
    [Authorize]
    public class KeysController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IRead<KeyModel> _readKeys;
        private readonly IUpdateKeys _updateKeys;
        private readonly IGenerateKey _generateKey;
        private readonly IRedisCache _redisCaching;
        private readonly IRedisKeys _redisKeys;
        private readonly IUserInfo _userInfo;
        private readonly ITokenService _tokenService;
        private readonly IDecryptKey _decryptKey;
        private readonly ILogger<KeysController> _logger;
        private readonly byte[] secretKey;

        public KeysController(
            IConfiguration configuration,
            IRead<KeyModel> readKeys,
            IUpdateKeys updateKeys,
            IGenerateKey generateKey,
            IRedisCache redisCaching,
            IRedisKeys redisKeys,
            IUserInfo userInfo,
            ITokenService tokenService,
            IDecryptKey decryptKey,
            ILogger<KeysController> logger)
        {
            _configuration = configuration;
            _readKeys = readKeys;
            _updateKeys = updateKeys;
            _generateKey = generateKey;
            _redisCaching = redisCaching;
            _redisKeys = redisKeys;
            _userInfo = userInfo;
            _tokenService = tokenService;
            _decryptKey = decryptKey;
            _logger = logger;
            secretKey = Convert.FromBase64String(_configuration["FileCryptKey"]!);
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllKeys()
        {
            try
            {
                List<string> decryptedKeys = new();

                var userKeys = await _readKeys.ReadById(_userInfo.UserId, true);

                string?[] encryptionKeys =
                {
                    userKeys.private_key,
                    userKeys.person_internal_key,
                    userKeys.received_internal_key
                };

                foreach (string? encryptedKey in encryptionKeys)
                {
                    try
                    {
                        if (encryptedKey is null)
                            continue;

                        decryptedKeys.Add(await _decryptKey.DecryptionKeyAsync(encryptedKey, secretKey));
                    }
                    catch (CryptographicException ex)
                    {
                        _logger.LogCritical(ex.ToString(), nameof(GetAllKeys));
                        continue;
                    }
                }
                return StatusCode(200, new { decryptedKeys });
            }
            catch (UserException ex)
            {
                _tokenService.DeleteTokens();
                return StatusCode(404, new { message = ex.Message });
            }
        }

        [HttpPut("private/auto")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdatePrivateKey()
        {
            try
            {
                string key = _generateKey.GenerateKey();
                var keyModel = new KeyModel { user_id = _userInfo.UserId, private_key = key };

                await _updateKeys.UpdatePrivateKey(keyModel);
                await _redisCaching.DeleteCache(_redisKeys.PrivateKey);

                return StatusCode(200, new { message = AccountSuccessMessage.KeyUpdated, private_key = key });
            }
            catch (UserException ex)
            {
                _tokenService.DeleteTokens();
                return StatusCode(404, new { message = ex.Message });
            }
        }

        [HttpPut("internal/auto")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdatePersonalInternalKey()
        {
            try
            {
                string key = _generateKey.GenerateKey();
                var keyModel = new KeyModel { user_id = _userInfo.UserId, person_internal_key = key };

                await _updateKeys.UpdatePersonalInternalKey(keyModel);
                await _redisCaching.DeleteCache(_redisKeys.PersonalInternalKey);

                return StatusCode(200, new { message = AccountSuccessMessage.KeyUpdated, internal_key = key });
            }
            catch (UserException ex)
            {
                _tokenService.DeleteTokens();
                return StatusCode(404, new { message = ex.Message });
            }
        }

        [HttpPut("internal/own")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdatePersonalInternalKeyToYourOwn([FromBody] KeyModel keyModel)
        {
            try
            {
                var newKeyModel = new KeyModel { user_id = _userInfo.UserId, person_internal_key = keyModel.person_internal_key };
                await _updateKeys.UpdatePersonalInternalKey(newKeyModel);
                await _redisCaching.DeleteCache(_redisKeys.PersonalInternalKey);

                return StatusCode(200, new { message = AccountSuccessMessage.KeyUpdated, your_internal_key = keyModel.person_internal_key });
            }
            catch (UserException)
            {
                _tokenService.DeleteTokens();
                return StatusCode(404);
            }
            catch (ArgumentException ex)
            {
                return StatusCode(422, new { message = ex.Message });
            }
        }

        [HttpPut("private/own")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdatePrivateKeyToYourOwn([FromBody] KeyModel keyModel)
        {
            try
            {
                var newKeyModel = new KeyModel { user_id = _userInfo.UserId, private_key = keyModel.private_key };
                await _updateKeys.UpdatePrivateKey(newKeyModel);
                await _redisCaching.DeleteCache(_redisKeys.PrivateKey);

                return StatusCode(200, new { message = AccountSuccessMessage.KeyUpdated, your_private_key = keyModel.private_key });
            }
            catch (UserException ex)
            {
                _tokenService.DeleteTokens();
                return StatusCode(404, new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return StatusCode(422, new { message = ex.Message });
            }
        }

        [HttpPut("received/clean")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CleanReceivedInternalKey()
        {
            try
            {
                await _updateKeys.CleanReceivedInternalKey(_userInfo.UserId);
                await _redisCaching.DeleteCache(_redisKeys.ReceivedInternalKey);

                return StatusCode(200, new { message = AccountSuccessMessage.KeyRemoved });
            }
            catch (UserException ex)
            {
                _tokenService.DeleteTokens();
                return StatusCode(404, new { message = ex.Message });
            }
        }
    }
}
