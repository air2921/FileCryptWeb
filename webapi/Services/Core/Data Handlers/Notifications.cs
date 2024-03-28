using Newtonsoft.Json;
using webapi.DB;
using webapi.Interfaces;
using webapi.Interfaces.Redis;
using webapi.Models;

namespace webapi.Services.Core.Data_Handlers
{
    public class Notifications(
        IRepository<NotificationModel> notificationRepository,
        ISorting sorting,
        IRedisCache redisCache,
        ILogger<Notifications> logger) : ICacheHandler<NotificationModel>
    {
        public async Task<NotificationModel> CacheAndGet(object dataObject)
        {
            try
            {
                var ntfObj = dataObject as NotificationObject ?? throw new FormatException();
                var notification = new NotificationModel();

                var cache = await redisCache.GetCachedData(ntfObj.CacheKey);
                if (cache is null)
                {
                    notification = await notificationRepository.GetByFilter
                        (query => query.Where(n => n.notification_id.Equals(ntfObj.NotificationId) && n.user_id.Equals(ntfObj.UserId)));

                    if (notification is null)
                        return null;

                    await redisCache.CacheData(ntfObj.CacheKey, notification, TimeSpan.FromMinutes(5));
                    return notification;
                }

                notification = JsonConvert.DeserializeObject<NotificationModel>(cache);
                if (notification is not null)
                    return notification;
                else
                    return null;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (JsonException ex)
            {
                logger.LogCritical(ex.ToString(), nameof(Files));
                throw new FormatException();
            }
        }

        public async Task<IEnumerable<NotificationModel>> CacheAndGetRange(object dataObject)
        {
            try
            {
                var ntfObj = dataObject as NotificationRangeObject ?? throw new FormatException();
                var notifications = new List<NotificationModel>();
                var cache = await redisCache.GetCachedData(ntfObj.CacheKey);
                if (cache is null)
                {
                    notifications = (List<NotificationModel>)await notificationRepository
                        .GetAll(sorting.SortNotifications(ntfObj.UserId, ntfObj.Skip, ntfObj.Count, ntfObj.ByDesc, ntfObj.Priority, ntfObj.IsChecked));

                    await redisCache.CacheData(ntfObj.CacheKey, notifications, TimeSpan.FromMinutes(10));
                    return notifications;
                }

                notifications = JsonConvert.DeserializeObject<List<NotificationModel>>(cache);
                if (notifications is not null)
                    return notifications;
                else
                    throw new FormatException();
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (JsonException ex)
            {
                logger.LogCritical(ex.ToString(), nameof(Files));
                throw new FormatException();
            }
        }
    }

    public record class NotificationObject(string CacheKey, int UserId, int NotificationId);
    public record class NotificationRangeObject(string CacheKey, int UserId, int Skip, int Count, bool ByDesc, string? Priority, bool? IsChecked);
}
