using Ardalis.Specification;
using webapi.Models;

namespace webapi.DB.Ef.Specifications
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
