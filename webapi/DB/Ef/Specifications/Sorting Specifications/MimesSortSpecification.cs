using Ardalis.Specification;
using webapi.Models;

namespace webapi.DB.Ef.Specifications.Sorting_Specifications
{
    public class MimesSortSpecification : Specification<FileMimeModel>
    {
        public MimesSortSpecification(int skip, int take)
        {
            Query.Skip(skip).Take(take);
        }
    }
}
