using Ardalis.Specification;
using domain.Models;

namespace domain.Specifications.Sorting_Specifications
{
    public class FilesSortSpec : Specification<FileModel>
    {
        public FilesSortSpec(int? userId, int skip, int count, bool byDesc, string? mime, string? category)
        {
            UserId = userId;
            SkipCount = skip;
            Count = count;
            ByDesc = byDesc;
            Mime = mime;
            Category = category;

            if (byDesc)
                Query.OrderByDescending(f => f.operation_date);
            else
                Query.OrderBy(f => f.operation_date);

            if (userId.HasValue)
                Query.Where(f => f.user_id.Equals(userId));

            if (!string.IsNullOrWhiteSpace(category))
                Query.Where(f => f.file_mime_category.Equals(category));

            if (!string.IsNullOrWhiteSpace(mime))
                Query.Where(f => f.file_mime.Equals(mime));

            Query.Skip(skip).Take(count);
        }

        public int? UserId { get; private set; }
        public int SkipCount { get; private set; }
        public int Count { get; private set; }
        public bool ByDesc { get; private set; }
        public string? Type { get; private set; }
        public string? Mime { get; private set; }
        public string? Category { get; private set; }
    }
}
