using UAParser;
using webapi.Interfaces.Services;

namespace webapi.Services.Security
{
    public class UserAgent : IUserAgent
    {
        public BrowserData GetBrowserData(ClientInfo clientInfo)
        {
            var browser = clientInfo.UA.Family;
            var browserVersion = clientInfo.UA.Major + "." + clientInfo.UA.Minor;
            var os = clientInfo.OS.Family;

            return new BrowserData(browser, browserVersion, os);
        }
    }

    public record BrowserData(string Browser, string Version, string OS);
}
