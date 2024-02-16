using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;
using System.Text.RegularExpressions;
using UAParser;
using webapi.DTO;
using webapi.Exceptions;
using webapi.Helpers;
using webapi.Interfaces;
using webapi.Interfaces.Redis;
using webapi.Interfaces.Services;
using webapi.Localization;
using webapi.Models;

namespace webapi.Controllers.Account.Edit
{
    [Route("api/account/edit/password")]
    [ApiController]
    [Authorize]
    [ValidateAntiForgeryToken]
    public class PasswordController : ControllerBase
    {
        private readonly IRepository<UserModel> _userRepository;
        private readonly IRepository<NotificationModel> _notificationRepository;
        private readonly IRedisCache _redisCache;
        private readonly IUserAgent _userAgent;
        private readonly ILogger<PasswordController> _logger;
        private readonly IPasswordManager _passwordManager;
        private readonly ITokenService _tokenService;
        private readonly IUserInfo _userInfo;

        public PasswordController(
            IRepository<UserModel> userRepository,
            IRepository<NotificationModel> notificationRepository,
            IRedisCache redisCache,
            IUserAgent userAgent,
            ILogger<PasswordController> logger,
            IPasswordManager passwordManager,
            ITokenService tokenService,
            IUserInfo userInfo)
        {
            _userRepository = userRepository;
            _notificationRepository = notificationRepository;
            _redisCache = redisCache;
            _userAgent = userAgent;
            _logger = logger;
            _passwordManager = passwordManager;
            _tokenService = tokenService;
            _userInfo = userInfo;
        }

        [HttpPut]
        public async Task<IActionResult> UpdatePassword([FromBody] PasswordDTO passwordDto)
        {
            try
            {
                if (!Regex.IsMatch(passwordDto.NewPassword, Validation.Password))
                    return StatusCode(422, new { message = Message.INVALID_FORMAT });

                var user = await _userRepository.GetByFilter(query => query.Where(u => u.email.Equals(_userInfo.Email)));
                if (user is null)
                    return StatusCode(404, new { message = Message.NOT_FOUND });

                bool IsCorrect = _passwordManager.CheckPassword(passwordDto.OldPassword, user.password);
                if (!IsCorrect)
                    return StatusCode(401, new { message = Message.INCORRECT });

                user.password = _passwordManager.HashingPassword(passwordDto.NewPassword);
                await _userRepository.Update(user);

                var clientInfo = Parser.GetDefault().Parse(HttpContext.Request.Headers["User-Agent"].ToString());
                var ua = _userAgent.GetBrowserData(clientInfo);

                await _notificationRepository.Add(new NotificationModel
                {
                    message_header = "Someone changed your password",
                    message = $"Someone changed your password at {DateTime.UtcNow} from {ua.Browser}   {ua.Version} on OS {ua.OS}.",
                    priority = Priority.Security.ToString(),
                    send_time = DateTime.UtcNow,
                    is_checked = false,
                    user_id = _userInfo.UserId
                });

                await _redisCache.DeteteCacheByKeyPattern($"{ImmutableData.NOTIFICATIONS_PREFIX}{_userInfo.UserId}");
                await _redisCache.DeteteCacheByKeyPattern($"{ImmutableData.USER_DATA_PREFIX}{_userInfo.UserId}");

                return StatusCode(200, new { message = Message.UPDATED });
            }
            catch (EntityNotCreatedException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
            catch (EntityNotUpdatedException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
            catch (OperationCanceledException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}
