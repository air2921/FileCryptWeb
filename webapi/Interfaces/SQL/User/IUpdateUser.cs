using webapi.Models;

namespace webapi.Interfaces.SQL.User
{
    public interface IUpdateUser
    {
        Task UpdatePasswordById(UserModel user);
        Task UpdateUsernameById(UserModel user);
        Task UpdateEmailById(UserModel user);
        Task UpdateRoleById(UserModel user);
    }
}
