using application.DTO.Outer;
using application.Master_Services;

namespace application.Abstractions.Services.Endpoints
{
    public interface IPasswordService
    {
        public Task<Response> UpdatePassword(PasswordDTO passwordDTO, int id);
    }
}
