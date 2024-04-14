using domain.DTO;
using domain.Services;

namespace domain.Upper_Module.Services
{
    public interface IRegistrationService
    {
        public Task<Response> Registration(RegisterDTO dto);
        public Task<Response> VerifyAccount(int code, string email);
    }
}
