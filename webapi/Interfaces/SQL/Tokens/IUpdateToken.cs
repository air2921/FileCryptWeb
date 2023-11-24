using webapi.Models;

namespace webapi.Interfaces.SQL.Tokens
{
    public interface IUpdateToken
    {
        Task UpdateRefreshToken(TokenModel tokenModel, string searchField);
    }
}
