using Ardalis.Specification;
using domain.Models;

namespace domain.Specifications.By_Relation_Specifications
{
    public class FileByIdAndRelationSpec : Specification<FileModel>
    {
        public FileByIdAndRelationSpec(int fileId, int userId)
        {
            FileId = fileId;
            UserId = userId;

            Query.Where(x => x.file_id.Equals(fileId) && x.user_id.Equals(userId));
        }

        public int FileId { get; private set; }
        public int UserId { get; private set; }
    }
}
