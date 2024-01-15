using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using webapi.DB.SQL;
using webapi.Exceptions;
using webapi.Interfaces.Redis;
using webapi.Interfaces.Services;
using webapi.Interfaces.SQL;
using webapi.Models;

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

        private const string API = "API_Settings";

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

        [HttpPost]
        public async Task<IActionResult> CreateNewAPI()
        {
            var apiModel = new ApiModel { user_id = _userInfo.UserId };

            await _createAPI.Create(apiModel);
            HttpContext.Session.SetString(API, true.ToString());

            return StatusCode(201);
        }

        [HttpGet]
        public async Task<IActionResult> GetAPI()
        {
            var cacheKey = $"API_Settings_{_userInfo.UserId}";

            try
            {
                var cacheApi = await _redisCache.GetCachedData(cacheKey);
                bool clearCache = HttpContext.Session.GetString(API) is not null ? bool.Parse(HttpContext.Session.GetString(API)) : true;
                if (clearCache)
                {
                    await _redisCache.DeleteCache(cacheKey);
                    HttpContext.Session.SetString(API, false.ToString());
                }

                if (cacheApi is not null)
                    return StatusCode(200, new { api = JsonConvert.DeserializeObject<ApiModel>(cacheApi) });

                var api = await _readAPI.ReadById(_userInfo.UserId, true);

                var settings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                };

                await _redisCache.CacheData(cacheKey, JsonConvert.SerializeObject(api, settings), TimeSpan.FromMinutes(10));

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
                HttpContext.Session.SetString(API, true.ToString());

                return StatusCode(200);
            }
            catch (ApiException ex)
            {
                return StatusCode(404, new { messaage = ex.Message });
            }
        }
    }
}
