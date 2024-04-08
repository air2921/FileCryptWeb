using Ardalis.Specification;
using webapi.Models;

namespace webapi.DB.Ef.Specifications.By_Relation_Specifications
{
    public class StorageByIdAndRelationSpec : Specification<KeyStorageModel>
    {
        public StorageByIdAndRelationSpec(int storageId, int userId)
        {
            StorageId = storageId;
            UserId = userId;

            Query.Where(x => x.storage_id.Equals(storageId) && x.user_id.Equals(userId));
        }

        public int StorageId { get; private set; }
        public int UserId { get; private set; }
    }
}
