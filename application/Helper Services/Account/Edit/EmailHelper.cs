using application.Abstractions.Inner;
using application.Helpers;
using application.Helpers.Localization;
using domain.Abstractions.Data;
using domain.Exceptions;
using domain.Models;
using Newtonsoft.Json;

namespace application.Helper_Services.Account.Edit
{
    public class EmailHelper(
        IRepository<UserModel> userRepository,
        IRepository<NotificationModel> notificationRepository,
        IRedisCache redisCache,
        IDatabaseTransaction dbTransaction) : ITransaction<UserModel>, IDataManagement, IValidator
    {
        public async Task CreateTransaction(UserModel user, object? parameter = null)
        {
            using var transaction = await dbTransaction.BeginAsync();
            try
            {
                var email = parameter as string ?? throw new EntityException(Message.ERROR);

                user.email = email;
                await userRepository.Update(user);

                await notificationRepository.Add(new NotificationModel
                {
                    message_header = NotificationMessage.AUTH_EMAIL_CHANGED_HEADER,
                    message = NotificationMessage.AUTH_EMAIL_CHANGED_BODY,
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
            await redisCache.DeleteCache($"EmailController_Email#{id}");
            await redisCache.DeleteCache($"EmailController_ConfirmationCode_OldEmail#{id}");
            await redisCache.DeleteCache($"EmailController_ConfirmationCode_NewEmail#{id}");

            await redisCache.DeteteCacheByKeyPattern($"{ImmutableData.USER_DATA_PREFIX}{id}");
            await redisCache.DeteteCacheByKeyPattern($"{ImmutableData.NOTIFICATIONS_PREFIX}{id}");
        }

        public async Task<object> GetData(string key)
        {
            var data = await redisCache.GetCachedData(key);
            if (data is not null)
                return JsonConvert.DeserializeObject<object>(data);
            else
                return null;
        }

        public async Task SetData(string key, object data) => await redisCache.CacheData(key, data, TimeSpan.FromMinutes(10));

        public bool IsValid(object data, object? parameter = null) => data is int v && v != default && parameter is not null && parameter.Equals(data);
    }
}
