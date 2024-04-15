using application.DTO;
using application.Services;

namespace application.Abstractions.Services.Endpoints
{
    public interface IPasswordService
    {
        public Task<Response> UpdatePassword(PasswordDTO passwordDTO, int id);
    }
}
