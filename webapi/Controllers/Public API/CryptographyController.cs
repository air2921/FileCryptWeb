using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using webapi.Exceptions;
using webapi.Helpers;
using webapi.Interfaces;
using webapi.Interfaces.Controllers;
using webapi.Interfaces.Cryptography;
using webapi.Interfaces.Redis;
using webapi.Models;

namespace webapi.Controllers.Public_API
{
    [ApiController]
    [EnableCors("AllowOriginAPI")]
    [Route("api/public/cryptography/{type}")]
    public class CryptographyController : ControllerBase
    {
        private readonly IRepository<ApiModel> _apiRepository;
        private readonly ICryptographyControllerBase _cryptographyController;
        private readonly IRedisCache _redisCache;
        private readonly ICypher _cypher;

        public CryptographyController(
            IRepository<ApiModel> apiRepository,
            ICryptographyControllerBase cryptographyController,
            IRedisCache redisCache,
            ICypher cypher)
        {
            _apiRepository = apiRepository;
            _cryptographyController = cryptographyController;
            _redisCache = redisCache;
            _cypher = cypher;
        }

        [HttpPost("{operation}")]
        [RequestSizeLimit(75 * 1024 * 1024)]
        public async Task<IActionResult> EncryptFiles(
            [FromHeader(Name = ImmutableData.ENCRYPTION_KEY_HEADER_NAME)] string encryptionKey,
            [FromHeader(Name = ImmutableData.API_HEADER_NAME)] string apiKey,
            IFormFile file, [FromRoute] string type, [FromRoute] string operation)
        {
            try
            {
                var apiData = await IsValidAPI(apiKey);
                await ControlRequestCount(apiKey, apiData.MaxRequest);

                return await _cryptographyController.EncryptFile(_cypher.CypherFileAsync, encryptionKey, file, apiData.UserId, type, operation);
            }
            catch (InvalidRouteException ex)
            {
                return StatusCode(404, new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return StatusCode(422, new { message = ex.Message });
            }
            catch (InvalidOperationException)
            {
                return StatusCode(500, new { message = "Unexpected error" });
            }
        }

        private async Task<ApiData> IsValidAPI(string apiKey)
        {
            try
            {
                var api = await _apiRepository.GetByFilter(query => query.Where(a => a.api_key.Equals(apiKey)));
                if (api is null)
                    throw new ArgumentException("API Key not found");

                if (api.is_blocked)
                    throw new ArgumentException("API Key has been revoked and is no longer available");

                if (api.type == ApiType.Classic.ToString() || api.type == ApiType.Production.ToString())
                {
                    if (api.expiry_date < DateTime.UtcNow)
                    {
                        await _apiRepository.Delete(api.api_id);
                        throw new ArgumentException("API Key expired");
                    }
                }

                api.last_time_activity = DateTime.UtcNow;
                await _apiRepository.Update(api);

                return new ApiData(api.user_id, api.max_request_of_day);
            }
            catch (EntityNotDeletedException)
            {
                throw new InvalidOperationException();
            }
            catch (EntityNotUpdatedException)
            {
                throw new InvalidOperationException();
            }
            catch (OperationCanceledException ex)
            {
                throw new InvalidOperationException();
            }
        }

        private async Task ControlRequestCount(string apiKey, int maxRequest)
        {
            var cacheKey = $"{DateTime.Today.ToString("yyyy-MM-dd")}_{apiKey}";

            var currentTime = DateTime.Now;
            var endOfDay = new DateTime(currentTime.Year, currentTime.Month, currentTime.Day, 23, 59, 59);
            var timeUntilEndOfDay = endOfDay - currentTime;

            var cacheResult = await _redisCache.GetCachedData(cacheKey);
            if (cacheResult is not null)
            {
                var requestCount = JsonConvert.DeserializeObject<int>(cacheResult);

                if (requestCount > maxRequest)
                    throw new ArgumentException("Max count request of day is exceed");

                await _redisCache.CacheData(cacheKey, requestCount + 1, timeUntilEndOfDay);
            }
            else
            {
                await _redisCache.CacheData(cacheKey, 1, timeUntilEndOfDay);
            }
        }
    }
    public record ApiData(int UserId, int MaxRequest);
}
