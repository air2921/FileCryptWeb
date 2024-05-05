using Ardalis.Specification;
using domain.Models;

namespace domain.Specifications.Sorting_Specifications
{
    public class ActivitySortSpec : Specification<ActivityModel>
    {
        public ActivitySortSpec(int userId, bool byDesc, DateTime start, DateTime end, string? type = null)
        {
            UserId = userId;
            ByDesc = byDesc;
            Start = start;
            End = end;

            Query.Where(a => a.user_id.Equals(userId));

            if (type is not null)
                Query.Where(a => a.action_type.Equals(type));

            Query.Where(a => a.action_date >= start.ToUniversalTime() && a.action_date < end.ToUniversalTime());

            if (byDesc)
                Query.OrderByDescending(a => a.action_date);
            else
                Query.OrderBy(a => a.action_date);
        }

        public int UserId { get; private set; }
        public bool ByDesc { get; private set; }
        public DateTime Start { get; private set; }
        public DateTime End { get; private set; }
    }
}
