using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using webapi.Exceptions;
using webapi.Interfaces.Services;
using webapi.Interfaces.SQL;
using webapi.Models;

namespace webapi.Controllers.Admin.Manage_Files.Manage_MIME_s
{
    [Route("api/admin/mime")]
    [ApiController]
    [Authorize(Roles = "HighestAdmin,Admin")]
    public class ReadMIMEController : ControllerBase
    {
        private readonly IUserInfo _userInfo;
        private readonly ILogger<ReadMIMEController> _logger;
        private readonly IRead<FileMimeModel> _readMime;

        public ReadMIMEController(IUserInfo userInfo, ILogger<ReadMIMEController> logger, IRead<FileMimeModel> readMime)
        {
            _userInfo = userInfo;
            _logger = logger;
            _readMime = readMime;
        }

        [HttpGet("{mimeId}")]
        public async Task<IActionResult> ReadOneMime([FromRoute] int mimeId)
        {
            try
            {
                var mime = await _readMime.ReadById(mimeId, null);
                _logger.LogInformation($"{_userInfo.Username}#{_userInfo.UserId} requested MIME type #{mimeId}");

                return StatusCode(200, new { mime });
            }
            catch (MimeException ex)
            {
                return StatusCode(404, new { message = ex.Message });
            }
        }

        [HttpGet("all")]
        public async Task<IActionResult> ReadAllMimes([FromQuery] int skip, [FromQuery] int count)
        {
            try
            {
                var mimes = await _readMime.ReadAll(null, skip, count);
                _logger.LogInformation($"{_userInfo.Username}#{_userInfo.UserId} requsted MIME collection, skipped {skip} and quantity requested {count}");

                return StatusCode(200, new { mimes });
            }
            catch (MimeException ex)
            {
                return StatusCode(404, new { message = ex.Message });
            }
        }
    }
}
