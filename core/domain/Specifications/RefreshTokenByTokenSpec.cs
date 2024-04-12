using Ardalis.Specification;
using domain.Models;

namespace domain.Specifications
{
    public class RefreshTokenByTokenSpec : Specification<TokenModel>
    {
        public RefreshTokenByTokenSpec(string token)
        {
            Token = token;

            Query.Where(x => x.refresh_token.Equals(token));
        }

        public string Token { get; private set; }
    }
}
