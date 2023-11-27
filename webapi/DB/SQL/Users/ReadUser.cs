using Microsoft.EntityFrameworkCore;
using webapi.Exceptions;
using webapi.Interfaces.SQL.User;
using webapi.Localization.English;
using webapi.Models;

namespace webapi.DB.SQL.Users
{
    public class ReadUser : IReadUser
    {
        private readonly FileCryptDbContext _dbContext;

        public ReadUser(FileCryptDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<UserModel> ReadFullUser(int id)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.id == id) ??
                throw new UserException(ExceptionUserMessages.UserNotFound);

            return user;
        }

        public async Task<List<UserModel>> ReadAllUsers()
        {
            var users = await _dbContext.Users.ToListAsync() ??
                throw new UserException(ExceptionUserMessages.NoOneUserNotFound);

            return users;
        }
    }
}
