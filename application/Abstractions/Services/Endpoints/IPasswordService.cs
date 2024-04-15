using application.DTO;
using application.Services;
using domain.DTO;

namespace application.Abstractions.Services.Endpoints
{
    public interface IPasswordService
    {
        public Task<Response> UpdatePassword(PasswordDTO passwordDTO, int id);
    }
}
