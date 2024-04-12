using Ardalis.Specification;
using domain.Models;

namespace domain.Specifications.By_Relation_Specifications
{
    public class StoragesByRelationSpec : Specification<KeyStorageModel>
    {
        public StoragesByRelationSpec(int userId)
        {
            UserId = userId;
            Query.Where(x => x.user_id.Equals(userId));
        }
        public int UserId
        {
            get; private set;
        }
    }
}
