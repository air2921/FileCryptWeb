using domain.Services;

namespace domain.Upper_Module.Services
{
    public interface IUsernameService
    {
        public Task<Response> UpdateUsername(string username, int id);
    }
}
