using Ardalis.Specification;
using webapi.Models;

namespace webapi.DB.Ef.Specifications
{
    public class RecoveryTokenByTokenSpec : Specification<LinkModel>
    {
        public RecoveryTokenByTokenSpec(string token)
        {
            Token = token;

            Query.Where(x => x.u_token.Equals(token));
        }

        public string Token { get; private set; }
    }
}
