using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using webapi.Exceptions;
using webapi.Interfaces.Services;
using webapi.Interfaces.SQL;
using webapi.Models;

namespace webapi.Controllers.Admin.Manage_Files
{
    [Route("api/admin/files")]
    [ApiController]
    [Authorize(Roles = "HighestAdmin,Admin")]
    public class ReadFileController : ControllerBase
    {
        private readonly IUserInfo _userInfo;
        private readonly ILogger<ReadFileController> _logger;
        private readonly IRead<FileModel> _read;

        public ReadFileController(IUserInfo userInfo, ILogger<ReadFileController> logger, IRead<FileModel> read)
        {
            _userInfo = userInfo;
            _logger = logger;
            _read = read;
        }

        [HttpGet("{fileId}")]
        public async Task<IActionResult> ReadOneFile([FromRoute] int fileId)
        {
            try
            {
                var file = await _read.ReadById(fileId, null);
                _logger.LogInformation($"{_userInfo.Username}#{_userInfo.UserId} requested information about file #{fileId}");

                return StatusCode(200, new { file });
            }
            catch (FileException ex)
            {
                return StatusCode(404, new { message = ex.Message });
            }
        }
    }
}
