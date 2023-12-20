using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using webapi.DB;
using webapi.Exceptions;
using webapi.Interfaces.Redis;
using webapi.Interfaces.Services;
using webapi.Interfaces.SQL;
using webapi.Models;
using webapi.Services;

namespace webapi.Controllers.Admin.Manage_Files.Manage_MIME_s
{
    [Route("api/admin/mime")]
    [ApiController]
    [Authorize(Roles = "HighestAdmin,Admin")]
    public class DeleteMIMEController : ControllerBase
    {
        private readonly FileCryptDbContext _dbContext;
        private readonly IUserInfo _userInfo;
        private readonly ILogger<DeleteMIMEController> _logger;
        private readonly IRedisCache _redisCache;
        private readonly IDelete<FileMimeModel> _deleteMime;
        private readonly IDeleteByName<FileMimeModel> _deleteMimeByName;

        public DeleteMIMEController(
            FileCryptDbContext dbContext,
            IUserInfo userInfo,
            ILogger<DeleteMIMEController> logger,
            IRedisCache redisCache,
            IDelete<FileMimeModel> deleteMime,
            IDeleteByName<FileMimeModel> deleteMimeByName)
        {
            _dbContext = dbContext;
            _userInfo = userInfo;
            _logger = logger;
            _redisCache = redisCache;
            _deleteMime = deleteMime;
            _deleteMimeByName = deleteMimeByName;
        }

        [HttpDelete("one")]
        public async Task<IActionResult> DeleteOneMime([FromBody] FileMimeModel mimeModel, [FromQuery] bool byID)
        {
            try
            {
                if(byID)
                {
                    await _deleteMime.DeleteById(mimeModel.mime_id, null);
                    await _redisCache.DeleteCache(Constants.MIME_COLLECTION);
                    _logger.LogWarning($"{_userInfo.Username}#{_userInfo.UserId} deleted MIME type: #{mimeModel.mime_id} from db and cache");

                    return StatusCode(200);
                }

                await _deleteMimeByName.DeleteByName(mimeModel.mime_name, null);
                await _redisCache.DeleteCache(Constants.MIME_COLLECTION);

                return StatusCode(200);
            }
            catch (MimeException ex)
            {
                return StatusCode(404, new { message = ex.Message });
            }
        }

        [HttpDelete("all")]
        public async Task<IActionResult> DeleteAllMimes()
        {
            var mimes = await _dbContext.Mimes.ToListAsync();

            _dbContext.RemoveRange(mimes);
            await _dbContext.SaveChangesAsync();
            await _redisCache.DeleteCache(Constants.MIME_COLLECTION);
            _logger.LogWarning($"{_userInfo.Username}#{_userInfo.UserId} deleted all MIME collection from db and cache");

            return StatusCode(200);
        }
    }
}
