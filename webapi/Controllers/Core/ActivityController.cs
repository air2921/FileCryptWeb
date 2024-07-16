using application.Master_Services.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using webapi.Helpers.Abstractions;

namespace webapi.Controllers.Core
{
    [Route("api/core/activity")]
    [ApiController]
    [Authorize]
    public class ActivityController(ActivityService service, IUserInfo userInfo) : ControllerBase
    {
        [HttpGet("{activityId}")]
        public async Task<IActionResult> GetActivity([FromRoute] int activityId)
        {
            var response = await service.GetOne(userInfo.UserId, activityId);
            if (!response.IsSuccess)
                return StatusCode(response.Status, new { message = response.Message });
            else
                return StatusCode(response.Status, new { activity = response.ObjectData });
        }

        [HttpGet("range")]
        public async Task<IActionResult> GetRangeActivity([FromQuery] bool byDesc,
            [FromQuery] DateTime start, [FromQuery] DateTime end, [FromQuery] int? type)
        {
            var response = await service.GetRange(userInfo.UserId, byDesc, start, end, type);
            if (!response.IsSuccess)
                return StatusCode(response.Status, new { message = response.Message });
            else
                return StatusCode(response.Status, new { activity = response.ObjectData });
        }
    }
}
