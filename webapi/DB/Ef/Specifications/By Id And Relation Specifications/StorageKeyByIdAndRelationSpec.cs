using Ardalis.Specification;
using webapi.Models;

namespace webapi.DB.Ef.Specifications.By_Relation_Specifications
{
    public class StorageKeyByIdAndRelationSpec : Specification<KeyStorageItemModel>
    {
        public StorageKeyByIdAndRelationSpec(int keyId, int storageId)
        {
            KeyId = keyId;
            StorageId = storageId;

            Query.Where(x => x.key_id.Equals(keyId) && x.storage_id.Equals(storageId));
        }

        public int KeyId { get; private set; }
        public int StorageId { get; private set; }
    }
}
