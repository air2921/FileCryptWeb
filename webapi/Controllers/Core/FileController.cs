using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using webapi.Exceptions;
using webapi.Interfaces.SQL;
using webapi.Models;

namespace webapi.Controllers.Core
{
    [Route("api/core/file")]
    [ApiController]
    [Authorize]
    public class FileController : ControllerBase
    {
        private readonly IDelete<FileModel> _deleteFileById;
        private readonly IDeleteByName<FileModel> _deleteFileByName;
        private readonly IRead<FileModel> _read;

        public FileController(IDelete<FileModel> deleteFileById, IDeleteByName<FileModel> deleteFileByName, IRead<FileModel> read)
        {
            _deleteFileById = deleteFileById;
            _deleteFileByName = deleteFileByName;
            _read = read;
        }

        [HttpDelete("delete/one/{byID}")]
        public async Task<IActionResult> DeleteFileFromHistory([FromBody] FileModel fileModel, [FromRoute] bool byID)
        {
            try
            {
                if (byID)
                {
                    await _deleteFileById.DeleteById(fileModel.file_id);

                    return StatusCode(200);
                }

                await _deleteFileByName.DeleteByName(fileModel.file_name);

                return StatusCode(200);
            }
            catch (FileException ex)
            {
                return StatusCode(404, new { message = ex.Message });
            }
        }

        [HttpGet("get/one")]
        public async Task<IActionResult> GetOneFile([FromBody] int id)
        {
            try
            {
                var file = await _read.ReadById(id, null);

                return StatusCode(200, new { file });
            }
            catch (FileException ex)
            {
                return StatusCode(404, new { message = ex.Message });
            }
        }
    }
}
