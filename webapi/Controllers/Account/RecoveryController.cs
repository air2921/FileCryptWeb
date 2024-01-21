using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using UAParser;
using webapi.DB;
using webapi.DTO;
using webapi.Exceptions;
using webapi.Interfaces.Services;
using webapi.Interfaces.SQL;
using webapi.Localization;
using webapi.Models;
using webapi.Services;

namespace webapi.Controllers.Account
{
    [Route("api/auth/recovery")]
    [ApiController]
    [ValidateAntiForgeryToken]
    public class RecoveryController : ControllerBase
    {
        private readonly FileCryptDbContext _dbContext;
        private readonly ILogger<RecoveryController> _logger;
        private readonly IUserAgent _userAgent;
        private readonly IEmailSender _emailSender;
        private readonly IRead<UserModel> _readUser;
        private readonly ICreate<LinkModel> _createLink;
        private readonly ICreate<NotificationModel> _createNotification;
        private readonly IUpdate<UserModel> _updateUser;
        private readonly IUpdate<TokenModel> _updateToken;
        private readonly IPasswordManager _passwordManager;
        private readonly IDeleteByName<LinkModel> _deleteByName;
        private readonly IGenerateKey _generateKey;
        private readonly IFileManager _fileManager;

        public RecoveryController(
            FileCryptDbContext dbContext,
            ILogger<RecoveryController> logger,
            IUserAgent userAgent,
            IEmailSender emailSender,
            IRead<UserModel> readUser,
            ICreate<LinkModel> createLink,
            ICreate<NotificationModel> createNotification,
            IUpdate<UserModel> updateUser,
            IUpdate<TokenModel> updateToken,
            IPasswordManager passwordManager,
            IDeleteByName<LinkModel> deleteByName,
            IGenerateKey generateKey,
            IFileManager fileManager)
        {
            _dbContext = dbContext;
            _logger = logger;
            _userAgent = userAgent;
            _emailSender = emailSender;
            _readUser = readUser;
            _createLink = createLink;
            _createNotification = createNotification;
            _updateUser = updateUser;
            _updateToken = updateToken;
            _passwordManager = passwordManager;
            _deleteByName = deleteByName;
            _generateKey = generateKey;
            _fileManager = fileManager;
        }

        [HttpPost("create/unique/token")]
        public async Task<IActionResult> RecoveryAccount([FromQuery] string email)
        {
            try
            {
                var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.email == email.ToLowerInvariant());
                if (user is null)
                    return StatusCode(404, new { message = AccountErrorMessage.UserNotFound });

                string token = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString() + _generateKey.GenerateKey();

                var linkModel = new LinkModel
                {
                    user_id = user.id,
                    u_token = token,
                    expiry_date = DateTime.UtcNow.AddMinutes(30),
                    is_used = false,
                    created_at = DateTime.UtcNow
                };

                var clientInfo = Parser.GetDefault().Parse(HttpContext.Request.Headers["User-Agent"].ToString());
                var ua = _userAgent.GetBrowserData(clientInfo);

                var notificationModel = new NotificationModel
                {
                    message_header = "Someone trying recovery your account",
                    message = $"Someone trying recovery your account {user.username}#{user.id} at {DateTime.UtcNow} from {ua.Browser} {ua.Version} on OS {ua.OS}." +
                    $"Qnique token was sended on {user.email}",
                    priority = Priority.Security.ToString(),
                    send_time = DateTime.UtcNow,
                    is_checked = false,
                    receiver_id = user.id
                };

                var emailDto = new EmailDto
                {
                    username = user.username,
                    email = user.email,
                    subject = EmailMessage.RecoveryAccountHeader,
                    message = EmailMessage.RecoveryAccountBody + $"{_fileManager.GetReactAppUrl(App.REACT_LAUNCH_JSON_PATH, true)}/auth/recovery?token={token}"
                };

                await _emailSender.SendMessage(emailDto);
                await _createNotification.Create(notificationModel);
                await _createLink.Create(linkModel);
                _logger.LogInformation($"Created new token for {user.username}#{user.id} with life time for 30 minutes");

                return StatusCode(201, new { message = AccountSuccessMessage.EmailSendedRecovery });
            }
            catch (Exception)
            {
                return StatusCode(500);
            }
        }

        [HttpPost("account")]
        public async Task<IActionResult> RecoveryAccountByToken([FromQuery] string password, [FromQuery] string token)
        {
            try
            {
                var link = await _dbContext.Links.FirstOrDefaultAsync(l => l.u_token == token);
                if (link is null)
                    return StatusCode(404, new { message = AccountErrorMessage.InvalidToken });

                if (link.expiry_date < DateTime.UtcNow)
                {
                    _logger.LogWarning($"Token: {token} is expired, it will be delete from db");
                    await _deleteByName.DeleteByName(token, null);
                    _logger.LogInformation("Expired token was deleted");
                    return StatusCode(422, new { message = AccountErrorMessage.InvalidToken });
                }

                _logger.LogInformation($"Token: '{token}' is not expired");

                var user = await _readUser.ReadById(link.user_id, null);
                user.password = _passwordManager.HashingPassword(password);

                await _updateUser.Update(user, null);
                _logger.LogInformation($"Password was updated for user with id: {link.user_id}");

                var clientInfo = Parser.GetDefault().Parse(HttpContext.Request.Headers["User-Agent"].ToString());
                var ua = _userAgent.GetBrowserData(clientInfo);

                var notificationModel = new NotificationModel
                {
                    message_header = "Someone changed your password",
                    message = $"Someone changed your password at {DateTime.UtcNow} from {ua.Browser} {ua.Version} on OS {ua.OS}.",
                    priority = Priority.Security.ToString(),
                    send_time = DateTime.UtcNow,
                    is_checked = false,
                    receiver_id = link.user_id
                };

                await _createNotification.Create(notificationModel);

                await _deleteByName.DeleteByName(token, null);
                _logger.LogInformation($"Token: {token} was deleted");

                await _updateToken.Update(new TokenModel
                {
                    user_id = link.user_id,
                    refresh_token = null,
                    expiry_date = DateTime.UtcNow.AddYears(-100)
                }, true);

                return StatusCode(200, new { message = AccountSuccessMessage.PasswordUpdated });
            }
            catch (LinkException ex)
            {
                return StatusCode(404, new { message = ex.Message });
            }
            catch (UserException ex)
            {
                return StatusCode(404, new { message = ex.Message });
            }
        }
    }
}
