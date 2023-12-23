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
        public async Task<IActionResult> DeleteOneFile([FromBody] FileModel fileModel, [FromQuery] bool byID)
        {
            try
            {
                if (byID)
                {
                    await _deleteById.DeleteById(fileModel.file_id, fileModel.user_id);
                    _logger.LogInformation($"{_userInfo.Username}#{_userInfo.UserId} deleted file from history #{fileModel.file_id}");
                    return StatusCode(200);
                }

                await _deleteByName.DeleteByName(fileModel.file_name, fileModel.user_id);
                _logger.LogInformation($"{_userInfo.Username}#{_userInfo.UserId} deleted file from history by name: '{fileModel.file_name}'");
                return StatusCode(200);
            }
            catch (FileException ex)
            {
                return StatusCode(404, new { message = ex.Message });
            }
        }
    }
}
