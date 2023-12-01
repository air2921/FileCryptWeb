using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using webapi.DB;
using webapi.Exceptions;
using webapi.Interfaces.SQL;
using webapi.Localization.English;

namespace webapi.Controllers.Admin.Manage_Encryption_Keys
{
    [Route("api/admin/keys")]
    [ApiController]
    [Authorize(Roles = "HighestAdmin")]
    public class UpdateKeysController : ControllerBase
    {
        private readonly FileCryptDbContext _dbContext;
        private readonly IUpdateKeys _updateKeys;

        public UpdateKeysController(FileCryptDbContext dbContext, IUpdateKeys updateKeys)
        {
            _dbContext = dbContext;
            _updateKeys = updateKeys;
        }

        [HttpPut("revoke/received")]
        public async Task<IActionResult> RevokeReceivedKey([FromBody] int userId)
        {
            try
            {
                await _updateKeys.CleanReceivedInternalKey(userId);

                return StatusCode(204, new { message = SuccessMessage.ReceivedKeyRevoked });
            }
            catch (UserException ex)
            {
                return StatusCode(404, new { message = ex.Message });
            }
        }

        [HttpPut("revoke/internal")]
        [Authorize(Roles = "HighestAdmin")]
        public async Task<IActionResult> RevokeInternal([FromBody] int userId)
        {
            try
            {
                var key = await _dbContext.Keys.FirstOrDefaultAsync(k => k.user_id == userId);
                if (key is null)
                    return StatusCode(404, new { ExceptionUserMessages.UserNotFound });

                key.person_internal_key = null;
                await _dbContext.SaveChangesAsync();

                return StatusCode(200, new { message = SuccessMessage.InternalKeyRevoked });
            }
            catch (UserException ex)
            {
                return StatusCode(404, new { message = ex.Message });
            }
        }

        [HttpPut("revoke/private")]
        [Authorize(Roles = "HighestAdmin")]
        public async Task<IActionResult> RevokePrivate([FromBody] int userId)
        {
            try
            {
                var key = await _dbContext.Keys.FirstOrDefaultAsync(k => k.user_id == userId);
                if (key is null)
                    return StatusCode(404, new { ExceptionUserMessages.UserNotFound });

                key.private_key = null;
                await _dbContext.SaveChangesAsync();

                return StatusCode(200, new { message = SuccessMessage.InternalKeyRevoked });
            }
            catch (UserException ex)
            {
                return StatusCode(404, new { message = ex.Message });
            }
        }
    }
}
