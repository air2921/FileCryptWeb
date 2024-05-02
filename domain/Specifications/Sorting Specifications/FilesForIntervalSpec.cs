using Ardalis.Specification;
using domain.Models;

namespace domain.Specifications.Sorting_Specifications
{
    public class FilesForIntervalSpec : Specification<FileModel>
    {
        public FilesForIntervalSpec(int userId, bool byDesc, DateTime start, DateTime end)
        {
            UserId = userId;
            ByDesc = byDesc;
            Start = start;
            End = end;

            Query.Where(f => f.user_id.Equals(userId));
            Query.Where(f => f.operation_date >= start && f.operation_date < end);

            if (byDesc)
                Query.OrderByDescending(f => f.operation_date);
            else
                Query.OrderBy(f => f.operation_date);
        }

        public int UserId { get; set; }
        public bool ByDesc { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
    }
}
