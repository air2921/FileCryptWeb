using domain.DTO;
using domain.Services;

namespace domain.Abstractions.Services
{
    public interface IRegistrationService
    {
        public Task<Response> Registration(RegisterDTO dto);
        public Task<Response> VerifyAccount(int code, string email);
    }
}
