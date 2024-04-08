using Ardalis.Specification;
using webapi.Models;

namespace webapi.DB.Ef.Specifications.By_Relation_Specifications
{
    public class RefreshTokenByTokenAndExpiresSpec : Specification<TokenModel>
    {
        public RefreshTokenByTokenAndExpiresSpec(int userId, DateTime expires)
        {
            UserId = userId;
            Expires = expires;

            Query.Where(x => x.user_id.Equals(userId) && x.expiry_date < expires);
        }

        public int UserId { get; private set; }
        public DateTime Expires { get; private set; }
    }
}
