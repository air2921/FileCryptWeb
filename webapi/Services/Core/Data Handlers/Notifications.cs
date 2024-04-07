using Newtonsoft.Json;
using webapi.DB.Abstractions;
using webapi.DB.Ef;
using webapi.DB.Ef.Specifications.By_Relation_Specifications;
using webapi.DB.Ef.Specifications.Sorting_Specifications;
using webapi.Localization;
using webapi.Models;
using webapi.Services.Abstractions;

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
                var ntfObj = dataObject as NotificationObject ?? throw new FormatException(Message.ERROR);
                var notification = new NotificationModel();

                var cache = await redisCache.GetCachedData(ntfObj.CacheKey);
                if (cache is null)
                {
                    notification = await notificationRepository.GetByFilter
                        (new NotificationByIdAndByRelationSpec(ntfObj.NotificationId, ntfObj.UserId));

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
                throw new FormatException(Message.ERROR);
            }
        }

        public async Task<IEnumerable<NotificationModel>> CacheAndGetRange(object dataObject)
        {
            try
            {
                var ntfObj = dataObject as NotificationRangeObject ?? throw new FormatException(Message.ERROR);
                var notifications = new List<NotificationModel>();
                var cache = await redisCache.GetCachedData(ntfObj.CacheKey);
                if (cache is null)
                {
                    notifications = (List<NotificationModel>)await notificationRepository
                        .GetAll(new NotificationsSortSpec(ntfObj.UserId, ntfObj.Skip, ntfObj.Count, ntfObj.ByDesc, ntfObj.Priority, ntfObj.IsChecked));

                    await redisCache.CacheData(ntfObj.CacheKey, notifications, TimeSpan.FromMinutes(10));
                    return notifications;
                }

                notifications = JsonConvert.DeserializeObject<List<NotificationModel>>(cache);
                if (notifications is not null)
                    return notifications;
                else
                    throw new FormatException(Message.ERROR);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (JsonException ex)
            {
                logger.LogCritical(ex.ToString(), nameof(Notifications));
                throw new FormatException(Message.ERROR);
            }
        }
    }

    public record class NotificationObject(string CacheKey, int UserId, int NotificationId);
    public record class NotificationRangeObject(string CacheKey, int UserId, int Skip, int Count, bool ByDesc, string? Priority, bool? IsChecked);
}
