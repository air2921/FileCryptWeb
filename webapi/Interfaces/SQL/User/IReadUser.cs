using webapi.Models;

namespace webapi.Interfaces.SQL.User
{
    public interface IReadUser
    {
        Task<UserModel> ReadFullUser(int id);
        Task<List<UserModel>> ReadAllUsers();
    }
}
