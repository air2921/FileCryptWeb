using domain.DTO;
using domain.Services;

namespace domain.Upper_Module.Services
{
    internal interface ISessionService
    {
        public Task<Response> Login(LoginDTO dto, string refresh);
        public Task<Response> Verify2Fa(int code, string email, string refresh);
        public Task<Response> Logout(string refresh);
    }
}
