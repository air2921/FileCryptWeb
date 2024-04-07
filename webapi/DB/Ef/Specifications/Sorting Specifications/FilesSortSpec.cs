using Ardalis.Specification;
using webapi.Models;

namespace webapi.DB.Ef.Specifications.Sorting_Specifications
{
    public class FilesSortSpec : Specification<FileModel>
    {
        public FilesSortSpec(int? userId, int skip, int count, bool byDesc, string? type, string? mime, string? category)
        {
            if (byDesc)
                Query.OrderByDescending(f => f.operation_date);
            else
                Query.OrderBy(f => f.operation_date);

            if (userId.HasValue)
                Query.Where(f => f.user_id.Equals(userId));

            if (!string.IsNullOrWhiteSpace(type))
                Query.Where(f => f.type.Equals(type));

            if (!string.IsNullOrWhiteSpace(category))
                Query.Where(f => f.file_mime_category.Equals(category));

            if (!string.IsNullOrWhiteSpace(mime))
                Query.Where(f => f.file_mime.Equals(mime));

            Query.Skip(skip).Take(count);
        }
    }
}
