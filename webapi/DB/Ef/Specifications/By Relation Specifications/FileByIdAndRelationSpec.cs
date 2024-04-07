using Ardalis.Specification;
using webapi.Models;

namespace webapi.DB.Ef.Specifications.By_Relation_Specifications
{
    public class FileByIdAndRelationSpec : Specification<FileModel>
    {
        public FileByIdAndRelationSpec(int fileId, int userId)
        {
            Query.Where(x => x.file_id.Equals(fileId) && x.user_id.Equals(userId));
        }
    }
}
