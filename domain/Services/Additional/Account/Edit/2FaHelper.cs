using domain.Abstractions;
using domain.Abstractions.Data;
using domain.Exceptions;
using domain.Helpers;
using domain.Localization;
using domain.Models;
using domain.Services.Abstractions;
using Newtonsoft.Json;

namespace domain.Services.Additional.Account.Edit
{
    public class _2FaHelper(
        IDatabaseTransaction transaction,
        IRepository<UserModel> userRepository,
        IRepository<NotificationModel> notificationRepository,
        IValidation validation,
        IRedisCache redisCache) : ITransaction<UserModel>, IDataManagement, IValidator
    {
        public async Task CreateTransaction(UserModel user, object? parameter = null)
        {
            try
            {
                if (parameter is not bool || parameter is null)
                    throw new EntityException(Message.ERROR);
                bool enable = (bool)parameter;

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

        public bool IsValid(object data, object? parameter = null) => data is int v && validation.IsSixDigit(v) && parameter.Equals(data);
    }
}
