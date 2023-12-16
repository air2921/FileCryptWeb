using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using webapi.DB;
using webapi.Exceptions;
using webapi.Interfaces.Services;
using webapi.Interfaces.SQL;
using webapi.Models;

namespace webapi.Controllers.Admin.Manage_Links
{
    [Route("api/admin/links")]
    [ApiController]
    [Authorize(Roles = "HighestAdmin,Admin")]
    public class ReadLinksController : ControllerBase
    {
        private readonly FileCryptDbContext _dbContext;
        private readonly IUserInfo _userInfo;
        private readonly ILogger<ReadLinksController> _logger;
        private readonly IRead<LinkModel> _readLinks;

        public ReadLinksController(
            FileCryptDbContext dbContext,
            IUserInfo userInfo,
            ILogger<ReadLinksController> logger,
            IRead<LinkModel> readLinks)
        {
            _dbContext = dbContext;
            _userInfo = userInfo;
            _logger = logger;
            _readLinks = readLinks;
        }

        [HttpGet("{tokenId}")]
        public async Task<IActionResult> ReadById([FromRoute] int tokenId)
        {
            try
            {
                var link = await _readLinks.ReadById(tokenId, false);
                _logger.LogInformation($"{_userInfo.Username}#{_userInfo.UserId} requested information about link #{tokenId}");

                return StatusCode(200, new { link });
            }
            catch (LinkException ex)
            {
                return StatusCode(404, new { message = ex.Message });
            }
        }

        [HttpGet("all")]
        public async Task<IActionResult> ReadAll()
        {
            try
            {
                var links = await _readLinks.ReadAll();
                _logger.LogInformation($"{_userInfo.Username}#{_userInfo.UserId} requested information about all links");

                return StatusCode(200, new { links });
            }
            catch (LinkException ex)
            {
                return StatusCode(404, new { message = ex.Message });
            }
        }

        [HttpGet("all/expires")]
        public async Task<IActionResult> ReadAllExpires()
        {
            var links = await _dbContext.Links.Where(l => l.expiry_date < DateTime.UtcNow).ToListAsync();
            if (links is null)
                return StatusCode(404, new { message = "No one expired links was not found" });

            _logger.LogInformation($"{_userInfo.Username}#{_userInfo.UserId} requested information about all expired links");

            return StatusCode(200, new { links });
        }
    }
}
