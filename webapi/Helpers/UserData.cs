using System.Security.Claims;
using webapi.Helpers.Abstractions;

namespace webapi.Helpers
{
    public class UserData(IHttpContextAccessor httpContextAccessor) : IUserInfo
    {
        private readonly HttpContext _httpContext = httpContextAccessor.HttpContext ?? throw new InvalidOperationException("User is not authenticated");

        public int UserId => GetIntClaimValue(ClaimTypes.NameIdentifier);

        public string Username => GetStringClaimValue(ClaimTypes.Name);

        public string Role => GetStringClaimValue(ClaimTypes.Role);

        public string Email => GetStringClaimValue(ClaimTypes.Email);

        public string RequestId => GetRequest();

        private string GetRequest()
        {
            Microsoft.Extensions.Primitives.StringValues token;
            if (_httpContext.Request.Headers.TryGetValue("X-REQUEST-TOKEN", out token))
                return token.FirstOrDefault() ?? "None";
            else
                return "None";
        }

        private string GetStringClaimValue(string claimType)
        {
            ClaimsPrincipal user = _httpContext.User;
            string? value = user.FindFirstValue(claimType);
            if (value is not null)
                return value;
            else
                throw new InvalidOperationException($"Cannot retrieve claim value for {claimType} as string");
        }

        private int GetIntClaimValue(string claimType)
        {
            ClaimsPrincipal user = _httpContext.User;
            string? value = user.FindFirstValue(claimType);
            if (value is not null && int.TryParse(value, out int result))
                return result;
            else
                throw new InvalidOperationException($"Cannot retrieve claim value for {claimType} as integer");
        }
    }
}
