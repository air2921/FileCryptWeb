using webapi.Exceptions;
using webapi.Interfaces.SQL;
using webapi.Localization.English;
using webapi.Models;

namespace webapi.DB.SQL.Notifications
{
    public class DeleteNotification : IDelete<NotificationModel>
    {
        private readonly FileCryptDbContext _dbContext;

        public DeleteNotification(FileCryptDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task DeleteById(int id)
        {
            var notification = await _dbContext.Notifications.FindAsync(id) ??
                throw new NotificationException(ExceptionNotificationMessages.NotificationNotFound);

            _dbContext.Notifications.Remove(notification);
            await _dbContext.SaveChangesAsync();
        }
    }
}
