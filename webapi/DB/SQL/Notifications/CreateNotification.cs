using Microsoft.EntityFrameworkCore;
using webapi.Exceptions;
using webapi.Interfaces.SQL;
using webapi.Localization.English;
using webapi.Models;

namespace webapi.DB.SQL.Notifications
{
    public class CreateNotification : ICreate<NotificationModel>
    {
        private readonly FileCryptDbContext _dbContext;

        public CreateNotification(FileCryptDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task Create(NotificationModel notificationModel)
        {
            bool exists = await _dbContext.Users.AnyAsync(u => u.id == notificationModel.sender_id && u.id == notificationModel.receiver_id);
            if (!exists)
                throw new UserException(AccountErrorMessage.UserNotFound);

            await _dbContext.AddAsync(notificationModel);
            await _dbContext.SaveChangesAsync();
        }
    }
}
