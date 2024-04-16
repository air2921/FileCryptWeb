using application.Helpers;
using application.Helpers.Localization;
using domain.Abstractions.Data;
using domain.Models;
using domain.Specifications.Sorting_Specifications;

namespace application.Services.Master_Services.Admin
{
    public class Admin_NotificationService(IRepository<NotificationModel> repository, IRedisCache redisCache)
    {
        public async Task<Response> GetOne(int notificationId)
        {
            var notification = await repository.GetById(notificationId);
            if (notification is null)
                return new Response { Status = 404, Message = Message.NOT_FOUND };

            return new Response { Status = 200, ObjectData = new { notification } };
        }

        public async Task<Response> GetRange(int? userId, int skip, int count, bool byDesc)
        {
            return new Response
            {
                Status = 200,
                ObjectData = new
                {
                    notifications = await repository
                        .GetAll(new NotificationsSortSpec(userId, skip, count, byDesc, null, null))
                }
            };
        }

        public async Task<Response> DeleteOne(int notificationId)
        {
            var notification = await repository.Delete(notificationId);
            if (notification is null)
                return new Response { Status = 404, Message = Message.NOT_FOUND };

            await redisCache.DeteteCacheByKeyPattern($"{ImmutableData.NOTIFICATIONS_PREFIX}{notification.user_id}");

            return new Response { Status = 204 };
        }

        public async Task<Response> DeleteRange(IEnumerable<int> identifiers)
        {
            var notificationList = await repository.DeleteMany(identifiers);
            await redisCache.DeleteRedisCache(notificationList, ImmutableData.NOTIFICATIONS_PREFIX, item => item.user_id);
            return new Response { Status = 204 };
        }
    }
}
