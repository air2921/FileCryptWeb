using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
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
                var ApiExpire = SetExpireAPI(type);

                await _createAPI.Create(new ApiModel
                {
                    api_key = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString(),
                    type = type,
                    expiry_date = ApiExpire,
                    is_blocked = false,
                    last_time_activity = DateTime.UtcNow,
                    user_id = _userInfo.UserId
                });

                HttpContext.Session.SetString(Constants.CACHE_API, true.ToString());

                return StatusCode(201);
            }
            catch (InvalidRouteException ex)
            {
                return StatusCode(404, new { message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAPI()
        {
            var cacheKey = $"API_Settings_{_userInfo.UserId}";

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
                    return StatusCode(200, new { api = JsonConvert.DeserializeObject<ApiModel>(cacheApi) });

                var api = await _readAPI.ReadById(_userInfo.UserId, true);

                await _redisCache.CacheData(cacheKey, api, TimeSpan.FromMinutes(10));

                return StatusCode(200, new { api });
            }
            catch (ApiException ex)
            {
                return StatusCode(404, new { message = ex.Message });
            }
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
                return StatusCode(404, new { messaage = ex.Message });
            }
        }

        private DateTime? SetExpireAPI(string type)
        {
            if (type.Equals(ApiType.Classic.ToString()))
                return DateTime.UtcNow.AddDays(90);

            if (type.Equals(ApiType.Development.ToString()))
                return null;

            if (type.Equals(ApiType.Production.ToString()))
                return DateTime.UtcNow.AddDays(30);

            throw new InvalidRouteException();
        } 
    }
}
