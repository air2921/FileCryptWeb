using application.DTO.Outer;
using application.Services;

namespace application.Abstractions.Services.Endpoints
{
    public interface IPasswordService
    {
        public Task<Response> UpdatePassword(PasswordDTO passwordDTO, int id);
    }
}
