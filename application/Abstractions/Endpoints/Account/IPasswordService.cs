using application.DTO.Outer;
using application.Master_Services;

namespace application.Abstractions.Endpoints.Account
{
    public interface IPasswordService
    {
        public Task<Response> UpdatePassword(PasswordDTO passwordDTO, int id);
    }
}
