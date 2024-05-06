using application.Abstractions.TP_Services;
using application.Helpers;
using application.Helpers.Localization;
using domain.Abstractions.Data;
using domain.Exceptions;
using domain.Models;

namespace application.Helper_Services.Account.Edit
{
    public class PasswordHelper(
        IHashUtility hashUtility,
        IRepository<UserModel> userRepository,
        IRepository<NotificationModel> notificationRepository,
        IRedisCache redisCache,
        IDatabaseTransaction dbTransaction) : ITransaction<UserModel>, IDataManagement
    {
        public async Task CreateTransaction(UserModel user, object? parameter = null)
        {
            using var transaction = await dbTransaction.BeginAsync();
            try
            {
                if (parameter is not string password)
                    throw new EntityException(Message.ERROR);

                user.password = hashUtility.Hash(password);
                user.last_time_password_modified = DateTime.UtcNow;
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

                await dbTransaction.CommitAsync(transaction);
            }
            catch (EntityException)
            {
                await dbTransaction.RollbackAsync(transaction);
                throw;
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
