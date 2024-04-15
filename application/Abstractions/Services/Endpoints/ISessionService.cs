using application.DTO;
using application.Services;

namespace application.Abstractions.Services.Endpoints
{
    internal interface ISessionService
    {
        public Task<Response> Login(LoginDTO dto, string refresh);
        public Task<Response> Verify2Fa(int code, string email, string refresh);
        public Task<Response> Logout(string refresh);
    }
}
