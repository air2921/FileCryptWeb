using Ardalis.Specification;
using webapi.Models;

namespace webapi.DB.Ef.Specifications.Sorting_Specifications
{
    public class MimesSortSpec : Specification<FileMimeModel>
    {
        public MimesSortSpec(int skip, int take)
        {
            Query.Skip(skip).Take(take);
        }

        public int SkipCount { get; private set; }
        public int Count { get; private set; }
    }
}
