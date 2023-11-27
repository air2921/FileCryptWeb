﻿using Microsoft.AspNetCore.Mvc;
using webapi.DB.SQL.Tokens;
using webapi.DB;
using webapi.Exceptions;
using webapi.Interfaces.SQL.Tokens;
using webapi.Localization.English;
using webapi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace webapi.Controllers.Admin.Manage_Tokens
{
    [Route("api/admin/tokens/update")]
    [ApiController]
    [Authorize(Roles = "HighestAdmin,Admin")]
    public class UpdateTokenController : ControllerBase
    {
        private readonly IUpdateToken _updateToken;
        private readonly FileCryptDbContext _dbContext;

        public UpdateTokenController(IUpdateToken updateToken, FileCryptDbContext dbContext)
        {
            _updateToken = updateToken;
            _dbContext = dbContext;
        }

        [HttpPut("revoke/refresh")]
        public async Task<IActionResult> RevokeRefreshToken(int id)
        {
            try
            {
                var tokenModel = new TokenModel { user_id = id, refresh_token = null, expiry_date = DateTime.UtcNow.AddYears(-100) };

                var targetUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.id == id);

                if (targetUser is null)
                    return StatusCode(404, new { message = ExceptionUserMessages.UserNotFound });

                if (!User.IsInRole("HighestAdmin") && targetUser.role == "HighestAdmin")
                    return StatusCode(403, new { message = ErrorMessage.HighestRoleError });

                await _updateToken.UpdateRefreshToken(tokenModel, UpdateToken.USER_ID);

                return StatusCode(200, new { message = SuccessMessage.SuccessRefreshRevoked });
            }
            catch (TokenException ex)
            {
                return StatusCode(404, new { message = ex.Message });
            }
        }
    }
}
