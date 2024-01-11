using UAParser;
using webapi.Services.Security;

namespace webapi.Interfaces.Services
{
    public interface IUserAgent
    {
        BrowserData GetBrowserData(ClientInfo clientInfo);
    }
}
