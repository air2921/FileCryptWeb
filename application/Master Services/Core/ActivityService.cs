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

        public async Task<Response> GetRange(int userId, bool byDesc, DateTime start, DateTime end, int? type = null)
        {
            try
            {
                var cacheKey = $"{ImmutableData.ACTIVITY_PREFIX}{userId}_{byDesc}_{FormatDate(start)}_{FormatDate(end)}_{type}";

                return new Response
                {
                    Status = 200,
                    ObjectData = FormatRange(await cacheHandler.CacheAndGetRange(
                        new ActivityRangeObject(cacheKey, userId, byDesc, type, start, end)))
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
            var groupedActivities = activities
                .GroupBy(activity => activity.action_date.ToString("dd.MM.yyyy"))
                .Select(group => new ActivityDTO
                {
                    Date = group.Key,
                    ActivityCount = group.Count(),
                    Activities = group.ToArray()
                })
                .ToHashSet();

            return groupedActivities;
        }
    }
}
