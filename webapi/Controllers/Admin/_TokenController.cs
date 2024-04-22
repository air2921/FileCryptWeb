using application.Abstractions.Endpoints.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using webapi.Helpers.Abstractions;

namespace webapi.Controllers.Admin
{
    [Route("api/admin/token")]
    [ApiController]
    [Authorize(Policy = "RequireAdminPolicy")]
    public class _TokenController(
        IAdminTokenService service,
        IUserInfo userInfo) : ControllerBase
    {
        [HttpDelete("{tokenId}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteToken([FromRoute] int tokenId)
        {
            var response = await service.RevokeToken(tokenId, userInfo.Role);
            return StatusCode(response.Status, new { message = response.Message });
        }

        [HttpDelete("range/{userId}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteRangeTokens([FromRoute] int userId)
        {
            var response = await service.RevokeAllUserTokens(userId, userInfo.Role);
            return StatusCode(response.Status, new { message = response.Message });
        }
    }
}
