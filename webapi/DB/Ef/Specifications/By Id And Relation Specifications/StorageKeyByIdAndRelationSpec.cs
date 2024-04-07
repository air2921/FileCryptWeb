using Ardalis.Specification;
using webapi.Models;

namespace webapi.DB.Ef.Specifications.By_Relation_Specifications
{
    public class StorageKeyByIdAndRelationSpec : Specification<KeyStorageItemModel>
    {
        public StorageKeyByIdAndRelationSpec(int keyId, int storageId)
        {
            Query.Where(x => x.key_id.Equals(keyId) && x.storage_id.Equals(storageId));
        }
    }
}
