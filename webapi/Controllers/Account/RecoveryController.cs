using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using UAParser;
using webapi.DB;
using webapi.DTO;
using webapi.Exceptions;
using webapi.Helpers;
using webapi.Interfaces;
using webapi.Interfaces.Redis;
using webapi.Interfaces.Services;
using webapi.Localization;
using webapi.Models;
using webapi.Security;

namespace webapi.Controllers.Account
{
    [Route("api/auth/recovery")]
    [ApiController]
    [ValidateAntiForgeryToken]
    public class RecoveryController : ControllerBase
    {
        private readonly IRepository<UserModel> _userRepository;
        private readonly IRepository<NotificationModel> _notificationRepository;
        private readonly FileCryptDbContext _dbContext;
        private readonly IRedisCache _redisCache;
        private readonly IRepository<LinkModel> _linkRepository;
        private readonly IRepository<TokenModel> _tokenRepository;
        private readonly ILogger<RecoveryController> _logger;
        private readonly IUserAgent _userAgent;
        private readonly IEmailSender _emailSender;
        private readonly IPasswordManager _passwordManager;
        private readonly IGenerate _generate;
        private readonly IFileManager _fileManager;

        public RecoveryController(
            IRepository<UserModel> userRepository,
            IRepository<NotificationModel> notificationRepository,
            FileCryptDbContext dbContext,
            IRedisCache redisCache,
            IRepository<LinkModel> linkRepository,
            IRepository<TokenModel> tokenRepository,
            ILogger<RecoveryController> logger,
            IUserAgent userAgent,
            IEmailSender emailSender,
            IPasswordManager passwordManager,
            IGenerate generate,
            IFileManager fileManager)
        {
            _userRepository = userRepository;
            _notificationRepository = notificationRepository;
            _dbContext = dbContext;
            _redisCache = redisCache;
            _linkRepository = linkRepository;
            _tokenRepository = tokenRepository;
            _logger = logger;
            _userAgent = userAgent;
            _emailSender = emailSender;
            _passwordManager = passwordManager;
            _generate = generate;
            _fileManager = fileManager;
        }

        [HttpPost("unique/token")]
        public async Task<IActionResult> RecoveryAccount([FromQuery] string email)
        {
            try
            {
                var user = await _userRepository.GetByFilter(query => query.Where(u => u.email.Equals(email.ToLowerInvariant())));
                if (user is null)
                    return StatusCode(404, new { message = Message.NOT_FOUND });

                string token = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString() + _generate.GenerateKey();

                var ua = _userAgent.GetBrowserData(Parser.GetDefault().Parse(HttpContext.Request.Headers["User-Agent"].ToString()));

                await _emailSender.SendMessage(new EmailDto
                {
                    username = user.username,
                    email = user.email,
                    subject = EmailMessage.RecoveryAccountHeader,
                    message = EmailMessage.RecoveryAccountBody + $"{_fileManager.GetReactAppUrl(App.REACT_LAUNCH_JSON_PATH, true)}/auth/recovery?token={token}"
                });

                await _linkRepository.Add(new LinkModel
                {
                    user_id = user.id,
                    u_token = token,
                    expiry_date = DateTime.UtcNow.AddMinutes(30),
                    created_at = DateTime.UtcNow
                });

                await _notificationRepository.Add(new NotificationModel
                {
                    message_header = "Someone trying recovery your account",
                    message = $"Someone trying recovery your account {user.username}#{user.id} at {DateTime.UtcNow} from {ua.Browser} {ua.Version} on OS {ua.OS}." +
                    $"Qnique token was sended on {user.email}",
                    priority = Priority.Security.ToString(),
                    send_time = DateTime.UtcNow,
                    is_checked = false,
                    user_id = user.id
                });

                _logger.LogInformation($"Created new token for {user.username}#{user.id} with life time for 30 minutes");
                await _redisCache.DeteteCacheByKeyPattern($"{ImmutableData.NOTIFICATIONS_PREFIX}{user.id}");

                return StatusCode(201, new { message = Message.EMAIL_SENT });
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
                    return StatusCode(400, new { message = Message.INVALID_FORMAT });

                var link = await _linkRepository.GetByFilter(query => query.Where(l => l.u_token.Equals(token)));
                if (link is null)
                    return StatusCode(404, new { message = Message.NOT_FOUND });

                if (link.expiry_date < DateTime.UtcNow)
                {
                    await _linkRepository.Delete(link.link_id);
                    _logger.LogInformation("Expired token was deleted");
                    return StatusCode(422, new { message = Message.FORBIDDEN });
                }

                var user = await _userRepository.GetById(link.user_id);
                if (user is null)
                    return StatusCode(404, new { message = Message.NOT_FOUND });
                user.password = _passwordManager.HashingPassword(password);

                var ua = _userAgent.GetBrowserData(Parser.GetDefault().Parse(HttpContext.Request.Headers["User-Agent"].ToString()));

                await DbTransaction(user, token, ua);

                await _redisCache.DeteteCacheByKeyPattern($"{ImmutableData.NOTIFICATIONS_PREFIX}{user.id}");
                await _redisCache.DeteteCacheByKeyPattern($"{ImmutableData.USER_DATA_PREFIX}{user.id}");

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
            catch (EntityNotCreatedException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
            catch (OperationCanceledException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        private async Task DbTransaction(UserModel user, string token, BrowserData ua)
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                await _userRepository.Update(user);

                await _notificationRepository.Add(new NotificationModel
                {
                    message_header = "Someone changed your password",
                    message = $"Someone changed your password at {DateTime.UtcNow} from {ua.Browser} {ua.Version} on OS {ua.OS}.",
                    priority = Priority.Security.ToString(),
                    send_time = DateTime.UtcNow,
                    is_checked = false,
                    user_id = user.id
                });

                await _linkRepository.DeleteByFilter(query => query.Where(l => l.u_token.Equals(token)));

                var tokenModel = await _tokenRepository.GetByFilter(query => query.Where(t => t.user_id.Equals(user.id)));
                tokenModel.refresh_token = Guid.NewGuid().ToString();
                tokenModel.expiry_date = DateTime.UtcNow.AddYears(-100);

                await _tokenRepository.Update(tokenModel);

                await transaction.CommitAsync();
            }
            catch (EntityNotUpdatedException)
            {
                await transaction.RollbackAsync();
                throw;
            }
            catch (EntityNotCreatedException)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
