using webapi.Models;

namespace webapi.Interfaces.SQL.Tokens
{
    public interface IReadToken
    {
        Task<TokenModel> ReadRefresh(int id);
        Task<TokenModel> ReadRefresh(TokenModel tokenModel);
        Task<int[]> ReadSuspectRefresh();
    }
}
