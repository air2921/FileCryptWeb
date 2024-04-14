using domain.DTO;
using domain.Services;

namespace domain.Upper_Module.Services
{
    public interface IPasswordService
    {
        public Task<Response> UpdatePassword(PasswordDTO passwordDTO, int id);
    }
}
