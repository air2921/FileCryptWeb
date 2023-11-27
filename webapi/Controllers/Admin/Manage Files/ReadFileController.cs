using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using webapi.DB.SQL.Files;
using webapi.Exceptions;
using webapi.Interfaces.SQL.Files;
using webapi.Models;

namespace webapi.Controllers.Admin.Manage_Files
{
    [Route("api/admin/files/get")]
    [ApiController]
    [Authorize(Roles = "HighestAdmin,Admin")]
    public class ReadFileController : ControllerBase
    {
        private readonly IReadFile _readFile;

        public ReadFileController(IReadFile readFile)
        {
            _readFile = readFile;
        }

        [HttpGet("one/file/{byID}")]
        public async Task<IActionResult> ReadOneFile(FileModel fileModel, [FromRoute] bool byID)
        {
            try
            {
                if (byID)
                {
                    await _readFile.ReadFileByIdOrName(fileModel, ReadFile.FILE_ID);

                    return StatusCode(200);
                }

                await _readFile.ReadFileByIdOrName(fileModel, ReadFile.FILE_NAME);

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
    }
}
