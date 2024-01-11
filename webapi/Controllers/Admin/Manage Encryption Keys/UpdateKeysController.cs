using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using webapi.DB;
using webapi.Interfaces.Cryptography;
using webapi.Interfaces.Redis;
using webapi.Interfaces.Services;
using webapi.Interfaces.SQL;
using webapi.Localization;
using webapi.Localization.Exceptions;
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
        private readonly IConfiguration _configuration;
        private readonly IEncryptKey _encryptKey;
        private readonly IGenerateKey _generateKey;
        private readonly ILogger<UpdateKeysController> _logger;
        private readonly IRedisCache _redisCache;
        private readonly IUserInfo _userInfo;
        private readonly byte[] secretKey;

        public UpdateKeysController(
            FileCryptDbContext dbContext,
            IConfiguration configuration,
            IEncryptKey encryptKey,
            IGenerateKey generateKey,
            ILogger<UpdateKeysController> logger,
            IRedisCache redisCache,
            IUserInfo userInfo)
        {
            _dbContext = dbContext;
            _configuration = configuration;
            _encryptKey = encryptKey;
            _generateKey = generateKey;
            _logger = logger;
            _redisCache = redisCache;
            _userInfo = userInfo;
            secretKey = Convert.FromBase64String(_configuration[App.appKey]!);
        }

        [HttpPut("revoke/received/{userId}")]
        public async Task<IActionResult> RevokeReceivedKey([FromRoute] int userId)
        {
            var keys = await _dbContext.Keys.FirstOrDefaultAsync(k => k.user_id == userId);
            if (keys is null)
                return StatusCode(404, new { message = ExceptionUserMessages.UserNotFound });

            keys.received_key = null;
            await _dbContext.SaveChangesAsync();

            await _redisCache.DeleteCache("receivedKey#" + userId);
            _logger.LogInformation($"{_userInfo.Username}#{_userInfo.UserId} revoked received key from user#{userId}");

            return StatusCode(204, new { message = SuccessMessage.ReceivedKeyRevoked });
        }

        [HttpPut("revoke/internal/{userId}")]
        public async Task<IActionResult> RevokeInternal([FromRoute] int userId)
        {
            var key = await _dbContext.Keys.FirstOrDefaultAsync(k => k.user_id == userId);
            if (key is null)
                return StatusCode(404, new { ExceptionUserMessages.UserNotFound });

            key.internal_key = null;
            await _dbContext.SaveChangesAsync();
            await _redisCache.DeleteCache("internalKey#" + userId);
            _logger.LogInformation($"{_userInfo.Username}#{_userInfo.UserId} revoked internal key from user#{userId}");

            return StatusCode(200, new { message = SuccessMessage.InternalKeyRevoked });
        }

        [HttpPut("revoke/private/{userId}")]
        public async Task<IActionResult> RevokePrivate([FromRoute] int userId)
        {
            var key = await _dbContext.Keys.FirstOrDefaultAsync(k => k.user_id == userId);
            if (key is null)
                return StatusCode(404, new { ExceptionUserMessages.UserNotFound });

            key.private_key = await _encryptKey.EncryptionKeyAsync(_generateKey.GenerateKey(), secretKey);
            await _dbContext.SaveChangesAsync();
            await _redisCache.DeleteCache("privateKey#" + userId);
            _logger.LogInformation($"{_userInfo.Username}#{_userInfo.UserId} revoked private key from user#{userId}");

            return StatusCode(200, new { message = SuccessMessage.PrivateKeyRevoked });
        }
    }
}
