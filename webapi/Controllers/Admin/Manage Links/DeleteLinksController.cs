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
    public class DeleteLinksController : ControllerBase
    {
        private readonly FileCryptDbContext _dbContext;
        private readonly IDelete<LinkModel> _deleteById;
        private readonly IDeleteByName<LinkModel> _deleteByName;

        public DeleteLinksController(FileCryptDbContext dbContext, IDelete<LinkModel> deleteById, IDeleteByName<LinkModel> deleteByName)
        {
            _dbContext = dbContext;
            _deleteById = deleteById;
            _deleteByName = deleteByName;
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteById([FromRoute] int id)
        {
            try
            {
                await _deleteById.DeleteById(id);

                return StatusCode(200);
            }
            catch (LinkException ex)
            {
                return StatusCode(404, new { message = ex.Message });
            }
        }

        [HttpDelete("{token}")]
        public async Task<IActionResult> DeleteByName([FromRoute] string token)
        {
            try
            {
                await _deleteByName.DeleteByName(token);

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

            return StatusCode(200, new { deleted_links = links });
        }
    }
}
