using application.Services;

namespace application.Abstractions.Services.Endpoints
{
    public interface IUsernameService
    {
        public Task<Response> UpdateUsername(string username, int id);
    }
}
