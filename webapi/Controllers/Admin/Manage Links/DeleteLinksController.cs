using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using webapi.DB;
using webapi.DB.SQL;
using webapi.Exceptions;
using webapi.Interfaces.Services;
using webapi.Interfaces.SQL;
using webapi.Models;

namespace webapi.Controllers.Admin.Manage_Links
{
    [Route("api/admin/links")]
    [ApiController]
    [Authorize(Roles = "HighestAdmin,Admin")]
    public class DeleteLinksController : ControllerBase
    {
        private readonly FileCryptDbContext _dbContext;
        private readonly IUserInfo _userInfo;
        private readonly ILogger<DeleteLinksController> _logger;
        private readonly IDelete<LinkModel> _deleteById;
        private readonly IDeleteByName<LinkModel> _deleteByName;

        public DeleteLinksController(
            FileCryptDbContext dbContext,
            IUserInfo userInfo,
            ILogger<DeleteLinksController> logger,
            IDelete<LinkModel> deleteById,
            IDeleteByName<LinkModel> deleteByName)
        {
            _dbContext = dbContext;
            _userInfo = userInfo;
            _logger = logger;
            _deleteById = deleteById;
            _deleteByName = deleteByName;
        }

        [HttpDelete("{tokenId}")]
        public async Task<IActionResult> DeleteById([FromRoute] int tokenId)
        {
            try
            {
                await _deleteById.DeleteById(tokenId);
                _logger.LogWarning($"{_userInfo.Username}#{_userInfo.UserId} deleted recovery link #{tokenId} from db");

                return StatusCode(200);
            }
            catch (LinkException ex)
            {
                return StatusCode(404, new { message = ex.Message });
            }
        }

        [HttpDelete("revoke")]
        public async Task<IActionResult> DeleteByName([FromQuery] string token)
        {
            try
            {
                await _deleteByName.DeleteByName(token);
                _logger.LogWarning($"{_userInfo.Username}#{_userInfo.UserId} delete recovery link by name: '{token}' from db");

                return StatusCode(200);
            }
            catch (LinkException ex)
            {
                return StatusCode(404, new { message = ex.Message });
            }
        }

        [HttpDelete("all/expired")]
        public async Task<IActionResult> DeleteAllExpired()
        {
            var links = await _dbContext.Links.Where(l => l.expiry_date < DateTime.UtcNow).ToListAsync();
            if (links is null)
                return StatusCode(404, new { message = "No one expired links was not found" });

            _dbContext.RemoveRange(links);
            await _dbContext.SaveChangesAsync();
            _logger.LogInformation($"{_userInfo.Username}#{_userInfo.UserId} delete all expired links from db");

            return StatusCode(200, new { deleted_links = links });
        }
    }
}
