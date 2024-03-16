using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using webapi.Attributes;
using webapi.Exceptions;
using webapi.Helpers;
using webapi.Interfaces;
using webapi.Interfaces.Redis;
using webapi.Interfaces.Services;
using webapi.Localization;
using webapi.Models;

namespace webapi.Controllers.Core
{
    [Route("api/core/api")]
    [ApiController]
    [Authorize]
    public class ApiController : ControllerBase
    {
        private const int CLASSIC_REQUEST_LIMIT = 300;
        private const int DEVELOPMENT_REQUEST_LIMIT = 150;
        private const int PRODUCTION_REQUEST_LIMIT = 1000;
        private const int CLASSIC_EXPIRY = 90;
        private const int PRODUCTION_EXPIRY = 30;

        #region fields and constructor

        private readonly IUserInfo _userInfo;
        private readonly IRedisCache _redisCache;
        private readonly IRepository<ApiModel> _apiRepository;

        public ApiController(
            IUserInfo userInfo,
            IRedisCache redisCache,
            IRepository<ApiModel> apiRepository)
        {
            _userInfo = userInfo;
            _redisCache = redisCache;
            _apiRepository = apiRepository;
        }

        #endregion

        [HttpPost("{type}")]
        [ValidateAntiForgeryToken]
        [ProducesResponseType(201)]
        [ProducesResponseType(typeof(object), 404)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> CreateNewAPI([FromRoute] string type)
        {
            try
            {
                var apiSettings = SetExpireAPI(type);

                await _apiRepository.Add(new ApiModel
                {
                    api_key = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString(),
                    type = type,
                    expiry_date = apiSettings.Expiry,
                    is_blocked = false,
                    last_time_activity = DateTime.UtcNow,
                    max_request_of_day = apiSettings.MaxRequest,
                    user_id = _userInfo.UserId
                });

                await _redisCache.DeteteCacheByKeyPattern($"{ImmutableData.API_PREFIX}{_userInfo.UserId}");

                return StatusCode(201);
            }
            catch (InvalidRouteException ex)
            {
                return StatusCode(404, new { message = ex.Message });
            }
            catch (EntityNotCreatedException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("{apiId}")]
        [ProducesResponseType(typeof(ApiModel), 200)]
        [ProducesResponseType(typeof(object), 404)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> GetAPI([FromRoute] int apiId)
        {
            try
            {
                var cacheKey = $"{ImmutableData.API_PREFIX}{_userInfo.UserId}_{apiId}";
                int apiCallLeft = 0;

                var cacheApi = await _redisCache.GetCachedData(cacheKey);
                if (cacheApi is not null)
                {
                    var cacheResult = JsonConvert.DeserializeObject<ApiModel>(cacheApi);
                    apiCallLeft = await ApiCallLeftCheck(cacheResult);

                    return StatusCode(200, new { api = cacheResult, apiCallLeft });
                }

                var api = await _apiRepository.GetByFilter(query => query.Where(a => a.api_id.Equals(apiId) && a.user_id.Equals(_userInfo.UserId)));
                if (api is null)
                    return StatusCode(404, new { message = Message.NOT_FOUND });

                apiCallLeft = await ApiCallLeftCheck(api);
                await _redisCache.CacheData(cacheKey, api, TimeSpan.FromMinutes(5));

                return StatusCode(200, new { api, apiCallLeft });
            }
            catch (OperationCanceledException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("all")]
        [ProducesResponseType(typeof(IEnumerable<ApiModel>), 200)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> GetAllApi()
        {
            try
            {
                var cacheKey = $"{ImmutableData.API_PREFIX}{_userInfo.UserId}";
                var apiObjects = new List<ApiObject>();

                var cacheApi = await _redisCache.GetCachedData(cacheKey);
                if (cacheApi is not null)
                {
                    var apiCacheModel = JsonConvert.DeserializeObject<IEnumerable<ApiModel>>(cacheApi);
                    apiObjects = await GetApiData(apiCacheModel);

                    return StatusCode(200, new { api = apiObjects });
                }

                var api = await _apiRepository.GetAll(query => query.Where(a => a.user_id.Equals(_userInfo.UserId)));
                apiObjects = await GetApiData(api);

                await _redisCache.CacheData(cacheKey, api, TimeSpan.FromMinutes(5));

                return StatusCode(200, new { api = apiObjects });
            }
            catch (OperationCanceledException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpDelete("revoke/{apiId}")]
        [ValidateAntiForgeryToken]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(object), 404)]
        public async Task<IActionResult> RevokeAPI([FromRoute] int apiId)
        {
            try
            {
                await _apiRepository.DeleteByFilter(query => query.Where(a => a.user_id.Equals(_userInfo.UserId) || a.api_id.Equals(apiId)));
                await _redisCache.DeteteCacheByKeyPattern($"{ImmutableData.API_PREFIX}{_userInfo.UserId}");

                return StatusCode(204);
            }
            catch (EntityNotDeletedException ex)
            {
                return StatusCode(404, new { message = ex.Message });
            }
        }

        [Helper]
        private ApiSettings SetExpireAPI(string type)
        {
            if (type.Equals(ApiType.Classic.ToString()))
                return new ApiSettings(DateTime.UtcNow.AddDays(CLASSIC_EXPIRY), CLASSIC_REQUEST_LIMIT);

            if (type.Equals(ApiType.Development.ToString()))
                return new ApiSettings(null, DEVELOPMENT_REQUEST_LIMIT);

            if (type.Equals(ApiType.Production.ToString()))
                return new ApiSettings(DateTime.UtcNow.AddDays(PRODUCTION_EXPIRY), PRODUCTION_REQUEST_LIMIT);

            throw new InvalidRouteException();
        }

        [Helper]
        private async Task<int> ApiCallLeftCheck(ApiModel apiModel)
        {
            var cacheApiCallLeft = await _redisCache.GetCachedData($"{DateTime.Today.ToString("yyyy-MM-dd")}_{apiModel.api_key}");
            if (cacheApiCallLeft is not null)
                return apiModel.max_request_of_day - JsonConvert.DeserializeObject<int>(cacheApiCallLeft);

            return apiModel.max_request_of_day;
        }

        [Helper]
        private async Task<List<ApiObject>> GetApiData(IEnumerable<ApiModel> apiModels)
        {
            var listApiObject = new List<ApiObject>();

            foreach (var apiModel in apiModels)
            {
                int requestCount = await ApiCallLeftCheck(apiModel);

                listApiObject.Add(new ApiObject
                {
                    api_id = apiModel.api_id,
                    api_key = apiModel.api_key,
                    type = apiModel.type,
                    expiry_date = apiModel.expiry_date,
                    is_blocked = apiModel.is_blocked,
                    last_time_activity = apiModel.last_time_activity,
                    max_request_of_day = apiModel.max_request_of_day,
                    apiCallLeft = requestCount,
                    user_id = apiModel.user_id
                });
            }

            return listApiObject;
        }

        [AuxiliaryObject]
        private class ApiObject
        {
            public int api_id { get; set; }
            public string api_key { get; set; }
            public string type { get; set; }
            public DateTime? expiry_date { get; set; }
            public bool is_blocked { get; set; }
            public DateTime last_time_activity { get; set; }
            public int max_request_of_day { get; set; }
            public int apiCallLeft { get; set; }
            public int user_id { get; set; }
        }

        [AuxiliaryObject]
        private record ApiSettings(DateTime? Expiry, int MaxRequest);
    }
}
