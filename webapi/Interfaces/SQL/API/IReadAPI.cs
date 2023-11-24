using webapi.Models;

namespace webapi.Interfaces.SQL.API
{
    public interface IReadAPI
    {
        Task<ApiModel> ReadUserApiSettings(int id);
        Task<int> ReadUserIdByApiKey(string apiKey);
    }
}
