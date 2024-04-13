using domain.Services;

namespace domain.Abstractions.Services
{
    public interface IUsernameService
    {
        public Task<Response> UpdateUsername(string username, int id);
    }
}
