using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using webapi.DB;
using webapi.Exceptions;
using webapi.Interfaces.Services;
using webapi.Interfaces.SQL;
using webapi.Localization;
using webapi.Localization.Exceptions;
using webapi.Models;

namespace webapi.Controllers.Core
{
    [Route("api/core/files")]
    [ApiController]
    [Authorize]
    public class FileController : ControllerBase
    {
        private readonly FileCryptDbContext _dbContext;
        private readonly IUserInfo _userInfo;
        private readonly IDelete<FileModel> _deleteFileById;
        private readonly IDeleteByName<FileModel> _deleteFileByName;
        private readonly IRead<FileModel> _readFile;

        public FileController(
            FileCryptDbContext dbContext,
            IUserInfo userInfo,
            IDelete<FileModel> deleteFileById,
            IDeleteByName<FileModel> deleteFileByName,
            IRead<FileModel> readFile)
        {
            _dbContext = dbContext;
            _userInfo = userInfo;
            _deleteFileById = deleteFileById;
            _deleteFileByName = deleteFileByName;
            _readFile = readFile;
        }

        [HttpDelete]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteFileFromHistory([FromQuery] int? fileId, [FromQuery] string? filename, [FromQuery] bool byId)
        {
            try
            {
                if (fileId.HasValue && byId)
                {
                    await _deleteFileById.DeleteById(fileId.Value, _userInfo.UserId);

                    return StatusCode(200, new { message = SuccessMessage.SuccessFileDeleted});
                }

                if (string.IsNullOrWhiteSpace(filename))
                    return StatusCode(400);

                await _deleteFileByName.DeleteByName(filename, _userInfo.UserId);

                return StatusCode(200, new { message = SuccessMessage.SuccessFileDeleted });
            }
            catch (FileException ex)
            {
                return StatusCode(404, new { message = ex.Message });
            }
        }

        [HttpGet("{fileId}")]
        public async Task<IActionResult> GetOneFile([FromRoute] int fileId)
        {
            try
            {
                var file = await _readFile.ReadById(fileId, null);

                if (file.user_id != _userInfo.UserId)
                    return StatusCode(404);

                return StatusCode(200, new { file });
            }
            catch (FileException ex)
            {
                return StatusCode(404, new { message = ex.Message });
            }
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllFiles([FromQuery] bool byAscending)
        {
            var query = _dbContext.Files.Where(f => f.user_id == _userInfo.UserId).AsQueryable();

            switch(byAscending)
            {
                case true:
                    query = query.OrderByDescending(f => f.operation_date).AsQueryable();
                    break;
                case false:
                    query = query.OrderBy(f => f.operation_date).AsQueryable();
                    break;
            }

            var files = await query.ToListAsync();

            if (files is null || files.Count == 0)
                return StatusCode(404, new { message = ExceptionFileMessages.NoOneFileNotFound });

            return StatusCode(200, new { files });
        }
    }
}
