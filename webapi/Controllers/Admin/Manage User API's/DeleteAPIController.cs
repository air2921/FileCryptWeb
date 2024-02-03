using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using webapi.Controllers.Admin.Manage_Notifications;
using webapi.DB.SQL;
using webapi.Exceptions;
using webapi.Interfaces.Services;
using webapi.Interfaces.SQL;
using webapi.Localization;
using webapi.Models;

namespace webapi.Controllers.Admin.Manage_User_s_API
{
    [Route("api/admin/api")]
    [ApiController]
    [Authorize(Roles = "HighestAdmin")]
    [ValidateAntiForgeryToken]
    public class DeleteAPIController : ControllerBase
    {
        private readonly IUserInfo _userInfo;
        private readonly ILogger<DeleteNotificationController> _logger;
        private readonly IDelete<ApiModel> _deleteAPIById;
        private readonly IDeleteByName<ApiModel> _deleteAPIByName;

        public DeleteAPIController(
            IUserInfo userInfo,
            ILogger<DeleteNotificationController> logger,
            IDelete<ApiModel> deleteAPIById,
            IDeleteByName<ApiModel> deleteAPIByName)
        {
            _userInfo = userInfo;
            _logger = logger;
            _deleteAPIById = deleteAPIById;
            _deleteAPIByName = deleteAPIByName;
        }

        [HttpDelete("revoke/key")]
        public async Task<IActionResult> RevokeAPI([FromQuery] int userId, [FromQuery] int? apiId, [FromQuery] string? apikey )
        {
            try
            {
                if (apiId.HasValue)
                {
                    await _deleteAPIById.DeleteById(apiId.Value, null);
                    _logger.LogWarning($"{_userInfo.Username}#{_userInfo.UserId} revoked API key from user#{userId}");

                    return StatusCode(200, new { message = SuccessMessage.SuccessApiRevoked });
                }

                if (string.IsNullOrWhiteSpace(apikey))
                    return StatusCode(400);

                await _deleteAPIByName.DeleteByName(apikey, userId);
                _logger.LogWarning($"{_userInfo.Username}#{_userInfo.UserId} revoked API key #{apikey}");

                return StatusCode(200, new { message = SuccessMessage.SuccessApiRevoked });
            }
            catch (ApiException ex)
            {
                return StatusCode(404, new { message = ex.Message });
            }
        }
    }
}
