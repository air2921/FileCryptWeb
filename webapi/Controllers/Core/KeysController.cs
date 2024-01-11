using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        private readonly FileCryptDbContext _dbContext;
        private readonly IGenerateKey _generateKey;
        private readonly IRedisCache _redisCaching;
        private readonly IRedisKeys _redisKeys;
        private readonly IUserInfo _userInfo;
        private readonly ITokenService _tokenService;
        private readonly IEncryptKey _encryptKey;
        private readonly IValidation _validation;
        private readonly IDecryptKey _decryptKey;
        private readonly byte[] secretKey;

        public KeysController(
            IConfiguration configuration,
            IRead<KeyModel> readKeys,
            FileCryptDbContext dbContext,
            IGenerateKey generateKey,
            IRedisCache redisCaching,
            IRedisKeys redisKeys,
            IUserInfo userInfo,
            ITokenService tokenService,
            IEncryptKey encryptKey,
            IValidation validation,
            IDecryptKey decryptKey)
        {
            _configuration = configuration;
            _readKeys = readKeys;
            _dbContext = dbContext;
            _generateKey = generateKey;
            _redisCaching = redisCaching;
            _redisKeys = redisKeys;
            _userInfo = userInfo;
            _tokenService = tokenService;
            _encryptKey = encryptKey;
            _validation = validation;
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
            string? key = null;

            if (auto)
            {
                key = _generateKey.GenerateKey();
            }
            else
            {
                if (keyModel is null || string.IsNullOrWhiteSpace(keyModel.private_key))
                    return StatusCode(400, new { message = ErrorMessage.InvalidKey });

                key = keyModel.private_key;
            }

            if (!Regex.IsMatch(key, Validation.EncryptionKey) || _validation.IsBase64String(key) == false)
                return StatusCode(400, new { message = ErrorMessage.InvalidKey });

            var existingUser = await _dbContext.Keys.FirstOrDefaultAsync(u => u.user_id == _userInfo.UserId);
            if (existingUser is null)
            {
                _tokenService.DeleteTokens();
                return StatusCode(404);
            }

            existingUser.private_key = await _encryptKey.EncryptionKeyAsync(key, secretKey);
            await _dbContext.SaveChangesAsync();
            await _redisCaching.DeleteCache(_redisKeys.PrivateKey);

            return StatusCode(200, new { message = AccountSuccessMessage.KeyUpdated});
        }

        [HttpPut("internal")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdatePersonalInternalKey([FromBody] KeyModel? keyModel, [FromQuery] bool auto)
        {
            string? key = null;

            if (auto)
            {
                key = _generateKey.GenerateKey();
            }
            else
            {
                if (keyModel is null || string.IsNullOrWhiteSpace(keyModel.internal_key))
                    return StatusCode(400, new { message = ErrorMessage.InvalidKey });

                key = keyModel.internal_key;
            }

            if (!Regex.IsMatch(key, Validation.EncryptionKey) || _validation.IsBase64String(key) == false)
                return StatusCode(400, new { message = ErrorMessage.InvalidKey });

            var existingUser = await _dbContext.Keys.FirstOrDefaultAsync(u => u.user_id == _userInfo.UserId);
            if (existingUser is null)
            {
                _tokenService.DeleteTokens();
                return StatusCode(404);
            }

            existingUser.internal_key = await _encryptKey.EncryptionKeyAsync(key, secretKey);
            await _dbContext.SaveChangesAsync();
            await _redisCaching.DeleteCache(_redisKeys.InternalKey);

            return StatusCode(200, new { message = AccountSuccessMessage.KeyUpdated });
        }

        [HttpPut("received/clean")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CleanReceivedInternalKey()
        {
            var existingUser = await _dbContext.Keys.FirstOrDefaultAsync(u => u.user_id == _userInfo.UserId);
            if (existingUser is null)
            {
                _tokenService.DeleteTokens();
                return StatusCode(404);
            }

            existingUser.received_key = null;
            await _dbContext.SaveChangesAsync();

            await _redisCaching.DeleteCache(_redisKeys.ReceivedKey);

            return StatusCode(200, new { message = AccountSuccessMessage.KeyUpdated });
        }
    }
}
