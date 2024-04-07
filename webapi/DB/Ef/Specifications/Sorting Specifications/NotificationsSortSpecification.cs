using Ardalis.Specification;
using webapi.Models;

namespace webapi.DB.Ef.Specifications.Sorting_Specifications
{
    public class NotificationsSortSpecification : Specification<NotificationModel>
    {
        public NotificationsSortSpecification(int? userId, int skip, int count, bool byDesc, string? priority, bool? isChecked)
        {
            if (byDesc)
                Query.OrderByDescending(n => n.send_time);
            else
                Query.OrderBy(n => n.send_time);

            if (userId.HasValue)
                Query.Where(n => n.user_id.Equals(userId.Value));

            if (!string.IsNullOrWhiteSpace(priority))
                Query.Where(n => n.priority.Equals(priority));

            if (isChecked.HasValue)
                Query.Where(o => o.is_checked.Equals(isChecked));

            Query.Skip(skip).Take(count);
        }
    }
}
