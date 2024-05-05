using application.Helpers.Localization;
using domain.Abstractions.Data;
using domain.Exceptions;
using domain.Models;
using domain.Specifications.By_Id_And_Relation_Specifications;
using domain.Specifications.Sorting_Specifications;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace application.Cache_Handlers
{
    public class Activity(
        IRepository<ActivityModel> repository,
        IRedisCache redisCache,
        ILogger<Activity> logger) : ICacheHandler<ActivityModel>
    {
        public async Task<ActivityModel> CacheAndGet(object dataObject)
        {
            try
            {
                var activityObj = dataObject as ActivityObject ?? throw new FormatException(Message.ERROR);
                var activity = new ActivityModel();

                var cache = await redisCache.GetCachedData(activityObj.CacheKey);
                if (cache is null)
                {
                    activity = await repository.GetByFilter(
                        new ActivityByIdAndRelationSpec(activityObj.ActivityId, activityObj.UserId));

                    if (activity is null)
                        return null;

                    await redisCache.CacheData(activityObj.CacheKey, activity, TimeSpan.FromMinutes(5));
                    return activity;
                }

                activity = JsonConvert.DeserializeObject<ActivityModel>(cache);
                if (activity is not null)
                    return activity;
                else
                    return null;
            }
            catch (EntityException)
            {
                throw;
            }
            catch (JsonException ex)
            {
                logger.LogCritical(ex.ToString(), nameof(Activity));
                throw new FormatException(Message.ERROR);
            }
        }

        public async Task<IEnumerable<ActivityModel>> CacheAndGetRange(object dataObject)
        {
            try
            {
                var activityObj = dataObject as ActivityRangeObject ?? throw new FormatException(Message.ERROR);
                var activity = new List<ActivityModel>();

                var cache = await redisCache.GetCachedData(activityObj.CacheKey);
                if (cache is null)
                {
                    activity = (List<ActivityModel>)await repository
                        .GetAll(new ActivitySortSpec(activityObj.UserId, activityObj.ByDesc, activityObj.Start, activityObj.End, activityObj.Type));

                    await redisCache.CacheData(activityObj.CacheKey, activity, TimeSpan.FromMinutes(20));
                    return activity;
                }

                activity = JsonConvert.DeserializeObject<List<ActivityModel>>(cache);
                if (activity is not null)
                    return activity;
                else
                    throw new FormatException(Message.ERROR);
            }
            catch (EntityException)
            {
                throw;
            }
            catch (JsonException ex)
            {
                logger.LogCritical(ex.ToString(), nameof(Activity));
                throw new FormatException(Message.ERROR);
            }
        }
    }

    public record class ActivityObject(string CacheKey, int ActivityId, int UserId);
    public record class ActivityRangeObject(string CacheKey, int UserId, bool ByDesc, string? Type, DateTime Start, DateTime End);
}
