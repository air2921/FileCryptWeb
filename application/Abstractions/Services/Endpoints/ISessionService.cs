using application.DTO.Outer;
using application.Master_Services;

namespace application.Abstractions.Services.Endpoints
{
    internal interface ISessionService
    {
        public Task<Response> Login(LoginDTO dto);
        public Task<Response> Verify2Fa(int code, string email);
        public Task<Response> Logout(string refresh);
        public Task<Response> UpdateJwt(string refresh);
    }
}
