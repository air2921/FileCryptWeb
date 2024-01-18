using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using webapi.DB;
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
        private readonly FileCryptDbContext _dbContext;
        private readonly ILogger<ReadFileController> _logger;
        private readonly IRead<FileModel> _read;

        public ReadFileController(IUserInfo userInfo, FileCryptDbContext dbContext, ILogger<ReadFileController> logger, IRead<FileModel> read)
        {
            _userInfo = userInfo;
            _dbContext = dbContext;
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

        [HttpGet("all")]
        public async Task<IActionResult> ReadAll([FromQuery] int? userId, [FromQuery] int skip, [FromQuery] int count)
        {
            try
            {
                var files = await _read.ReadAll(userId, skip, count);
                _logger.LogInformation($"{_userInfo.Username}#{_userInfo.UserId} requested information about some files, skipped {skip} and quantity requested {count}");

                return StatusCode(200, new { files });
            }
            catch (FileException ex)
            {
                return StatusCode(404, new { message = ex.Message });
            }
        }
    }
}
