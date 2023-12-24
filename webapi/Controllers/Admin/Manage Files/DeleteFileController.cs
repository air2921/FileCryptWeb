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
    public class DeleteFileController : ControllerBase
    {
        private readonly IUserInfo _userInfo;
        private readonly ILogger<DeleteFileController> _logger;
        private readonly IDelete<FileModel> _deleteById;
        private readonly IDeleteByName<FileModel> _deleteByName;

        public DeleteFileController(IUserInfo userInfo, ILogger<DeleteFileController> logger, IDelete<FileModel> deleteById, IDeleteByName<FileModel> deleteByName)
        {
            _userInfo = userInfo;
            _logger = logger;
            _deleteById = deleteById;
            _deleteByName = deleteByName;
        }

        [HttpDelete("one")]
        public async Task<IActionResult> DeleteOneFile([FromQuery] int userId, [FromQuery] int? fileId, [FromQuery] string? filename )
        {
            try
            {
                if (fileId.HasValue)
                {
                    await _deleteById.DeleteById(fileId.Value, userId);
                    _logger.LogInformation($"{_userInfo.Username}#{_userInfo.UserId} deleted file from history #{fileId}");
                    return StatusCode(200);
                }

                if (string.IsNullOrWhiteSpace(filename))
                    return StatusCode(400);

                await _deleteByName.DeleteByName(filename, userId);
                _logger.LogInformation($"{_userInfo.Username}#{_userInfo.UserId} deleted file from history by name: '{filename}'");
                return StatusCode(200);
            }
            catch (FileException ex)
            {
                return StatusCode(404, new { message = ex.Message });
            }
        }
    }
}
