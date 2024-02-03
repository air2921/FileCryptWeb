using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using webapi.DB.SQL;
using webapi.Exceptions;
using webapi.Interfaces.Redis;
using webapi.Interfaces.Services;
using webapi.Interfaces.SQL;
using webapi.Models;
using webapi.Services;

namespace webapi.Controllers.Core
{
    [Route("api/core/api")]
    [ApiController]
    [Authorize]
    public class ApiController : ControllerBase
    {
        private readonly IUserInfo _userInfo;
        private readonly IRedisCache _redisCache;
        private readonly ICreate<ApiModel> _createAPI;
        private readonly IDelete<ApiModel> _deleteAPI;
        private readonly IRead<ApiModel> _readAPI;

        public ApiController(
            IUserInfo userInfo,
            IRedisCache redisCache,
            ICreate<ApiModel> createAPI,
            IDelete<ApiModel> deleteAPI,
            IRead<ApiModel> readAPI)
        {
            _userInfo = userInfo;
            _redisCache = redisCache;
            _createAPI = createAPI;
            _deleteAPI = deleteAPI;
            _readAPI = readAPI;
        }

        [HttpPost("{type}")]
        public async Task<IActionResult> CreateNewAPI([FromRoute] string type)
        {
            try
            {
                var apiSettings = SetExpireAPI(type);

                await _createAPI.Create(new ApiModel
                {
                    api_key = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString(),
                    type = type,
                    expiry_date = apiSettings.Expiry,
                    is_blocked = false,
                    last_time_activity = DateTime.UtcNow,
                    max_request_of_day = apiSettings.MaxRequest,
                    user_id = _userInfo.UserId
                });

                HttpContext.Session.SetString(Constants.CACHE_API, true.ToString());

                return StatusCode(201);
            }
            catch (InvalidRouteException ex)
            {
                return StatusCode(404, new { message = ex.Message });
            }
            catch (ApiException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("{apiId}")]
        public async Task<IActionResult> GetAPI([FromRoute] int apiId)
        {
            var cacheKey = $"API_Key_Settings_{apiId}";
            int apiCallLeft = 0;

            try
            {
                var cacheApi = await _redisCache.GetCachedData(cacheKey);
                bool clearCache = bool.TryParse(HttpContext.Session.GetString(Constants.CACHE_API), out var parsedValue) ? parsedValue : true;
                if (clearCache)
                {
                    await _redisCache.DeleteCache(cacheKey);
                    HttpContext.Session.SetString(Constants.CACHE_API, false.ToString());
                }

                if (cacheApi is not null)
                {
                    var cacheResult = JsonConvert.DeserializeObject<ApiModel>(cacheApi);
                    apiCallLeft = await ApiCallLeftCheck(cacheResult);

                    return StatusCode(200, new { api = cacheResult, apiCallLeft });
                }

                var api = await _readAPI.ReadById(apiId, null);
                apiCallLeft = await ApiCallLeftCheck(api);

                await _redisCache.CacheData(cacheKey, api, TimeSpan.FromMinutes(5));

                return StatusCode(200, new { api, apiCallLeft });
            }
            catch (ApiException ex)
            {
                return StatusCode(404, new { message = ex.Message });
            }
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllApi()
        {
            var cacheKey = $"API_Keys_{_userInfo.UserId}";
            var cacheApi = await _redisCache.GetCachedData(cacheKey);
            bool clearCache = bool.TryParse(HttpContext.Session.GetString(Constants.CACHE_API), out var parsedValue) ? parsedValue : true;
            if (clearCache)
            {
                await _redisCache.DeleteCache(cacheKey);
                HttpContext.Session.SetString(Constants.CACHE_API, false.ToString());
            }

            if (cacheApi is not null)
                return StatusCode(200, new { api = JsonConvert.DeserializeObject<IEnumerable<ApiModel>>(cacheApi) });

            var api = await _readAPI.ReadAll(_userInfo.UserId, 0, 5);
            await _redisCache.CacheData(cacheKey, api, TimeSpan.FromMinutes(5));

            return StatusCode(200, new { api });
        }

        [HttpDelete("revoke")]
        public async Task<IActionResult> RevokeAPI()
        {
            try
            {
                await _deleteAPI.DeleteById(_userInfo.UserId, null);
                HttpContext.Session.SetString(Constants.CACHE_API, true.ToString());

                return StatusCode(200);
            }
            catch (ApiException ex)
            {
                return StatusCode(404, new { message = ex.Message });
            }
        }

        private ApiSettings SetExpireAPI(string type)
        {
            if (type.Equals(ApiType.Classic.ToString()))
                return new ApiSettings(DateTime.UtcNow.AddDays(90), 50);

            if (type.Equals(ApiType.Development.ToString()))
                return new ApiSettings(null, 25);

            if (type.Equals(ApiType.Production.ToString()))
                return new ApiSettings(DateTime.UtcNow.AddDays(30), 1000);

            throw new InvalidRouteException();
        }

        private async Task<int> ApiCallLeftCheck(ApiModel apiModel)
        {
            var cacheApiCallLeft = await _redisCache.GetCachedData($"{DateTime.Today.ToString("yyyy-MM-dd")}_{apiModel.api_key}");
            if (cacheApiCallLeft is not null)
            {
                return JsonConvert.DeserializeObject<int>(cacheApiCallLeft);
            }
            else
            {
                return apiModel.max_request_of_day;
            }
        }
    }

    public record ApiSettings(DateTime? Expiry, int MaxRequest);
}
