using application.Helpers;
using application.Helpers.Localization;
using application.Cache_Handlers;
using domain.Abstractions.Data;
using domain.Exceptions;
using domain.Models;
using domain.Specifications.By_Relation_Specifications;
using application.Abstractions.Endpoints.Core;

namespace application.Master_Services.Core
{
    public class NotificationsService(
        IRepository<NotificationModel> repository,
        ICacheHandler<NotificationModel> cacheHandler,
        IRedisCache redisCache) : INotificationService
    {
        public async Task<Response> GetOne(int userId, int notificationId)
        {
            try
            {
                var cacheKey = $"{ImmutableData.NOTIFICATIONS_PREFIX}{userId}_{notificationId}";
                var notification = await cacheHandler.CacheAndGet(new NotificationObject(cacheKey, userId, notificationId));
                if (notification is null)
                    return new Response { Status = 404, Message = Message.NOT_FOUND };
                else
                    return new Response { Status = 200, ObjectData = notification };
            }
            catch (EntityException ex)
            {
                return new Response { Status = 500, Message = ex.Message };
            }
            catch (FormatException ex)
            {
                return new Response { Status = 500, Message = ex.Message };
            }
        }

        public async Task<Response> GetRange(int userId, int skip, int count,
            bool byDesc, int? priority, bool? isChecked)
        {
            try
            {
                var cacheKey = $"{ImmutableData.NOTIFICATIONS_PREFIX}{userId}_{skip}_{count}_{byDesc}_{priority}_{isChecked}";
                return new Response
                {
                    Status = 200,
                    ObjectData = await cacheHandler.CacheAndGetRange(
                        new NotificationRangeObject(cacheKey, userId, skip, count, byDesc, priority, isChecked))
                };
            }
            catch (EntityException ex)
            {
                return new Response { Status = 500, Message = ex.Message };
            }
            catch (FormatException ex)
            {
                return new Response { Status = 500, Message = ex.Message };
            }
        }

        public async Task<Response> DeleteOne(int userId, int notificationId)
        {
            try
            {
                var notification = await repository.DeleteByFilter(
                    new NotificationByIdAndByRelationSpec(notificationId, userId));
                if (notification is null)
                    return new Response { Status = 404, Message = Message.NOT_FOUND };

                await redisCache.DeteteCacheByKeyPattern($"{ImmutableData.NOTIFICATIONS_PREFIX}{userId}");
                return new Response { Status = 204 };
            }
            catch (EntityException ex)
            {
                return new Response { Status = 500, Message = ex.Message };
            }
        }
    }
}
