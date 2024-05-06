using Ardalis.Specification;
using domain.Models;

namespace domain.Specifications.Sorting_Specifications
{
    public class MimesSortSpec : Specification<MimeModel>
    {
        public MimesSortSpec(int skip, int take)
        {
            Query.Skip(skip).Take(take);
        }

        public int SkipCount { get; private set; }
        public int Count { get; private set; }
    }
}
