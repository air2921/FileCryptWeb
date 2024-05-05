using application.Abstractions.Endpoints.Core;
using application.Cache_Handlers;
using application.DTO.Outer;
using application.Helpers;
using application.Helpers.Localization;
using domain.Exceptions;
using domain.Models;

namespace application.Master_Services.Core
{
    public class ActivityService(ICacheHandler<ActivityModel> cacheHandler) : IActivityService
    {
        public async Task<Response> GetOne(int userId, int activityId)
        {
            try
            {
                var cacheKey = $"{ImmutableData.ACTIVITY_PREFIX}{userId}_{activityId}";
                var activity = await cacheHandler.CacheAndGet(new ActivityObject(cacheKey, activityId, userId));
                if (activity is null)
                    return new Response { Status = 404, Message = Message.NOT_FOUND };
                else
                    return new Response { Status = 200, ObjectData = activity };
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

        public async Task<Response> GetRange(int userId, bool byDesc, DateTime start, DateTime end, string? type = null)
        {
            try
            {
                var cacheKey = $"{ImmutableData.ACTIVITY_PREFIX}{userId}_{byDesc}_{FormatDate(start)}_{FormatDate(end)}_{type}";

                return new Response
                {
                    Status = 200,
                    ObjectData = await cacheHandler.CacheAndGetRange(
                        new ActivityRangeObject(cacheKey, userId, byDesc, type, start, end))
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

        private string FormatDate(DateTime time) => time.ToString("dd.MM.yyyy");

        private HashSet<ActivityDTO> FormatRange(IEnumerable<ActivityModel> activities)
        {
            var dict = activities
                .GroupBy(activity => activity.action_date.ToString("dd.MM.yyyy"))
                .ToDictionary(
                    group => group.Key,
                    group => group.Count()
                );

            var activity = new HashSet<ActivityDTO>();
            foreach (var el in dict)
                activity.Add(new ActivityDTO { Date = el.Key, ActivityCount = el.Value });

            return activity;
        }
    }
}
