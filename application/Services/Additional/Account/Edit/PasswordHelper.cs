using application.Abstractions.Services.TP_Services;
using application.Helpers;
using application.Helpers.Localization;
using application.Services.Abstractions;
using domain.Abstractions.Data;
using domain.Exceptions;
using domain.Models;

namespace application.Services.Additional.Account.Edit
{
    public class PasswordHelper(
        IDatabaseTransaction transaction,
        IHashUtility hashUtility,
        IRepository<UserModel> userRepository,
        IRepository<NotificationModel> notificationRepository,
        IRedisCache redisCache) : ITransaction<UserModel>, IDataManagement
    {
        public async Task CreateTransaction(UserModel user, object? parameter = null)
        {
            try
            {
                if (parameter is not string password)
                    throw new EntityException(Message.ERROR);

                user.password = hashUtility.Hash(password);
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
