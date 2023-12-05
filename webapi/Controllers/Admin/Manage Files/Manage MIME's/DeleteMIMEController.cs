using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using webapi.DB;
using webapi.Exceptions;
using webapi.Interfaces.SQL;
using webapi.Models;

namespace webapi.Controllers.Admin.Manage_Files.Manage_MIME_s
{
    [Route("api/admin/mime")]
    [ApiController]
    [Authorize(Roles = "HighestAdmin,Admin")]
    public class DeleteMIMEController : ControllerBase
    {
        private readonly FileCryptDbContext _dbContext;
        private readonly IDelete<FileMimeModel> _deleteMime;
        private readonly IDeleteByName<FileMimeModel> _deleteMimeByName;

        public DeleteMIMEController(FileCryptDbContext dbContext, IDelete<FileMimeModel> deleteMime, IDeleteByName<FileMimeModel> deleteMimeByName)
        {
            _dbContext = dbContext;
            _deleteMime = deleteMime;
            _deleteMimeByName = deleteMimeByName;
        }

        [HttpDelete("one")]
        public async Task<IActionResult> DeleteOneMime([FromBody] FileMimeModel mimeModel, [FromQuery] bool byID)
        {
            try
            {
                if(byID)
                {
                    await _deleteMime.DeleteById(mimeModel.mime_id);

                    return StatusCode(200);
                }

                await _deleteMimeByName.DeleteByName(mimeModel.mime_name);

                return StatusCode(200);
            }
            catch (MimeException ex)
            {
                return StatusCode(404, new { message = ex.Message });
            }
        }

        [HttpDelete("all")]
        public async Task<IActionResult> DeleteAllMimes()
        {
            var mimes = await _dbContext.Mimes.ToListAsync();

            _dbContext.RemoveRange(mimes);
            await _dbContext.SaveChangesAsync();

            return StatusCode(200);
        }
    }
}
