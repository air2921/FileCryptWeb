using application.DTO.Outer;
using application.Master_Services;

namespace application.Abstractions.Services.Endpoints
{
    public interface IRegistrationService
    {
        public Task<Response> Registration(RegisterDTO dto);
        public Task<Response> VerifyAccount(int code, string email);
    }
}
