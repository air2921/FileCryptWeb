using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using webapi.Controllers.Admin.Manage_Notifications;
using webapi.Exceptions;
using webapi.Interfaces.Services;
using webapi.Interfaces.SQL;
using webapi.Models;

namespace webapi.Controllers.Admin.Manage_User_s_API
{
    [Route("api/admin/api")]
    [ApiController]
    [Authorize(Roles = "HighestAdmin")]
    public class UpdateAPIController : ControllerBase
    {
        private readonly IUpdate<ApiModel> _update;
        private readonly IUserInfo _userInfo;
        private readonly ILogger<DeleteNotificationController> _logger;

        public UpdateAPIController(IUpdate<ApiModel> update, IUserInfo userInfo, ILogger<DeleteNotificationController> logger)
        {
            _update = update;
            _userInfo = userInfo;
            _logger = logger;
        }

        [HttpPut("settings")]
        public async Task<IActionResult> UpdateAPI([FromBody] ApiModel apiModel)
        {
            try
            {
                await _update.Update(apiModel, true);
                _logger.LogWarning($"{_userInfo.Username}#{_userInfo.UserId} requested update API settings from user#{apiModel.user_id}");

                return StatusCode(200, new { apiModel });
            }
            catch (ApiException ex)
            {
                return StatusCode(404, new { message = ex.Message });
            }
        }
    }
}
