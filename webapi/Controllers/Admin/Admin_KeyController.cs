using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using webapi.Exceptions;
using webapi.Interfaces;
using webapi.Interfaces.Cryptography;
using webapi.Interfaces.Redis;
using webapi.Interfaces.Services;
using webapi.Localization;
using webapi.Models;
using webapi.Services;

namespace webapi.Controllers.Admin
{
    [Route("api/admin/keys")]
    [ApiController]
    [Authorize(Roles = "HighestAdmin")]
    public class Admin_KeyController : ControllerBase
    {
        private readonly IRepository<KeyModel> _keyRepository;
        private readonly IRedisCache _redisCache;
        private readonly IUserInfo _userInfo;
        private readonly IDecryptKey _decryptKey;
        private readonly IConfiguration _configuration;
        private readonly ILogger<Admin_KeyController> _logger;
        private readonly byte[] secretKey;

        public Admin_KeyController(
            IRepository<KeyModel> keyRepository,
            IRedisCache redisCache,
            IUserInfo userInfo,
            IDecryptKey decryptKey,
            IConfiguration configuration,
            ILogger<Admin_KeyController> logger)
        {
            _keyRepository = keyRepository;
            _redisCache = redisCache;
            _userInfo = userInfo;
            _decryptKey = decryptKey;
            _configuration = configuration;
            _logger = logger;
            secretKey = Convert.FromBase64String(_configuration[App.ENCRYPTION_KEY]!);
        }

        [HttpGet("all/{userId}")]
        public async Task<IActionResult> AllKeys([FromRoute] int userId)
        {
            HashSet<string> decryptedKeys = new();

            var userKeys = await _keyRepository.GetByFilter(query => query.Where(k => k.user_id.Equals(userId)));
            if (userKeys is null)
                return StatusCode(404);

            string?[] encryptionKeys =
            {
                userKeys.private_key,
                userKeys.internal_key,
                userKeys.received_key
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
                    _logger.LogCritical(ex.ToString(), nameof(Admin_KeyController));
                    continue;
                }
            }

            _logger.LogInformation($"{_userInfo.Username}#{_userInfo.UserId} get keys user#{userId}");
            return StatusCode(200, new { keys = decryptedKeys });
        }

        [HttpPut("revoke/received/{userId}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RevokeReceivedKey([FromRoute] int userId)
        {
            try
            {
                var keys = await _keyRepository.GetByFilter(query => query.Where(k => k.user_id.Equals(userId)));
                if (keys is null)
                    return StatusCode(404);

                keys.received_key = null;
                await _keyRepository.Update(keys);
                await _redisCache.DeleteCache("receivedKey#" + userId);

                _logger.LogInformation($"{_userInfo.Username}#{_userInfo.UserId} revoked received key from user#{userId}");

                return StatusCode(200, new { message = SuccessMessage.ReceivedKeyRevoked });
            }
            catch (EntityNotUpdatedException ex)
            {
                return StatusCode(404, new { message = ex.Message });
            }
        }
    }
}
