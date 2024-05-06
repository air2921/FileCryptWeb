using Ardalis.Specification;
using domain.Models;

namespace domain.Specifications.Sorting_Specifications
{
    public class StoragesSortSpec : Specification<KeyStorageModel>
    {
        public StoragesSortSpec(int? userId, int skip, int count, bool byDesc)
        {
            UserId = userId;
            SkipCount = skip;
            Count = count;
            ByDesc = byDesc;

            if (userId.HasValue)
                Query.Where(s => s.user_id.Equals(userId));

            if (byDesc)
                 Query.OrderByDescending(s => s.last_time_modified);
            else
                 Query.OrderBy(s => s.last_time_modified);

            Query.Skip(skip).Take(count);
        }

        public int? UserId { get; private set; }
        public int SkipCount { get; private set; }
        public int Count { get; private set; }
        public bool ByDesc { get; private set; }
    }
}
