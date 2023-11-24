using webapi.Exceptions;
using webapi.Interfaces.Services;
using webapi.Interfaces.SQL.User;
using webapi.Localization.English;
using webapi.Models;

namespace webapi.DB.SQL.Users
{
    public class UpdateUser : IUpdateUser
    {
        private readonly FileCryptDbContext _dbContext;
        private readonly IPasswordManager _passwordManager;

        public UpdateUser(IPasswordManager passwordManager, FileCryptDbContext dbContext)
        {
            _passwordManager = passwordManager;
            _dbContext = dbContext;
        }

        public async Task UpdateUsernameById(UserModel userModel)
        {
            var existingUser = await _dbContext.Users.FindAsync(userModel.id) ??
                throw new UserException(ExceptionUserMessages.UserNotFound);

            existingUser.username = userModel.username;
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdatePasswordById(UserModel userModel)
        {
            var existingUser = await _dbContext.Users.FindAsync(userModel.id) ??
                throw new UserException(ExceptionUserMessages.UserNotFound);

            existingUser.password_hash = _passwordManager.HashingPassword(userModel.password_hash);

            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateEmailById(UserModel userModel)
        {
            var existingUser = await _dbContext.Users.FindAsync(userModel.id) ??
                throw new UserException(ExceptionUserMessages.UserNotFound);

            existingUser.email = userModel.email;
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateRoleById(UserModel userModel)
        {
            var existingUser = await _dbContext.Users.FindAsync(userModel.id) ??
                throw new UserException(ExceptionUserMessages.UserNotFound);

            existingUser.role = userModel.role;
            await _dbContext.SaveChangesAsync();
        }
    }
}
