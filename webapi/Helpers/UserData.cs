using System.Security.Claims;
using webapi.Interfaces.Services;

namespace webapi.Helpers
{
    public class UserData : IUserInfo
    {
        private readonly HttpContext _httpContext;

        public UserData(IHttpContextAccessor httpContextAccessor)
        {
            _httpContext = httpContextAccessor.HttpContext ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        private int? _userId;
        private string? _username;
        private string? _role;
        private string? _email;

        public int UserId
        {
            get
            {
                ClaimsPrincipal user = _httpContext.User;
                return _userId ??= Convert.ToInt32(user.FindFirstValue(ClaimTypes.NameIdentifier));
            }
        }

        public string Username
        {
            get
            {
                ClaimsPrincipal user = _httpContext.User;
                return _username ??= user.FindFirstValue(ClaimTypes.Name);
            }
        }

        public string Role
        {
            get
            {
                ClaimsPrincipal user = _httpContext.User;
                return _role ??= user.FindFirstValue(ClaimTypes.Role);
            }
        }

        public string Email
        {
            get
            {
                ClaimsPrincipal user = _httpContext.User;
                return _email ??= user.FindFirstValue(ClaimTypes.Email);
            }
        }
    }
}
