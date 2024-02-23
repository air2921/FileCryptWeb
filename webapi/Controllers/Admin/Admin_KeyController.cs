using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;
using System.Security.Cryptography;
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

namespace webapi.Controllers.Admin
{
    [Route("api/admin/keys")]
    [ApiController]
    [Authorize(Roles = "HighestAdmin")]
    public class Admin_KeyController : ControllerBase
    {
        #region fields and constructor

        private readonly IRepository<KeyModel> _keyRepository;
        private readonly IRedisCache _redisCache;
        private readonly IUserInfo _userInfo;
        private readonly ICypherKey _decryptKey;
        private readonly IConfiguration _configuration;
        private readonly ILogger<Admin_KeyController> _logger;
        private readonly byte[] secretKey;

        public Admin_KeyController(
            IRepository<KeyModel> keyRepository,
            IRedisCache redisCache,
            IUserInfo userInfo,
            IEnumerable<ICypherKey> cypherKeys,
            IImplementationFinder implementationFinder,
            IConfiguration configuration,
            ILogger<Admin_KeyController> logger)
        {
            _keyRepository = keyRepository;
            _redisCache = redisCache;
            _userInfo = userInfo;
            _decryptKey = implementationFinder.GetImplementationByKey(cypherKeys, ImplementationKey.DECRYPT_KEY);
            _configuration = configuration;
            _logger = logger;
            secretKey = Convert.FromBase64String(_configuration[App.ENCRYPTION_KEY]!);
        }

        #endregion

        [HttpGet("all/{userId}")]
        [ProducesResponseType(typeof(HashSet<string>), 200)]
        [ProducesResponseType(typeof(object), 404)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> AllKeys([FromRoute] int userId)
        {
            try
            {        
                var userKeys = await _keyRepository.GetByFilter(query => query.Where(k => k.user_id.Equals(userId)));
                if (userKeys is null)
                    return StatusCode(404, new { message = Message.NOT_FOUND });

                return StatusCode(200, new { keys = await GetKeys(userKeys) });
            }
            catch (OperationCanceledException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPut("revoke/received/{userId}")]
        [XSRFProtection]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(typeof(object), 404)]
        public async Task<IActionResult> RevokeReceivedKey([FromRoute] int userId)
        {
            try
            {
                var keys = await _keyRepository.GetByFilter(query => query.Where(k => k.user_id.Equals(userId)));
                if (keys is null)
                    return StatusCode(404, new { message = Message.NOT_FOUND });

                await UpdateKeys(keys);
                await UpdateCache(userId);

                return StatusCode(200, new { message = Message.REMOVED });
            }
            catch (EntityNotUpdatedException ex)
            {
                return StatusCode(404, new { message = ex.Message });
            }
        }

        [Helper]
        private async Task<HashSet<string>> GetKeys(KeyModel userKeys)
        {
            HashSet<string> decryptedKeys = new();

            var encryptionKeys = new string?[]
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

                    decryptedKeys.Add(await _decryptKey.CypherKeyAsync(encryptedKey, secretKey));
                }
                catch (CryptographicException ex)
                {
                    _logger.LogCritical(ex.ToString(), nameof(Admin_KeyController));
                    continue;
                }
            }

            return decryptedKeys;
        }

        [Helper]
        private async Task UpdateKeys(KeyModel keys)
        {
            try
            {
                keys.received_key = null;
                await _keyRepository.Update(keys);
            }
            catch (EntityNotUpdatedException)
            {
                throw;
            }
        }

        [Helper]
        private async Task UpdateCache(int userId)
        {
            await _redisCache.DeleteCache("receivedKey#" + userId);
            await _redisCache.DeteteCacheByKeyPattern($"{ImmutableData.KEYS_PREFIX}{userId}");
        }
    }
}
