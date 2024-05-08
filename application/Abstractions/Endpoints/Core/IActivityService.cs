using application.Master_Services;

namespace application.Abstractions.Endpoints.Core
{
    public interface IActivityService
    {
        Task<Response> GetOne(int userId, int activityId);
        Task<Response> GetRange(int userId, bool byDesc, DateTime start, DateTime end, int? type = null);
    }
}
