using domain.Abstractions.Data;
using domain.Exceptions;
using domain.Helpers;
using domain.Localization;
using domain.Models;
using domain.Services.Abstractions;
using services.Abstractions;

namespace domain.Services.Additional.Account.Edit
{
    public class PasswordHelper(
        IDatabaseTransaction transaction,
        IPasswordManager passwordManager,
        IRepository<UserModel> userRepository,
        IRepository<NotificationModel> notificationRepository,
        IRedisCache redisCache) : ITransaction<UserModel>, IDataManagement
    {
        public async Task CreateTransaction(UserModel user, object? parameter = null)
        {
            try
            {
                string? password = (string?)parameter;
                if (password is null)
                    throw new EntityException(Message.ERROR);

                user.password = passwordManager.HashingPassword(password);
                await userRepository.Update(user);

                await notificationRepository.Add(new NotificationModel
                {
                    message_header = NotificationMessage.AUTH_PASSWORD_CHANGED_HEADER,
                    message = NotificationMessage.AUTH_PASSWORD_CHANGED_BODY,
                    priority = Priority.Security.ToString(),
                    send_time = DateTime.UtcNow,
                    is_checked = false,
                    user_id = user.id
                });

                await transaction.CommitAsync();
            }
            catch (EntityException)
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
    }
}
