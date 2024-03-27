using System.Text.RegularExpressions;
using webapi.Exceptions;
using webapi.Helpers;
using webapi.Interfaces;
using webapi.Interfaces.Controllers.Services;
using webapi.Interfaces.Redis;
using webapi.Interfaces.Services;
using webapi.Localization;
using webapi.Models;

namespace webapi.Services.Account
{
    public sealed class PasswordService(
        IDatabaseTransaction transaction,
        IPasswordManager passwordManager,
        IRepository<UserModel> userRepository,
        IRepository<NotificationModel> notificationRepository,
        IRedisCache redisCache,
        IUserInfo userInfo) : ITransaction<UserModel>, IDataManagement, IValidator
    {
        public async Task CreateTransaction(UserModel user, object? parameter = null)
        {
            try
            {
                string? password = (string?)parameter;
                if (password is null)
                    throw new EntityNotUpdatedException(Message.ERROR);

                user.password = passwordManager.HashingPassword(password);
                await userRepository.Update(user);

                await notificationRepository.Add(new NotificationModel
                {
                    message_header = NotificationMessage.AUTH_PASSWORD_CHANGED_HEADER,
                    message = NotificationMessage.AUTH_PASSWORD_CHANGED_BODY,
                    priority = Priority.Security.ToString(),
                    send_time = DateTime.UtcNow,
                    is_checked = false,
                    user_id = userInfo.UserId
                });

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
            finally
            {
                await transaction.DisposeAsync();
            }
        }

        public async Task DeleteData(int id, object? parameter = null)
        {
            await redisCache.DeteteCacheByKeyPattern($"{ImmutableData.NOTIFICATIONS_PREFIX}{id}");
            await redisCache.DeteteCacheByKeyPattern($"{ImmutableData.USER_DATA_PREFIX}{id}");
        }

        public Task<object> GetData(string key) => throw new NotImplementedException();

        public Task SetData(string key, object data) => throw new NotImplementedException();

        public bool IsValid(object data, object parameter = null) => Regex.IsMatch((string)parameter, Validation.Password);
    }
}
