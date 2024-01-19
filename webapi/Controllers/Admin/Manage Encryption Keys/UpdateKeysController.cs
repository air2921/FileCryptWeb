using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using webapi.DB;
using webapi.Exceptions;
using webapi.Interfaces.Cryptography;
using webapi.Interfaces.Redis;
using webapi.Interfaces.Services;
using webapi.Interfaces.SQL;
using webapi.Localization;
using webapi.Models;
using webapi.Services;

namespace webapi.Controllers.Admin.Manage_Encryption_Keys
{
    [Route("api/admin/keys")]
    [ApiController]
    [Authorize(Roles = "HighestAdmin")]
    [ValidateAntiForgeryToken]
    public class UpdateKeysController : ControllerBase
    {
        private readonly FileCryptDbContext _dbContext;
        private readonly IRead<KeyModel> _readKeys;
        private readonly IUpdate<KeyModel> _updateKeys;
        private readonly IConfiguration _configuration;
        private readonly IDecryptKey _decryptKey;
        private readonly ILogger<UpdateKeysController> _logger;
        private readonly IRedisCache _redisCache;
        private readonly IUserInfo _userInfo;
        private readonly byte[] secretKey;

        public UpdateKeysController(
            FileCryptDbContext dbContext,
            IConfiguration configuration,
            IDecryptKey decryptKey,
            ILogger<UpdateKeysController> logger,
            IRedisCache redisCache,
            IUserInfo userInfo)
        {
            _dbContext = dbContext;
            _configuration = configuration;
            _decryptKey = decryptKey;
            _logger = logger;
            _redisCache = redisCache;
            _userInfo = userInfo;
            secretKey = Convert.FromBase64String(_configuration[App.ENCRYPTION_KEY]!);
        }

        [HttpPut("revoke/received/{userId}")]
        public async Task<IActionResult> RevokeReceivedKey([FromRoute] int userId)
        {
            try
            {
                var existingUser = await _readKeys.ReadById(userId, true);

                var keyModel = new KeyModel
                {
                    user_id = userId,
                    private_key = await _decryptKey.DecryptionKeyAsync(existingUser.private_key, secretKey),
                    received_key = null
                };

                await _updateKeys.Update(keyModel, true);
                await _redisCache.DeleteCache("receivedKey#" + userId);

                _logger.LogInformation($"{_userInfo.Username}#{_userInfo.UserId} revoked received key from user#{userId}");

                return StatusCode(204, new { message = SuccessMessage.ReceivedKeyRevoked });
            }
            catch (UserException ex)
            {
                return StatusCode(404, new { message = ex.Message });
            }
        }
    }
}
