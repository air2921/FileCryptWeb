using application.Master_Services;

namespace application.Abstractions.Endpoints.Core
{
    public interface IUserService
    {
        Task<Response> GetOne(int ownerId, int targetId);
        Task<Response> GetRange(string username);
        Task<Response> DeleteOne(int userId);
    }
}
