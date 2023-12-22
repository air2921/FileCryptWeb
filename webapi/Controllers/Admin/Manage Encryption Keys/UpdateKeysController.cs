using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using webapi.DB;
using webapi.Exceptions;
using webapi.Interfaces.Redis;
using webapi.Interfaces.Services;
using webapi.Interfaces.SQL;
using webapi.Localization;
using webapi.Localization.Exceptions;

namespace webapi.Controllers.Admin.Manage_Encryption_Keys
{
    [Route("api/admin/keys")]
    [ApiController]
    [Authorize(Roles = "HighestAdmin")]
    [ValidateAntiForgeryToken]
    public class UpdateKeysController : ControllerBase
    {
        private readonly FileCryptDbContext _dbContext;
        private readonly ILogger<UpdateKeysController> _logger;
        private readonly IRedisCache _redisCache;
        private readonly IUserInfo _userInfo;
        private readonly IUpdateKeys _updateKeys;

        public UpdateKeysController(
            FileCryptDbContext dbContext,
            ILogger<UpdateKeysController> logger,
            IRedisCache redisCache,
            IUserInfo userInfo,
            IUpdateKeys updateKeys)
        {
            _dbContext = dbContext;
            _logger = logger;
            _redisCache = redisCache;
            _userInfo = userInfo;
            _updateKeys = updateKeys;
        }

        [HttpPut("revoke/received/{userId}")]
        public async Task<IActionResult> RevokeReceivedKey([FromRoute] int userId)
        {
            try
            {
                await _updateKeys.CleanReceivedInternalKey(userId);
                await _redisCache.DeleteCache("receivedInternalKey#" + userId);
                _logger.LogInformation($"{_userInfo.Username}#{_userInfo.UserId} revoked received key from user#{userId}");

                return StatusCode(204, new { message = SuccessMessage.ReceivedKeyRevoked });
            }
            catch (UserException ex)
            {
                _logger.LogWarning($"user#{userId} not exists");
                return StatusCode(404, new { message = ex.Message });
            }
        }

        [HttpPut("revoke/internal/{userId}")]
        [Authorize(Roles = "HighestAdmin")]
        public async Task<IActionResult> RevokeInternal([FromRoute] int userId)
        {
            try
            {
                var key = await _dbContext.Keys.FirstOrDefaultAsync(k => k.user_id == userId);
                if (key is null)
                    return StatusCode(404, new { ExceptionUserMessages.UserNotFound });

                key.person_internal_key = null;
                await _dbContext.SaveChangesAsync();
                await _redisCache.DeleteCache("personalInternalKey#" + userId);
                _logger.LogInformation($"{_userInfo.Username}#{_userInfo.UserId} revoked internal key from user#{userId}");
                
                return StatusCode(200, new { message = SuccessMessage.InternalKeyRevoked });
            }
            catch (UserException ex)
            {
                _logger.LogWarning($"user#{userId} not exists");
                return StatusCode(404, new { message = ex.Message });
            }
        }

        [HttpPut("revoke/private/{userId}")]
        [Authorize(Roles = "HighestAdmin")]
        public async Task<IActionResult> RevokePrivate([FromRoute] int userId)
        {
            try
            {
                var key = await _dbContext.Keys.FirstOrDefaultAsync(k => k.user_id == userId);
                if (key is null)
                    return StatusCode(404, new { ExceptionUserMessages.UserNotFound });

                key.private_key = null;
                await _dbContext.SaveChangesAsync();
                await _redisCache.DeleteCache("privateKey#" + userId);
                _logger.LogInformation($"{_userInfo.Username}#{_userInfo.UserId} revoked private key from user#{userId}");

                return StatusCode(200, new { message = SuccessMessage.InternalKeyRevoked });
            }
            catch (UserException ex)
            {
                _logger.LogWarning($"user#{userId} not exists");
                return StatusCode(404, new { message = ex.Message });
            }
        }
    }
}
