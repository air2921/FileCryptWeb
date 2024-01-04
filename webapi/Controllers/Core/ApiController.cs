using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using webapi.Exceptions;
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
        private readonly ICreate<ApiModel> _createAPI;
        private readonly IDelete<ApiModel> _deleteAPI;
        private readonly IRead<ApiModel> _readAPI;

        public ApiController(
            IUserInfo userInfo,
            ICreate<ApiModel> createAPI,
            IDelete<ApiModel> deleteAPI,
            IRead<ApiModel> readAPI)
        {
            _userInfo = userInfo;
            _createAPI = createAPI;
            _deleteAPI = deleteAPI;
            _readAPI = readAPI;
        }

        [HttpPost]
        public async Task<IActionResult> CreateNewAPI()
        {
            var apiModel = new ApiModel { user_id = _userInfo.UserId };

            await _createAPI.Create(apiModel);

            return StatusCode(201);
        }

        [HttpGet]
        public async Task<IActionResult> GetAPI()
        {
            try
            {
                var api = await _readAPI.ReadById(_userInfo.UserId, true);

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

                return StatusCode(200);
            }
            catch (ApiException ex)
            {
                return StatusCode(404, new { messaage = ex.Message });
            }
        }
    }
}
