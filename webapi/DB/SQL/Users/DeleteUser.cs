using Microsoft.EntityFrameworkCore;
using webapi.Exceptions;
using webapi.Interfaces.SQL;
using webapi.Localization.English;
using webapi.Models;

namespace webapi.DB.SQL.Users
{
    public class DeleteUser : IDelete<UserModel>
    {
        private readonly FileCryptDbContext _dbContext;

        public DeleteUser(FileCryptDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task DeleteById(int id)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.id == id) ??
                throw new UserException(ExceptionUserMessages.UserNotFound);

            _dbContext.Users.Remove(user);
            await _dbContext.SaveChangesAsync();
        }
    }
}
