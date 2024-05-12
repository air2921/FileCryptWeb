using application.Master_Services;

namespace application.Abstractions.Endpoints.Core
{
    public interface INotificationService
    {
        Task<Response> GetOne(int userId, int notificationId);
        Task<Response> GetRange(int userId, int skip, int count, bool byDesc, int? priority, bool? isChecked);
        Task<Response> DeleteOne(int userId, int notificationId);
    }
}
