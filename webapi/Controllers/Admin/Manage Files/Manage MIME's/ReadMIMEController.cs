using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using webapi.Exceptions;
using webapi.Interfaces.SQL;
using webapi.Models;

namespace webapi.Controllers.Admin.Manage_Files.Manage_MIME_s
{
    [Route("api/admin/mime")]
    [ApiController]
    [Authorize(Roles = "HighestAdmin,Admin")]
    public class ReadMIMEController : ControllerBase
    {
        private readonly IRead<FileMimeModel> _readMime;

        public ReadMIMEController(IRead<FileMimeModel> readMime)
        {
            _readMime = readMime;
        }

        [HttpGet("one")]
        public async Task<IActionResult> ReadOneMime([FromBody] int id)
        {
            try
            {
                var mime = await _readMime.ReadById(id, null);

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
                var mimes = await _readMime.ReadAll();

                return StatusCode(200, new { types = mimes });
            }
            catch (MimeException ex)
            {
                return StatusCode(404, new { message = ex.Message });
            }
        }
    }
}
