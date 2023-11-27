using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using webapi.DB.SQL.Files;
using webapi.Exceptions;
using webapi.Interfaces.SQL.Files;
using webapi.Models;

namespace webapi.Controllers.Admin.Manage_Files
{
    [Route("api/admin/files/delete")]
    [ApiController]
    [Authorize(Roles = "HighestAdmin,Admin")]
    public class DeleteFileController : ControllerBase
    {
        private readonly IDeleteFile _deleteFile;

        public DeleteFileController(IDeleteFile deleteFile)
        {
            _deleteFile = deleteFile;
        }

        [HttpDelete("one/file/{byID}")]
        public async Task<IActionResult> DeleteOneFile(FileModel fileModel, [FromRoute] bool byID)
        {
            try
            {
                if (byID)
                {
                    await _deleteFile.DeleteFileByNameOrID(fileModel, DeleteFile.FILE_ID);

                    return StatusCode(200);
                }

                await _deleteFile.DeleteFileByNameOrID(fileModel, DeleteFile.FILE_NAME);

                return StatusCode(200);
            }
            catch (FileException ex)
            {
                return StatusCode(404, new { message = ex.Message });
            }
            catch (ArgumentException)
            {
                return StatusCode(503);
            }
        }

        [HttpDelete("all/files")]
        public async Task<IActionResult> DeleteFiles(int userId)
        {
            await _deleteFile.DeleteAllUserFiles(userId);

            return StatusCode(200);
        }
    }
}
