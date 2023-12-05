using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using webapi.DB;
using webapi.Exceptions;
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
        private readonly IRead<LinkModel> _readLinks;

        public ReadLinksController(FileCryptDbContext dbContext, IRead<LinkModel> readLinks)
        {
            _dbContext = dbContext;
            _readLinks = readLinks;
        }

        [HttpGet("{tokenId}")]
        public async Task<IActionResult> ReadById([FromRoute] int tokenId)
        {
            try
            {
                var link = await _readLinks.ReadById(tokenId, false);

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

            return StatusCode(200, new { links });
        }
    }
}
