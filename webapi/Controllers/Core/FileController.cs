using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using webapi.DB.SQL.Files;
using webapi.Exceptions;
using webapi.Interfaces.SQL;
using webapi.Interfaces.SQL.Files;
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
        private readonly IReadFile _readFile;

        public FileController(IDelete<FileModel> deleteFileById, IDeleteByName<FileModel> deleteFileByName, IReadFile readFile)
        {
            _deleteFileById = deleteFileById;
            _deleteFileByName = deleteFileByName;
            _readFile = readFile;
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

        [HttpGet("get/one/{byID}")]
        public async Task<IActionResult> GetOneFile(FileModel fileModel, [FromRoute] bool byID)
        {
            try
            {
                var file = new FileModel();

                if (byID)
                {
                    file = await _readFile.ReadFileByIdOrName(fileModel, ReadFile.FILE_ID);

                    return StatusCode(200, new { file });
                }

                file = await _readFile.ReadFileByIdOrName(fileModel, ReadFile.FILE_NAME);

                return StatusCode(200, new { file });
            }
            catch (FileException ex)
            {
                return StatusCode(404, new { message = ex.Message });
            }
        }
    }
}
