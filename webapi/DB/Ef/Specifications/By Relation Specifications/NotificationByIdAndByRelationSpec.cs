using Ardalis.Specification;
using webapi.Models;

namespace webapi.DB.Ef.Specifications.By_Relation_Specifications
{
    public class NotificationByIdAndByRelationSpec : Specification<NotificationModel>
    {
        public NotificationByIdAndByRelationSpec(int notificationId, int userId)
        {
            Query.Where(x => x.notification_id.Equals(notificationId) && x.user_id.Equals(userId));
        }
    }
}
