using Ardalis.Specification;
using domain.Models;

namespace domain.Specifications.Sorting_Specifications
{
    public class LinksSortSpec : Specification<LinkModel>
    {
        public LinksSortSpec(int? userId, int skip, int count, bool byDesc, bool? expired)
        {
            UserId = userId;
            SkipCount = skip;
            Count = count;
            ByDesc = byDesc;
            Expired = expired;

            if (byDesc)
                Query.OrderByDescending(l => l.created_at);
            else
                Query.OrderBy(l => l.created_at);

            if (userId.HasValue)
                Query.Where(l => l.user_id.Equals(userId.Value));

            if (expired.HasValue)
            {
                if (expired.Equals(true))
                    Query.Where(l => l.expiry_date < DateTime.UtcNow);
                else
                    Query.Where(l => l.expiry_date > DateTime.UtcNow);
            }

            Query.Skip(skip).Take(count);
        }

        public int? UserId { get; private set; }
        public int SkipCount { get; private set; }
        public int Count { get; private set; }
        public bool ByDesc { get; private set; }
        public bool? Expired { get; private set; }
    }
}
