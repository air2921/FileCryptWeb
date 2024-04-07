using Ardalis.Specification;
using webapi.Models;

namespace webapi.DB.Ef.Specifications.By_Relation_Specifications
{
    public class StorageKeysByRelationSpec : Specification<KeyStorageItemModel>
    {
        public StorageKeysByRelationSpec(int storageId)
        {
            Query.Where(x => x.storage_id.Equals(storageId));
        }
    }
}
