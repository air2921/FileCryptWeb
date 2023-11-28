using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using webapi.Exceptions;
using webapi.Interfaces.SQL;
using webapi.Models;

namespace webapi.Controllers.Admin.Manage_Files
{
    [Route("api/admin/files/delete")]
    [ApiController]
    [Authorize(Roles = "HighestAdmin,Admin")]
    public class DeleteFileController : ControllerBase
    {
        private readonly IDelete<FileModel> _deleteById;
        private readonly IDeleteByName<FileModel> _deleteByName;

        public DeleteFileController(IDelete<FileModel> deleteById, IDeleteByName<FileModel> deleteByName)
        {
            _deleteById = deleteById;
            _deleteByName = deleteByName;
        }

        [HttpDelete("one/file/{byID}")]
        public async Task<IActionResult> DeleteOneFile([FromBody] FileModel fileModel, [FromRoute] bool byID)
        {
            try
            {
                if (byID)
                {
                    await _deleteById.DeleteById(fileModel.file_id);
                    return StatusCode(200);
                }

                await _deleteByName.DeleteByName(fileModel.file_name);
                return StatusCode(200);
            }
            catch (FileException ex)
            {
                return StatusCode(404, new { message = ex.Message });
            }
        }
    }
}
