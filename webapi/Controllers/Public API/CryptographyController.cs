using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using webapi.Attributes;
using webapi.Exceptions;
using webapi.Helpers;
using webapi.Interfaces;
using webapi.Interfaces.Controllers.Services;
using webapi.Interfaces.Redis;
using webapi.Localization;
using webapi.Models;
using webapi.Services.Core;

namespace webapi.Controllers.Public_API
{
    [ApiController]
    [EnableCors("AllowOriginAPI")]
    [Route("api/public/cryptography/{type}")]
    public class CryptographyController : ControllerBase
    {
        #region fields and constructor

        private readonly IRepository<ApiModel> _apiRepository;
        private readonly ICryptographyProvider _provider;
        private readonly IRedisCache _redisCache;

        public CryptographyController(
            IRepository<ApiModel> apiRepository,
            ICryptographyProvider provider,
            IRedisCache redisCache)
        {
            _apiRepository = apiRepository;
            _provider = provider;
            _redisCache = redisCache;
        }

        #endregion

        [HttpPost("{operation}")]
        [RequestSizeLimit(75 * 1024 * 1024)]
        [ProducesResponseType(typeof(FileStreamResult), 200)]
        [ProducesResponseType(typeof(object), 400)]
        [ProducesResponseType(typeof(object), 415)]
        [ProducesResponseType(typeof(object), 422)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> EncryptFiles(
            [FromHeader(Name = ImmutableData.ENCRYPTION_KEY_HEADER_NAME)] string encryptionKey,
            [FromHeader(Name = ImmutableData.API_HEADER_NAME)] string apiKey,
            IFormFile file, [FromRoute] string type, [FromRoute] string operation)
        {
            try
            {
                var apiData = await IsValidAPI(apiKey);
                await ControlRequestCount(apiKey, apiData.MaxRequest);

                return await _provider.EncryptFile(new CryptographyOperationOptions
                {
                    Key = encryptionKey,
                    File = file,
                    UserID = apiData.UserId,
                    Type = type,
                    Operation = operation
                });
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
                return StatusCode(500, new { message = Message.ERROR });
            }
        }

        [Helper]
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
                throw new InvalidOperationException(ex.Message);
            }
        }

        [Helper]
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

        [AuxiliaryObject]
        public record ApiData(int UserId, int MaxRequest);
    }
}
