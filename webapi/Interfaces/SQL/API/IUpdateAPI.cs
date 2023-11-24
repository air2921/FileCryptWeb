using webapi.Models;

namespace webapi.Interfaces.SQL.API
{
    public interface IUpdateAPI
    {
        Task UpdateApiSetting(ApiModel apiModel);
    }
}
