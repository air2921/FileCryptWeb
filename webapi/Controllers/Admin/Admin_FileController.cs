using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using webapi.DB;
using webapi.Exceptions;
using webapi.Interfaces;
using webapi.Models;

namespace webapi.Controllers.Admin
{
    [Route("api/admin/files")]
    [ApiController]
    [Authorize(Roles = "HighestAdmin,Admin")]
    public class Admin_FileController : ControllerBase
    {
        private readonly IRepository<FileModel> _fileRepository;
        private readonly ISorting _sorting;
        private readonly ILogger<Admin_FileController> _logger;

        public Admin_FileController(IRepository<FileModel> fileRepository, ISorting sorting, ILogger<Admin_FileController> logger)
        {
            _fileRepository = fileRepository;
            _sorting = sorting;
            _logger = logger;
        }

        [HttpGet("fileId")]
        public async Task<IActionResult> GetFile([FromRoute] int fileId)
        {
            var file = await _fileRepository.GetById(fileId);
            if (file is null)
                return StatusCode(404);

            return StatusCode(200, new { file });
        }

        [HttpGet("many")]
        public async Task<IActionResult> GetFiles([FromQuery] int? userId, [FromQuery] int skip, [FromQuery] int count, [FromQuery] bool byDesc)
        {
            return StatusCode(200, new { files = await _fileRepository.GetAll(_sorting.SortFiles(userId, skip, count, byDesc, null, null)) });
        }

        [HttpDelete("fileId")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteFile([FromRoute] int fileId)
        {
            try
            {
                await _fileRepository.Delete(fileId);
                return StatusCode(204);
            }
            catch (EntityNotDeletedException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpDelete("many")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteRangeFiles([FromBody] IEnumerable<int> identifiers)
        {
            try
            {
                await _fileRepository.DeleteMany(identifiers);
                return StatusCode(204);
            }
            catch (EntityNotDeletedException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}
