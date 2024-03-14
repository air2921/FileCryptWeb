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
    public class UsernameController : ControllerBase
    {
        #region fields and constructor

        private readonly IRepository<UserModel> _userRepository;
        private readonly IApiUsernameService _usernameService;
        private readonly ILogger<UsernameController> _logger;
        private readonly IUserInfo _userInfo;
        private readonly ITokenService _tokenService;

        public UsernameController(
            IRepository<UserModel> userRepository,
            IApiUsernameService usernameService,
            ILogger<UsernameController> logger,
            IUserInfo userInfo,
            ITokenService tokenService)
        {
            _userRepository = userRepository;
            _usernameService = usernameService;
            _logger = logger;
            _userInfo = userInfo;
            _tokenService = tokenService;
        }

        #endregion

        [HttpPut]
        [XSRFProtection]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(typeof(object), 400)]
        [ProducesResponseType(typeof(object), 404)]
        [ProducesResponseType(typeof(object), 500)]
        [ProducesResponseType(typeof(object), 206)]
        public async Task<IActionResult> UpdateUsername([FromQuery] string username)
        {
            try
            {
                if (!_usernameService.ValidateUsername(username))
                    return StatusCode(400, new { message = Message.INVALID_FORMAT });

                var user = await _userRepository.GetById(_userInfo.UserId);
                if (user is null)
                    return StatusCode(404, new { message = Message.NOT_FOUND });

                await _usernameService.DbUpdate(user, username);
                await _tokenService.UpdateJwtToken();
                await _usernameService.ClearData(_userInfo.UserId);

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
    }

    public interface IApiUsernameService
    {
        public Task DbUpdate(UserModel user, string username);
        public Task ClearData(int userId);
        public bool ValidateUsername(string username);
    }

    public class UsernameService : IApiUsernameService
    {
        private readonly IRepository<UserModel> _userRepository;
        private readonly ITokenService _tokenService;
        private readonly IRedisCache _redisCache;

        public UsernameService(IRepository<UserModel> userRepository, ITokenService tokenService, IRedisCache redisCache)
        {
            _userRepository = userRepository;
            _tokenService = tokenService;
            _redisCache = redisCache;
        }

        [Helper]
        public async Task DbUpdate(UserModel user, string username)
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
        public bool ValidateUsername(string username)
        {
            return Regex.IsMatch(username, Validation.Username);
        }

        [Helper]
        public async Task ClearData(int userId)
        {
            _tokenService.DeleteUserDataSession();
            await _redisCache.DeteteCacheByKeyPattern($"{ImmutableData.USER_DATA_PREFIX}{userId}");
        }
    }
}
