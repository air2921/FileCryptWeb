using application.Abstractions.Endpoints.Admin;
using application.Helpers;
using application.Helpers.Localization;
using domain.Abstractions.Data;
using domain.Exceptions;
using domain.Models;
using domain.Specifications.Sorting_Specifications;

namespace application.Master_Services.Admin
{
    public class Admin_NotificationService(
        IRepository<NotificationModel> repository,
        IRedisCache redisCache) : IAdminNotificationService
    {
        public async Task<Response> GetOne(int notificationId)
        {
            try
            {
                var notification = await repository.GetById(notificationId);
                if (notification is null)
                    return new Response { Status = 404, Message = Message.NOT_FOUND };

                return new Response { Status = 200, ObjectData = notification };
            }
            catch (EntityException ex)
            {
                return new Response { Status = 500, Message = ex.Message };
            }
        }

        public async Task<Response> GetRange(int? userId, int skip, int count, bool byDesc)
        {
            try
            {
                return new Response
                {
                    Status = 200,
                    ObjectData = await repository
                            .GetAll(new NotificationsSortSpec(userId, skip, count, byDesc, null, null))
                };
            }
            catch (EntityException ex)
            {
                return new Response { Status = 500, Message = ex.Message };
            }
        }

        public async Task<Response> DeleteOne(int notificationId)
        {
            try
            {
                var notification = await repository.Delete(notificationId);
                if (notification is null)
                    return new Response { Status = 404, Message = Message.NOT_FOUND };

                await redisCache.DeteteCacheByKeyPattern($"{ImmutableData.NOTIFICATIONS_PREFIX}{notification.user_id}");

                return new Response { Status = 204 };
            }
            catch (EntityException ex)
            {
                return new Response { Status = 500, Message = ex.Message };
            }
        }

        public async Task<Response> DeleteRange(IEnumerable<int> identifiers)
        {
            try
            {
                var notificationList = await repository.DeleteMany(identifiers);
                await redisCache.DeleteRedisCache(notificationList, ImmutableData.NOTIFICATIONS_PREFIX, item => item.user_id);
                return new Response { Status = 204 };
            }
            catch (EntityException ex)
            {
                return new Response { Status = 500, Message = ex.Message };
            }
        }
    }
}
