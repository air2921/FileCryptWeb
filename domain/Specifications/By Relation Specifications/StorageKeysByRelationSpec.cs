using Ardalis.Specification;
using domain.Models;

namespace domain.Specifications.By_Relation_Specifications
{
    public class StorageKeysByRelationSpec : Specification<KeyStorageItemModel>
    {
        public StorageKeysByRelationSpec(int storageId)
        {
            StorageId = storageId;

            Query.Where(x => x.storage_id.Equals(storageId));
        }

        public int StorageId { get; private set; }
    }
}
