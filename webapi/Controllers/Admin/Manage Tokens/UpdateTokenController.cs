using Microsoft.AspNetCore.Mvc;
using webapi.DB;
using webapi.Exceptions;
using webapi.Interfaces.SQL;
using webapi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using webapi.Localization;
using webapi.Localization.Exceptions;
using webapi.Controllers.Admin.Manage_Notifications;
using webapi.Interfaces.Services;

namespace webapi.Controllers.Admin.Manage_Tokens
{
    [Route("api/admin/tokens")]
    [ApiController]
    [Authorize(Roles = "HighestAdmin,Admin")]
    [ValidateAntiForgeryToken]
    public class UpdateTokenController : ControllerBase
    {
        private readonly IUserInfo _userInfo;
        private readonly ILogger<DeleteNotificationController> _logger;
        private readonly IUpdate<TokenModel> _update;
        private readonly FileCryptDbContext _dbContext;

        public UpdateTokenController(
            IUserInfo userInfo,
            ILogger<DeleteNotificationController> logger,
            IUpdate<TokenModel> update,
            FileCryptDbContext dbContext)
        {
            _userInfo = userInfo;
            _logger = logger;
            _update = update;
            _dbContext = dbContext;
        }

        [HttpPut("revoke/refresh/{userId}")]
        public async Task<IActionResult> RevokeRefreshToken([FromRoute] int userId)
        {
            try
            {
                var tokenModel = new TokenModel { user_id = userId, refresh_token = Guid.NewGuid().ToString(), expiry_date = DateTime.UtcNow.AddYears(-100) };

                var targetUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.id == userId);

                if (targetUser is null)
                    return StatusCode(404, new { message = ExceptionUserMessages.UserNotFound });

                if (!User.IsInRole("HighestAdmin") && targetUser.role == "HighestAdmin")
                    return StatusCode(403, new { message = ErrorMessage.HighestRoleError });

                _logger.LogCritical($"{_userInfo.Username}#{_userInfo.UserId} revoked refresh token from user#{userId}");

                await _update.Update(tokenModel, true);

                return StatusCode(200, new { message = SuccessMessage.SuccessRefreshRevoked });
            }
            catch (TokenException ex)
            {
                return StatusCode(404, new { message = ex.Message });
            }
        }
    }
}
