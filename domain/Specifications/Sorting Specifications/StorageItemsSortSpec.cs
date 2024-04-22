using Ardalis.Specification;
using domain.Models;

namespace domain.Specifications.Sorting_Specifications
{
    public class StorageItemsSortSpec : Specification<KeyStorageItemModel>
    {
        public StorageItemsSortSpec(int storageId, int skip, int count, bool byDesc)
        {
            Query.Where(s => s.storage_id.Equals(storageId));

            if (byDesc)
                Query.OrderByDescending(s => s.created_at);
            else
                Query.OrderBy(s => s.created_at);

            Query.Skip(skip).Take(count);
        }
    }
}
