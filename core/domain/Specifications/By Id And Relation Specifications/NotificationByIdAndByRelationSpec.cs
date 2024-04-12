using Ardalis.Specification;
using domain.Models;

namespace domain.Specifications.By_Relation_Specifications
{
    public class NotificationByIdAndByRelationSpec : Specification<NotificationModel>
    {
        public NotificationByIdAndByRelationSpec(int notificationId, int userId)
        {
            NotificationId = notificationId;
            UserId = userId;

            Query.Where(x => x.notification_id.Equals(notificationId) && x.user_id.Equals(userId));
        }

        public int NotificationId { get; private set; }
        public int UserId { get; private set; }
    }
}
