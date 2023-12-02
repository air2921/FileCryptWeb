using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using webapi.DB;
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
        private readonly IEmailSender<UserModel> _emailSender;
        private readonly ICreate<LinkModel> _createLink;
        private readonly IUpdate<UserModel> _updateUser;
        private readonly IPasswordManager _passwordManager;
        private readonly IDeleteByName<LinkModel> _deleteByName;
        private readonly IGenerateKey _generateKey;

        public RecoveryController(
            FileCryptDbContext dbContext,
            IEmailSender<UserModel> emailSender,
            ICreate<LinkModel> createLink,
            IUpdate<UserModel> updateUser,
            IPasswordManager passwordManager,
            IDeleteByName<LinkModel> deleteByName,
            IGenerateKey generateKey)
        {
            _dbContext = dbContext;
            _emailSender = emailSender;
            _createLink = createLink;
            _updateUser = updateUser;
            _passwordManager = passwordManager;
            _deleteByName = deleteByName;
            _generateKey = generateKey;
        }

        [HttpPost("create/unique/token")]
        public async Task<IActionResult> RecoveryAccount(string email)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.email == email.ToLowerInvariant());
            if (user is null)
                return StatusCode(404, new { message = AccountErrorMessage.UserNotFound });

            string token = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString() + _generateKey.GenerateKey();

            var linkModel = new LinkModel
            {
                user_id = user.id,
                u_token = token,
                expiry_date = DateTime.UtcNow.AddHours(12),
                is_used = false,
                created_at = DateTime.UtcNow
            };

            var userModel = new UserModel { email = user.email, username = user.username };
            await _emailSender.SendMessage(userModel, EmailMessage.RecoveryAccountHeader, EmailMessage.RecoveryAccountBody + token);
            await _createLink.Create(linkModel);

            return StatusCode(201, new { message = AccountSuccessMessage.EmailSendedRecovery });
        }

        [HttpPost("{token}")]
        public async Task<IActionResult> RecoveryAccountByToken([FromBody] string password, [FromRoute] string token)
        {
            try
            {
                var link = await _dbContext.Links.FirstOrDefaultAsync(l => l.u_token == token);
                if (link is null || link.expiry_date < DateTime.UtcNow)
                    return StatusCode(400, new { message = AccountErrorMessage.InvalidToken });

                var userModel = new UserModel { id = link.user_id, password_hash = _passwordManager.HashingPassword(password) };
                await _updateUser.Update(userModel, null);

                await _deleteByName.DeleteByName(token);

                return StatusCode(200, new { message = AccountSuccessMessage.PasswordUpdated });
            }
            catch (LinkException ex)
            {
                return StatusCode(404, new { message = ex.Message });
            }
        }
    }
}
