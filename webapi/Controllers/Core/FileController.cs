using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using webapi.DB;
using webapi.Exceptions;
using webapi.Interfaces.Services;
using webapi.Interfaces.SQL;
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

        [HttpDelete("one/{byID}")]
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

        [HttpGet("one/{id}")]
        public async Task<IActionResult> GetOneFile([FromRoute] int id)
        {
            try
            {
                var file = await _readFile.ReadById(id, null);

                return StatusCode(200, new { file });
            }
            catch (FileException ex)
            {
                return StatusCode(404, new { message = ex.Message });
            }
        }

        [HttpGet("all/{byAscending}")]
        public async Task<IActionResult> GetAllFiles([FromRoute] bool byAscending)
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
                return StatusCode(404);

            return StatusCode(200, new { files });
        }
    }
}
