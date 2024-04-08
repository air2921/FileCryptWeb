using Ardalis.Specification;
using webapi.Models;

namespace webapi.DB.Ef.Specifications.By_Relation_Specifications
{
    public class KeysByRelationSpec : Specification<KeyModel>
    {
        public KeysByRelationSpec(int userId)
        {
            UserId = userId;

            Query.Where(x => x.user_id.Equals(userId));
        }

        public int UserId { get; private set; }
    }
}
