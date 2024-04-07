using Ardalis.Specification;
using webapi.Models;

namespace webapi.DB.Ef.Specifications
{
    public class RecoveryTokenByTokenSpecification : Specification<LinkModel>
    {
        public RecoveryTokenByTokenSpecification(string token)
        {
            Query.Where(x => x.u_token.Equals(token));
        }
    }
}
