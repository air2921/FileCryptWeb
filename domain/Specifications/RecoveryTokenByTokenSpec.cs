using Ardalis.Specification;
using domain.Models;

namespace domain.Specifications
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
