using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
            IDecryptKey decryptKey)
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
            secretKey = Convert.FromBase64String(_configuration["FileCryptKey"]!);
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllKeys()
        {
            try
            {
                var userKeys = await _readKeys.ReadById(_userInfo.UserId, true);

                string? privateKey = await _decryptKey.DecryptionKeyAsync(userKeys.private_key!, secretKey);
                string? receivedKey = userKeys.received_key is not null ? "hidden" : null;
                string? internalKey = null;
                if(userKeys.internal_key is not null)
                {
                    internalKey = await _decryptKey.DecryptionKeyAsync(userKeys.internal_key, secretKey);
                }

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
        public async Task<IActionResult> UpdatePrivateKey([FromBody] KeyModel? keyModel, [FromQuery] bool auto)
        {
            try
            {
                string? key = null;

                if(auto)
                {
                    key = _generateKey.GenerateKey();
                }
                else
                {
                    if (keyModel is null)
                        return StatusCode(400, new { message = "Client request error" });

                    key = keyModel.private_key;
                }

                var newKeyModel = new KeyModel { user_id = _userInfo.UserId, private_key = key };

                await _updateKeys.UpdatePrivateKey(newKeyModel);
                await _redisCaching.DeleteCache(_redisKeys.PrivateKey);

                return StatusCode(200, new { message = AccountSuccessMessage.KeyUpdated, private_key = key });
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

        [HttpPut("internal")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdatePersonalInternalKey([FromBody] KeyModel? keyModel, [FromQuery] bool auto)
        {
            try
            {
                string? key = null;

                if (auto)
                {
                    key = _generateKey.GenerateKey();
                }
                else
                {
                    if (keyModel is null)
                        return StatusCode(400, new { message = "Client request error" });

                    key = keyModel.private_key;
                }

                var newKeyModel = new KeyModel { user_id = _userInfo.UserId, internal_key = key };

                await _updateKeys.UpdatePersonalInternalKey(newKeyModel);
                await _redisCaching.DeleteCache(_redisKeys.InternalKey);

                return StatusCode(200, new { message = AccountSuccessMessage.KeyUpdated, internal_key = key });
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
                await _redisCaching.DeleteCache(_redisKeys.ReceivedKey);

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
