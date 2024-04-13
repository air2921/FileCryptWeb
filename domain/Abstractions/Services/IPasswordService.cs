using domain.DTO;
using domain.Services;

namespace domain.Abstractions.Services
{
    public interface IPasswordService
    {
        public Task<Response> UpdatePassword(PasswordDTO passwordDTO, int id);
    }
}
