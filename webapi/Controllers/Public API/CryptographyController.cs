using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using webapi.DB;
using webapi.Exceptions;
using webapi.Interfaces.Controllers;
using webapi.Interfaces.Cryptography;
using webapi.Interfaces.Redis;
using webapi.Models;
using webapi.Services;

namespace webapi.Controllers.Public_API
{
    [ApiController]
    [EnableCors("AllowOriginAPI")]
    [Route("api/public/cryptography/{type}/encryption")]
    public class CryptographyController : ControllerBase
    {
        private readonly ICryptographyControllerBase _cryptographyController;
        private readonly IRedisCache _redisCache;
        private readonly FileCryptDbContext _dbContext;
        private readonly IEncrypt _encrypt;
        private readonly IDecrypt _decrypt;

        public CryptographyController(
            ICryptographyControllerBase cryptographyController,
            IRedisCache redisCache,
            FileCryptDbContext dbContext,
            IEncrypt encrypt,
            IDecrypt decrypt)
        {
            _cryptographyController = cryptographyController;
            _redisCache = redisCache;
            _dbContext = dbContext;
            _encrypt = encrypt;
            _decrypt = decrypt;
        }

        [HttpPost("encrypt")]
        [RequestSizeLimit(75 * 1024 * 1024)]
        public async Task<IActionResult> EncryptFiles(
            [FromHeader(Name = Constants.ENCRYPTION_KEY_HEADER_NAME)] string encryptionKey,
            [FromHeader(Name = Constants.API_HEADER_NAME)] string apiKey,
            IFormFile file, [FromRoute] string type)
        {
            try
            {
                await ControlRequestCount(apiKey);
                var userId = await IsValidAPI(apiKey);

                return await _cryptographyController.EncryptFile(_encrypt.EncryptFileAsync, encryptionKey, file, userId, type);
            }
            catch (InvalidRouteException ex)
            {
                return StatusCode(404, new { message = ex.Message });
            }
            catch (ApiException ex)
            {
                return StatusCode(422, new { message = ex.Message });
            }
        }

        [HttpPost("decrypt")]
        [RequestSizeLimit(75 * 1024 * 1024)]
        public async Task<IActionResult> DecryptFiles(
            [FromHeader(Name = Constants.ENCRYPTION_KEY_HEADER_NAME)] string encryptionKey,
            [FromHeader(Name = Constants.API_HEADER_NAME)] string apiKey,
            IFormFile file, [FromRoute] string type)
        {
            try
            {
                await ControlRequestCount(apiKey);
                var userId = await IsValidAPI(apiKey);

                return await _cryptographyController.EncryptFile(_decrypt.DecryptFileAsync, encryptionKey, file, userId, type);
            }
            catch (InvalidRouteException ex)
            {
                return StatusCode(404, new { message = ex.Message });
            }
            catch (ApiException ex)
            {
                return StatusCode(422, new { message = ex.Message });
            }
        }

        private async Task<int> IsValidAPI(string apiKey)
        {
            var api = await _dbContext.API.FirstOrDefaultAsync(a => a.api_key == apiKey);
            if (api is null)
                throw new ApiException("API Key not found");

            if (api.is_blocked)
                throw new ApiException("API Key has been revoked and is no longer available");

            if (api.type == ApiType.Classic.ToString() || api.type == ApiType.Production.ToString())
            {
                if (api.expiry_date < DateTime.UtcNow)
                    throw new ApiException("API Key expired");
            }

            api.last_time_activity = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();

            return api.user_id;
        }

        private async Task ControlRequestCount(string apiKey)
        {
            var cacheKey = $"{DateTime.Today.ToString("yyyy-MM-dd")}_{apiKey}";

            var now = DateTime.UtcNow.Date;
            var endOfDay = now.AddDays(1).AddTicks(-1);
            var timeUntilEndOfDay = endOfDay - now;

            var cacheResult = await _redisCache.GetCachedData(cacheKey);
            if (cacheResult is not null)
            {
                var requestCount = JsonConvert.DeserializeObject<int>(cacheResult);

                if (requestCount > 25)
                    throw new ApiException("Max count request of day is exceeded");

                await _redisCache.CacheData(cacheKey, requestCount + 1, timeUntilEndOfDay);
            }
            else
            {
                await _redisCache.CacheData(cacheKey, 1, timeUntilEndOfDay);
            }
        }
    }
}
