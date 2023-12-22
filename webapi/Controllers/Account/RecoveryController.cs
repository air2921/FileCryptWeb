using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using webapi.DB;
using webapi.DB.SQL;
using webapi.Exceptions;
using webapi.Interfaces.Services;
using webapi.Interfaces.SQL;
using webapi.Localization;
using webapi.Models;

namespace webapi.Controllers.Account
{
    [Route("api/recovery")]
    [ApiController]
    public class RecoveryController : ControllerBase
    {
        private readonly FileCryptDbContext _dbContext;
        private readonly ILogger<RecoveryController> _logger;
        private readonly IEmailSender<UserModel> _emailSender;
        private readonly ICreate<LinkModel> _createLink;
        private readonly IUpdate<UserModel> _updateUser;
        private readonly IUpdate<TokenModel> _updateToken;
        private readonly IPasswordManager _passwordManager;
        private readonly IDeleteByName<LinkModel> _deleteByName;
        private readonly IGenerateKey _generateKey;

        public RecoveryController(
            FileCryptDbContext dbContext,
            ILogger<RecoveryController> logger,
            IEmailSender<UserModel> emailSender,
            ICreate<LinkModel> createLink,
            IUpdate<UserModel> updateUser,
            IUpdate<TokenModel> updateToken,
            IPasswordManager passwordManager,
            IDeleteByName<LinkModel> deleteByName,
            IGenerateKey generateKey)
        {
            _dbContext = dbContext;
            _logger = logger;
            _emailSender = emailSender;
            _createLink = createLink;
            _updateUser = updateUser;
            _updateToken = updateToken;
            _passwordManager = passwordManager;
            _deleteByName = deleteByName;
            _generateKey = generateKey;
        }

        [HttpPost("create/unique/token")]
        public async Task<IActionResult> RecoveryAccount(string email)
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

                var userModel = new UserModel { email = user.email, username = user.username };
                await _emailSender.SendMessage(userModel, EmailMessage.RecoveryAccountHeader, EmailMessage.RecoveryAccountBody + token);
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
        public async Task<IActionResult> RecoveryAccountByToken([FromBody] string password, [FromQuery] string token)
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

                var userModel = new UserModel { id = link.user_id, password_hash = _passwordManager.HashingPassword(password) };
                await _updateUser.Update(userModel, null);
                _logger.LogInformation($"Password was updated for user with id: {link.user_id}");

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
        }
    }
}
