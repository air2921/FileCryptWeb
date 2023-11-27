using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using webapi.Exceptions;
using webapi.Interfaces.SQL.Files.Mimes;
using webapi.Models;

namespace webapi.Controllers.Admin.Manage_Files.Manage_MIME_s
{
    [Route("api/admin/mime/get")]
    [ApiController]
    [Authorize(Roles = "HighestAdmin")]
    public class ReadMIMEController : ControllerBase
    {
        private readonly IReadMime _readMime;

        public ReadMIMEController(IReadMime readMime)
        {
            _readMime = readMime;
        }

        [HttpGet("one")]
        public async Task<IActionResult> ReadOneMime(FileMimeModel mimeModel)
        {
            try
            {
                var mime = await _readMime.ReadMimeById(mimeModel.mime_id);

                return StatusCode(200, new { type = mime });
            }
            catch (MimeException ex)
            {
                return StatusCode(404, new { message = ex.Message });
            }
        }

        [HttpGet("all")]
        public async Task<IActionResult> ReadAllMimes()
        {
            try
            {
                var mimes = await _readMime.ReadAllMimes();

                return StatusCode(200, new { types = mimes });
            }
            catch (MimeException ex)
            {
                return StatusCode(404, new { message = ex.Message });
            }
        }
    }
}
