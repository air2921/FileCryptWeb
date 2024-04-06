using webapi.Models;

namespace webapi.Helpers.Abstractions
{
    public interface ITokenService
    {
        public string GenerateRefreshToken();
        public string GenerateJwtToken(UserModel userModel, TimeSpan expiry);
        public CookieOptions SetCookieOptions(TimeSpan expireTime);
        public Task UpdateJwtToken();
        public string HashingToken(string token);
        public void DeleteTokens();
        public void DeleteUserDataSession();
    }
}
