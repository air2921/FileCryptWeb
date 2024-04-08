using Ardalis.Specification;
using webapi.Models;

namespace webapi.DB.Ef.Specifications
{
    public class RecoveryTokenByTokenSpec : Specification<LinkModel>
    {
        public RecoveryTokenByTokenSpec(string token)
        {
            Query.Where(x => x.u_token.Equals(token));

            Token = token;
        }

        public string Token { get; private set; }
    }
}
