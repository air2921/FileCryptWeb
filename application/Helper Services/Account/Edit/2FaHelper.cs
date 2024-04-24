using application.Abstractions.Inner;
using application.Helpers;
using application.Helpers.Localization;
using domain.Abstractions.Data;
using domain.Exceptions;
using domain.Models;
using Newtonsoft.Json;

namespace application.Helper_Services.Account.Edit
{
    public class _2FaHelper(
        IRepository<UserModel> userRepository,
        IRepository<NotificationModel> notificationRepository,
        IRedisCache redisCache) : ITransaction<UserModel>, IDataManagement, IValidator
    {
        public async Task CreateTransaction(UserModel user, object? parameter = null)
        {
            try
            {
                if (parameter is null || !bool.TryParse(parameter.ToString(), out bool enable))
                    throw new EntityException(Message.ERROR);

                if (user.is_2fa_enabled == enable)
                    throw new EntityException(Message.CONFLICT);

                user.is_2fa_enabled = enable;
                await userRepository.Update(user);

                await notificationRepository.Add(new NotificationModel
                {
                    message_header = NotificationMessage.AUTH_2FA_HEADER,
                    message = enable ? NotificationMessage.AUTH_2FA_ENABLE_BODY : NotificationMessage.AUTH_2FA_DISABLE_BODY,
                    priority = Priority.Security.ToString(),
                    send_time = DateTime.UtcNow,
                    is_checked = false,
                    user_id = user.id
                });

            }
            catch (EntityException)
            {
                throw;
            }
        }

        public async Task DeleteData(int id, object? parameter = null)
        {
            await redisCache.DeleteCache($"_2FaController_VerificationCode#{id}");
            await redisCache.DeteteCacheByKeyPattern($"{ImmutableData.USER_DATA_PREFIX}{id}");
            await redisCache.DeteteCacheByKeyPattern($"{ImmutableData.NOTIFICATIONS_PREFIX}{id}");
        }

        public async Task<object> GetData(string key)
        {
            var code = await redisCache.GetCachedData(key);
            if (code is not null)
                return JsonConvert.DeserializeObject<int>(code);
            else
                return 0;
        }

        public async Task SetData(string key, object data) => await redisCache.CacheData(key, data, TimeSpan.FromMinutes(10));

        public bool IsValid(object data, object? parameter = null) => data is int v && v != default && parameter is not null && parameter.Equals(data);
    }
}
