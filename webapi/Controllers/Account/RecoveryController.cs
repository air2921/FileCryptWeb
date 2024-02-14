using Microsoft.AspNetCore.Mvc;
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

namespace webapi.Controllers.Account
{
    [Route("api/auth/recovery")]
    [ApiController]
    [ValidateAntiForgeryToken]
    public class RecoveryController : ControllerBase
    {
        private readonly IRepository<UserModel> _userRepository;
        private readonly IRepository<NotificationModel> _notificationRepository;
        private readonly IRedisCache _redisCache;
        private readonly IRepository<LinkModel> _linkRepository;
        private readonly IRepository<TokenModel> _tokenRepository;
        private readonly ILogger<RecoveryController> _logger;
        private readonly IUserAgent _userAgent;
        private readonly IEmailSender _emailSender;
        private readonly IPasswordManager _passwordManager;
        private readonly IGenerateKey _generateKey;
        private readonly IFileManager _fileManager;

        public RecoveryController(
            IRepository<UserModel> userRepository,
            IRepository<NotificationModel> notificationRepository,
            IRedisCache redisCache,
            IRepository<LinkModel> linkRepository,
            IRepository<TokenModel> tokenRepository,
            ILogger<RecoveryController> logger,
            IUserAgent userAgent,
            IEmailSender emailSender,
            IPasswordManager passwordManager,
            IGenerateKey generateKey,
            IFileManager fileManager)
        {
            _userRepository = userRepository;
            _notificationRepository = notificationRepository;
            _redisCache = redisCache;
            _linkRepository = linkRepository;
            _tokenRepository = tokenRepository;
            _logger = logger;
            _userAgent = userAgent;
            _emailSender = emailSender;
            _passwordManager = passwordManager;
            _generateKey = generateKey;
            _fileManager = fileManager;
        }

        [HttpPost("unique/token")]
        public async Task<IActionResult> RecoveryAccount([FromQuery] string email)
        {
            try
            {
                var user = await _userRepository.GetByFilter(query => query.Where(u => u.email.Equals(email.ToLowerInvariant())));
                if (user is null)
                    return StatusCode(404, new { message = AccountErrorMessage.UserNotFound });

                string token = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString() + _generateKey.GenerateKey();

                var clientInfo = Parser.GetDefault().Parse(HttpContext.Request.Headers["User-Agent"].ToString());
                var ua = _userAgent.GetBrowserData(clientInfo);

                await _linkRepository.Add(new LinkModel
                {
                    user_id = user.id,
                    u_token = token,
                    expiry_date = DateTime.UtcNow.AddMinutes(30),
                    created_at = DateTime.UtcNow
                });

                await _emailSender.SendMessage(new EmailDto
                {
                    username = user.username,
                    email = user.email,
                    subject = EmailMessage.RecoveryAccountHeader,
                    message = EmailMessage.RecoveryAccountBody + $"{_fileManager.GetReactAppUrl(App.REACT_LAUNCH_JSON_PATH, true)}/auth/recovery?token={token}"
                });

                await _notificationRepository.Add(new NotificationModel
                {
                    message_header = "Someone trying recovery your account",
                    message = $"Someone trying recovery your account {user.username}#{user.id} at {DateTime.UtcNow} from {ua.Browser} {ua.Version} on OS {ua.OS}." +
                    $"Qnique token was sended on {user.email}",
                    priority = Priority.Security.ToString(),
                    send_time = DateTime.UtcNow,
                    is_checked = false,
                    receiver_id = user.id
                });

                _logger.LogInformation($"Created new token for {user.username}#{user.id} with life time for 30 minutes");
                await _redisCache.DeteteCacheByKeyPattern($"Notification_{user.id}");

                return StatusCode(201, new { message = AccountSuccessMessage.EmailSendedRecovery });
            }
            catch (EntityNotCreatedException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
            catch (SmtpClientException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
            catch (OperationCanceledException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost("account")]
        public async Task<IActionResult> RecoveryAccountByToken([FromQuery] string password, [FromQuery] string token)
        {
            try
            {
                if (!Regex.IsMatch(password, Validation.Password))
                    return StatusCode(400, new { message = AccountErrorMessage.InvalidFormatPassword });

                var link = await _linkRepository.GetByFilter(query => query.Where(l => l.u_token.Equals(token)));
                if (link is null)
                    return StatusCode(404, new { message = AccountErrorMessage.InvalidToken });

                if (link.expiry_date < DateTime.UtcNow)
                {
                    await _linkRepository.Delete(link.link_id);
                    _logger.LogInformation("Expired token was deleted");
                    return StatusCode(422, new { message = AccountErrorMessage.InvalidToken });
                }

                _logger.LogInformation($"Token: '{token}' is not expired");

                var user = await _userRepository.GetById(link.user_id);
                if (user is null)
                    return StatusCode(404, new { message = "Not found" });

                user.password = _passwordManager.HashingPassword(password);

                await _userRepository.Update(user);

                var clientInfo = Parser.GetDefault().Parse(HttpContext.Request.Headers["User-Agent"].ToString());
                var ua = _userAgent.GetBrowserData(clientInfo);

                await _notificationRepository.Add(new NotificationModel
                {
                    message_header = "Someone changed your password",
                    message = $"Someone changed your password at {DateTime.UtcNow} from {ua.Browser} {ua.Version} on OS {ua.OS}.",
                    priority = Priority.Security.ToString(),
                    send_time = DateTime.UtcNow,
                    is_checked = false,
                    receiver_id = link.user_id
                });

                await _linkRepository.DeleteByFilter(query => query.Where(l => l.u_token.Equals(token)));
                _logger.LogInformation($"Token: {token} was deleted");

                var tokenModel = await _tokenRepository.GetByFilter(query => query.Where(t => t.user_id.Equals(link.user_id)));
                tokenModel.refresh_token = Guid.NewGuid().ToString();
                tokenModel.expiry_date = DateTime.UtcNow.AddYears(-100);

                await _tokenRepository.Update(tokenModel);
                await _redisCache.DeteteCacheByKeyPattern($"Notification_{user.id}");
                await _redisCache.DeteteCacheByKeyPattern($"User_Data_{user.id}");

                return StatusCode(200);
            }
            catch (EntityNotDeletedException ex)
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
