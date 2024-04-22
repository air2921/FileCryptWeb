using application.Master_Services;

namespace application.Abstractions.Endpoints.Admin
{
    public interface IAdminNotificationService
    {
        Task<Response> GetOne(int notificationId);
        Task<Response> GetRange(int? userId, int skip, int count, bool byDesc);
        Task<Response> DeleteOne(int notificationId);
        Task<Response> DeleteRange(IEnumerable<int> identifiers);
    }
}
