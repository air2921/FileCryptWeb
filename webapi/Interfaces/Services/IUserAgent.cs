using UAParser;
using webapi.Security;

namespace webapi.Interfaces.Services
{
    public interface IUserAgent
    {
        BrowserData GetBrowserData(ClientInfo clientInfo);
    }
}
