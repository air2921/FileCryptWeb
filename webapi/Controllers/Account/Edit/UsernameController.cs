using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;
using webapi.Attributes;
using webapi.Exceptions;
using webapi.Helpers;
using webapi.Interfaces;
using webapi.Interfaces.Redis;
using webapi.Interfaces.Services;
using webapi.Localization;
using webapi.Models;

namespace webapi.Controllers.Account.Edit
{
    [Route("api/account/edit/username")]
    [ApiController]
    [Authorize]
    [ValidateAntiForgeryToken]
    public class UsernameController : ControllerBase
    {
        #region fields and constructor

        private readonly IRepository<UserModel> _userRepository;
        private readonly IRedisCache _redisCache;
        private readonly ILogger<UsernameController> _logger;
        private readonly IUserInfo _userInfo;
        private readonly ITokenService _tokenService;

        public UsernameController(
            IRepository<UserModel> userRepository,
            IRedisCache redisCache,
            ILogger<UsernameController> logger,
            IUserInfo userInfo,
            ITokenService tokenService)
        {
            _userRepository = userRepository;
            _redisCache = redisCache;
            _logger = logger;
            _userInfo = userInfo;
            _tokenService = tokenService;
        }

        #endregion

        [HttpPut]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(typeof(object), 400)]
        [ProducesResponseType(typeof(object), 404)]
        [ProducesResponseType(typeof(object), 500)]
        [ProducesResponseType(typeof(object), 206)]
        public async Task<IActionResult> UpdateUsername([FromQuery] string username)
        {
            try
            {
                if (!Regex.IsMatch(username, Validation.Username))
                    return StatusCode(400, new { message = Message.INVALID_FORMAT });

                var user = await _userRepository.GetById(_userInfo.UserId);
                if (user is null)
                    return StatusCode(404, new { message = Message.NOT_FOUND });

                await DbUpdate(user, username);
                await _tokenService.UpdateJwtToken();
                await ClearData();

                return StatusCode(200, new { message = Message.UPDATED });
            }
            catch (EntityNotUpdatedException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
            catch (OperationCanceledException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                _tokenService.DeleteTokens();
                return StatusCode(206, new { message = ex.Message });
            }
        }

        [Helper]
        private async Task DbUpdate(UserModel user, string username)
        {
            try
            {
                user.username = username;
                await _userRepository.Update(user);
            }
            catch (EntityNotUpdatedException)
            {
                throw;
            }
        }

        [Helper]
        private async Task ClearData()
        {
            _tokenService.DeleteUserDataSession();
            await _redisCache.DeteteCacheByKeyPattern($"{ImmutableData.USER_DATA_PREFIX}{_userInfo.UserId}");
        }
    }
}
