using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;
using webapi.Attributes;
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
    public class PasswordController : ControllerBase
    {
        #region fields and contructor

        private readonly IPasswordService _passwordService;
        private readonly IRepository<UserModel> _userRepository;
        private readonly ILogger<PasswordController> _logger;
        private readonly IPasswordManager _passwordManager;
        private readonly IUserInfo _userInfo;

        public PasswordController(
            IPasswordService passwordService,
            IRepository<UserModel> userRepository,
            ILogger<PasswordController> logger,
            IPasswordManager passwordManager,
            IUserInfo userInfo)
        {
            _passwordService = passwordService;
            _userRepository = userRepository;
            _logger = logger;
            _passwordManager = passwordManager;
            _userInfo = userInfo;
        }

        #endregion

        [HttpPut]
        [ValidateAntiForgeryToken]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(typeof(object), 422)]
        [ProducesResponseType(typeof(object), 404)]
        [ProducesResponseType(typeof(object), 401)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> UpdatePassword([FromBody] PasswordDTO passwordDto)
        {
            try
            {
                if (!_passwordService.ValidatePassword(passwordDto.NewPassword))
                    return StatusCode(422, new { message = Message.INVALID_FORMAT });

                var user = await _userRepository.GetById(_userInfo.UserId);
                if (user is null)
                    return StatusCode(404, new { message = Message.NOT_FOUND });

                bool IsCorrect = _passwordManager.CheckPassword(passwordDto.OldPassword, user.password);
                if (!IsCorrect)
                    return StatusCode(401, new { message = Message.INCORRECT });

                await _passwordService.UpdateTransaction(user, passwordDto.NewPassword);
                await _passwordService.ClearData(_userInfo.UserId);

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

    public interface IPasswordService
    {
        public Task UpdateTransaction(UserModel user, string password);
        public Task ClearData(int userId);
        public bool ValidatePassword(string password);
    }

    public class PasswordService : IPasswordService
    {
        private readonly IDatabaseTransaction _transaction;
        private readonly IPasswordManager _passwordManager;
        private readonly IRepository<UserModel> _userRepository;
        private readonly IRepository<NotificationModel> _notificationRepository;
        private readonly IRedisCache _redisCache;
        private readonly IUserInfo _userInfo;

        public PasswordService(
            IDatabaseTransaction transaction,
            IPasswordManager passwordManager,
            IRepository<UserModel> userRepository,
            IRepository<NotificationModel> notificationRepository,
            IRedisCache redisCache,
            IUserInfo userInfo)
        {
            _transaction = transaction;
            _passwordManager = passwordManager;
            _userRepository = userRepository;
            _notificationRepository = notificationRepository;
            _redisCache = redisCache;
            _userInfo = userInfo;
        }

        [Helper]
        public async Task UpdateTransaction(UserModel user, string password)
        {
            try
            {
                user.password = _passwordManager.HashingPassword(password);
                await _userRepository.Update(user);

                await _notificationRepository.Add(new NotificationModel
                {
                    message_header = NotificationMessage.AUTH_PASSWORD_CHANGED_HEADER,
                    message = NotificationMessage.AUTH_PASSWORD_CHANGED_BODY,
                    priority = Priority.Security.ToString(),
                    send_time = DateTime.UtcNow,
                    is_checked = false,
                    user_id = _userInfo.UserId
                });

                await _transaction.CommitAsync();
            }
            catch (EntityNotUpdatedException)
            {
                await _transaction.RollbackAsync();
                throw;
            }
            catch (EntityNotCreatedException)
            {
                await _transaction.RollbackAsync();
                throw;
            }
            finally
            {
                await _transaction.DisposeAsync();
            }
        }

        [Helper]
        public async Task ClearData(int userId)
        {
            await _redisCache.DeteteCacheByKeyPattern($"{ImmutableData.NOTIFICATIONS_PREFIX}{userId}");
            await _redisCache.DeteteCacheByKeyPattern($"{ImmutableData.USER_DATA_PREFIX}{userId}");
        }

        public bool ValidatePassword(string password)
        {
            return Regex.IsMatch(password, Validation.Password);
        }
    }
}
