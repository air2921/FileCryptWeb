using Ardalis.Specification;
using webapi.Models;

namespace webapi.DB.Ef.Specifications.Sorting_Specifications
{
    public class LinksSortSpec : Specification<LinkModel>
    {
        public LinksSortSpec(int? userId, int skip, int count, bool byDesc, bool? expired)
        {
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
    }
}
