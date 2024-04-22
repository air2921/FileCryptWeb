using application.Master_Services;

namespace application.Abstractions.Endpoints.Admin
{
    public interface IAdminUserService
    {
        Task<Response> DeleteUser(int userId);
        Task<Response> BlockUser(int userId, bool block);
    }
}
