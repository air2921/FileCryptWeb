using System.Security.Claims;
using webapi.Interfaces.Services;

namespace webapi.Helpers
{
    public class UserData : IUserInfo
    {
        private readonly HttpContext _httpContext;

        public UserData(IHttpContextAccessor httpContextAccessor)
        {
            _httpContext = httpContextAccessor.HttpContext ?? throw new InvalidOperationException("User is not authenticated");
        }

        public int UserId => GetIntClaimValue(ClaimTypes.NameIdentifier);

        public string Username => GetStringClaimValue(ClaimTypes.Name);

        public string Role => GetStringClaimValue(ClaimTypes.Role);

        public string Email => GetStringClaimValue(ClaimTypes.Email);

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
