using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using webapi.Exceptions;
using webapi.Interfaces.SQL;
using webapi.Models;

namespace webapi.Controllers.Admin.Manage_Files
{
    [Route("api/admin/files/get")]
    [ApiController]
    [Authorize(Roles = "HighestAdmin,Admin")]
    public class ReadFileController : ControllerBase
    {
        private readonly IRead<FileModel> _read;

        public ReadFileController(IRead<FileModel> read)
        {
            _read = read;
        }

        [HttpGet("one")]
        public async Task<IActionResult> ReadOneFile([FromBody] int id)
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
