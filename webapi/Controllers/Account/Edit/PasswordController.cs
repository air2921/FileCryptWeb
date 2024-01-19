using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using UAParser;
using webapi.DB;
using webapi.DTO;
using webapi.Exceptions;
using webapi.Interfaces.Services;
using webapi.Interfaces.SQL;
using webapi.Localization;
using webapi.Models;
using webapi.Services;

namespace webapi.Controllers.Account.Edit
{
    [Route("api/account/edit/password")]
    [ApiController]
    [Authorize]
    [ValidateAntiForgeryToken]
    public class PasswordController : ControllerBase
    {
        private readonly ICreate<NotificationModel> _createNotification;
        private readonly IUpdate<UserModel> _update;
        private readonly IUserAgent _userAgent;
        private readonly ILogger<PasswordController> _logger;
        private readonly IPasswordManager _passwordManager;
        private readonly ITokenService _tokenService;
        private readonly IUserInfo _userInfo;
        private readonly FileCryptDbContext _dbContext;

        public PasswordController(
            ICreate<NotificationModel> createNotification,
            IUpdate<UserModel> update,
            IUserAgent userAgent,
            ILogger<PasswordController> logger,
            IPasswordManager passwordManager,
            ITokenService tokenService,
            IUserInfo userInfo,
            FileCryptDbContext dbContext)
        {
            _createNotification = createNotification;
            _update = update;
            _userAgent = userAgent;
            _logger = logger;
            _passwordManager = passwordManager;
            _tokenService = tokenService;
            _userInfo = userInfo;
            _dbContext = dbContext;
        }

        [HttpPut]
        public async Task<IActionResult> UpdatePassword([FromBody] PasswordDTO passwordDto)
        {
            try
            {
                if (!Regex.IsMatch(passwordDto.NewPassword, Validation.Password))
                    return StatusCode(422, new { message = AccountErrorMessage.InvalidFormatPassword });

                var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.email == _userInfo.Email);
                if (user is null)
                {
                    _logger.LogWarning($"Non-existent user {_userInfo.Username}#{_userInfo.UserId} was requested to authorized endpoint.\nTrying delete tokens from cookie");
                    _tokenService.DeleteTokens();
                    _logger.LogWarning("Tokens was deleted");
                    return StatusCode(404, new { message = AccountErrorMessage.UserNotFound });
                }

                bool IsCorrect = _passwordManager.CheckPassword(passwordDto.OldPassword, user.password);
                if (!IsCorrect)
                    return StatusCode(401, new { message = AccountErrorMessage.PasswordIncorrect });


                _logger.LogInformation($"{_userInfo.Username}#{_userInfo.UserId} Password is correct, action is allowed");

                user.password = _passwordManager.HashingPassword(passwordDto.NewPassword);
                await _update.Update(user, null);
                _logger.LogInformation("Password was hashed and updated in db");

                var clientInfo = Parser.GetDefault().Parse(HttpContext.Request.Headers["User-Agent"].ToString());
                var ua = _userAgent.GetBrowserData(clientInfo);

                var notificationModel = new NotificationModel
                {
                    message_header = "Someone changed your password",
                    message = $"Someone changed your password at {DateTime.UtcNow} from {ua.Browser}   {ua.Version} on OS {ua.OS}.",
                    priority = Priority.Security.ToString(),
                    send_time = DateTime.UtcNow,
                    is_checked = false,
                    receiver_id = _userInfo.UserId
                };

                await _createNotification.Create(notificationModel);

                return StatusCode(200, new { message = AccountSuccessMessage.PasswordUpdated });
            }
            catch (UserException ex)
            {
                _logger.LogWarning($"Non-existent user {_userInfo.Username}#{_userInfo.UserId} was requested to authorized endpoint.\nTrying delete tokens from cookie");
                _tokenService.DeleteTokens();
                _logger.LogWarning("Tokens was deleted");

                return StatusCode(404, new { message = ex.Message });
            }
        }
    }
}
