using Microsoft.EntityFrameworkCore;
using webapi.Exceptions;
using webapi.Interfaces.SQL;
using webapi.Localization.English;
using webapi.Models;

namespace webapi.DB.SQL
{
    public class Notifications : ICreate<NotificationModel>, IDelete<NotificationModel>, IRead<NotificationModel>
    {
        private readonly FileCryptDbContext _dbContext;

        public Notifications(FileCryptDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task Create(NotificationModel notificationModel)
        {
            bool senderExists = await _dbContext.Users.AnyAsync(u => u.id == notificationModel.sender_id);
            bool receiverExists = await _dbContext.Users.AnyAsync(u => u.id == notificationModel.receiver_id);
            if (!senderExists || !receiverExists)
                throw new UserException(AccountErrorMessage.UserNotFound);

            await _dbContext.AddAsync(notificationModel);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<NotificationModel> ReadById(int id, bool? byForeign)
        {
            return await _dbContext.Notifications.FirstOrDefaultAsync(u => u.notification_id == id) ??
                throw new NotificationException(ExceptionNotificationMessages.NotificationNotFound);
        }

        public async Task<IEnumerable<NotificationModel>> ReadAll()
        {
            return await _dbContext.Notifications.ToListAsync();
        }

        public async Task DeleteById(int id)
        {
            var notification = await _dbContext.Notifications.FirstOrDefaultAsync(u => u.notification_id == id) ??
                throw new NotificationException(ExceptionNotificationMessages.NotificationNotFound);

            _dbContext.Notifications.Remove(notification);
            await _dbContext.SaveChangesAsync();
        }
    }
}
