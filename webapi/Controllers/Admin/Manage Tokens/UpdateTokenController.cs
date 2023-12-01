using Microsoft.AspNetCore.Mvc;
using webapi.DB;
using webapi.Exceptions;
using webapi.Interfaces.SQL;
using webapi.Localization.English;
using webapi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace webapi.Controllers.Admin.Manage_Tokens
{
    [Route("api/admin/tokens")]
    [ApiController]
    [Authorize(Roles = "HighestAdmin,Admin")]
    public class UpdateTokenController : ControllerBase
    {
        private readonly IUpdate<TokenModel> _update;
        private readonly FileCryptDbContext _dbContext;

        public UpdateTokenController(IUpdate<TokenModel> update, FileCryptDbContext dbContext)
        {
            _update = update;
            _dbContext = dbContext;
        }

        [HttpPut("revoke/refresh")]
        public async Task<IActionResult> RevokeRefreshToken([FromBody] int id)
        {
            try
            {
                var tokenModel = new TokenModel { user_id = id, refresh_token = null, expiry_date = DateTime.UtcNow.AddYears(-100) };

                var targetUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.id == id);

                if (targetUser is null)
                    return StatusCode(404, new { message = ExceptionUserMessages.UserNotFound });

                if (!User.IsInRole("HighestAdmin") && targetUser.role == "HighestAdmin")
                    return StatusCode(403, new { message = ErrorMessage.HighestRoleError });

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
