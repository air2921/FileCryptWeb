using application.DTO;
using application.Services;
using domain.DTO;

namespace application.Abstractions.Services.Endpoints
{
    public interface IRegistrationService
    {
        public Task<Response> Registration(RegisterDTO dto);
        public Task<Response> VerifyAccount(int code, string email);
    }
}
