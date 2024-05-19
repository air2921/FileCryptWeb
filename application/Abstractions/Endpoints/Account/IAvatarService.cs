using application.Master_Services;

namespace application.Abstractions.Endpoints.Account
{
    public interface IAvatarService
    {
        public Task<Response> Delete(int userId);
        public Task<Response> Change(Stream stream, string name, string contentType, int userId, string avatarId);
        public Task<Response> Download(int userId);
    }
}
