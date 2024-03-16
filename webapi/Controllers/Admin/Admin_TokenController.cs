using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using webapi.Interfaces;
using webapi.Localization;
using webapi.Models;
using webapi.Exceptions;
using webapi.Attributes;
using webapi.Interfaces.Services;
using webapi.DB;

namespace webapi.Controllers.Admin
{
    [Route("api/admin/tokens")]
    [ApiController]
    [Authorize(Roles = "HighestAdmin,Admin")]
    public class Admin_TokenController : ControllerBase
    {
        #region fields and constructor

        private readonly IApiAdminTokenService _adminTokenService;
        private readonly IRepository<TokenModel> _tokenRepository;
        private readonly IRepository<UserModel> _userRepository;
        private readonly IUserInfo _userInfo;

        public Admin_TokenController(
            IApiAdminTokenService adminTokenService,
            IRepository<TokenModel> tokenRepository,
            IRepository<UserModel> userRepository,
            IUserInfo userInfo)
        {
            _adminTokenService = adminTokenService;
            _tokenRepository = tokenRepository;
            _userRepository = userRepository;
            _userInfo = userInfo;
        }

        #endregion

        [HttpDelete("revoke/all/{userId}")]
        [ValidateAntiForgeryToken]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(typeof(object), 404)]
        [ProducesResponseType(typeof(object), 403)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> RevokeAllUserTokens([FromRoute] int userId)
        {
            try
            {
                var target = await _userRepository.GetById(userId);
                if (target is null)
                    return StatusCode(404, new { message = Message.NOT_FOUND });

                if (!_adminTokenService.IsAllowed(target, _userInfo.Role))
                    return StatusCode(403, new { message = Message.FORBIDDEN });

                await _adminTokenService.DbTransaction(await _tokenRepository
                    .GetAll(query => query.Where(t => t.user_id.Equals(userId))),target.id);
                return StatusCode(200, new { message = Message.REMOVED });
            }
            catch (EntityNotCreatedException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
            catch (EntityNotDeletedException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
            catch (OperationCanceledException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpDelete("revoke/{tokenId}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RevokeToken(int tokenId)
        {
            try
            {
                var token = await _tokenRepository.GetById(tokenId);
                if (token is null)
                    return StatusCode(404, new { message = Message.NOT_FOUND });

                var target = await _userRepository.GetById(token.user_id);
                if (target is null)
                    return StatusCode(404, new { message = Message.NOT_FOUND });

                if (!_adminTokenService.IsAllowed(target, _userInfo.Role))
                    return StatusCode(403, new { message = Message.FORBIDDEN });

                await _tokenRepository.Delete(tokenId);
                return StatusCode(200, new { message = Message.REMOVED });
            }
            catch (OperationCanceledException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
            catch (EntityNotDeletedException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }

    public interface IApiAdminTokenService
    {
        public bool IsAllowed(UserModel user, string role);
        public Task DbTransaction(IEnumerable<TokenModel> tokenModels, int userId);
    }

    public class AdminTokenService : IApiAdminTokenService
    {
        private readonly FileCryptDbContext _dbContext;
        private readonly IRepository<TokenModel> _tokenRepository;
        private readonly IRepository<NotificationModel> _notificationRepository;

        public AdminTokenService(
            FileCryptDbContext dbContext,
            IRepository<TokenModel> tokenRepository,
            IRepository<NotificationModel> notificationRepository)
        {
            _dbContext = dbContext;
            _tokenRepository = tokenRepository;
            _notificationRepository = notificationRepository;
        }

        [Helper]
        public async Task DbTransaction(IEnumerable<TokenModel> tokenModels, int userId)
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                var identifiers = new List<int>();
                foreach (var tokenModel in tokenModels)
                    identifiers.Add(tokenModel.token_id);

                await _tokenRepository.DeleteMany(identifiers);

                await _notificationRepository.Add(new NotificationModel
                {
                    message_header = NotificationMessage.AUTH_TOKENS_REVOKED_HEADER,
                    message = NotificationMessage.AUTH_TOKENS_REVOKED_BODY,
                    is_checked = false,
                    priority = Priority.Security.ToString(),
                    send_time = DateTime.UtcNow,
                    user_id = userId
                });

                await transaction.CommitAsync();
            }
            catch (EntityNotDeletedException)
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

        public bool IsAllowed(UserModel user, string role)
        {
            return role.Equals("HighestAdmin") && !user.role.Equals("HighestAdmin");
        }
    }
}
