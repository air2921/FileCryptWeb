using application.Master_Services;

namespace application.Abstractions.Endpoints.Account
{
    public interface IUsernameService
    {
        public Task<Response> UpdateUsername(string username, int id);
    }
}
