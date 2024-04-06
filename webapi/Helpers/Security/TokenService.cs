using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using webapi.DB.Ef;
using webapi.Helpers.Abstractions;
using webapi.Models;

namespace webapi.Helpers.Security
{
    public class TokenService(
        IConfiguration configuration,
        IGenerate generate,
        FileCryptDbContext dbContext,
        IHttpContextAccessor context) : ITokenService
    {
        public string GenerateJwtToken(UserModel userModel, TimeSpan expiry)
        {
            SymmetricSecurityKey key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(configuration[App.SECRET_KEY]!));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, userModel.username),
                new Claim(ClaimTypes.NameIdentifier, userModel.id.ToString()),
                new Claim(ClaimTypes.Email, userModel.email),
                new Claim(ClaimTypes.Role, userModel.role)
            };

            var token = new JwtSecurityToken(
                issuer: "FileCrypt",
                audience: "User",
                claims: claims,
                expires: DateTime.UtcNow + expiry,
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string GenerateRefreshToken()
        {
            var str = Guid.NewGuid().ToString("N") + "_" + generate.GenerateKey() + "_" + Guid.NewGuid().ToString();

            return InsertRandomChars(str, 75);
        }

        public string HashingToken(string token)
        {
            byte[] hashBytes = SHA512.HashData(Encoding.UTF8.GetBytes(token));

            return Convert.ToBase64String(hashBytes);
        }

        private string GenerateRandomChars(int numChars)
        {
            string chars = ".!@#$%^&*()_-+=<>?/{}[]";
            Random random = new();
            char[] randomArray = new char[numChars];

            for (int i = 0; i < numChars; i++)
            {
                randomArray[i] = chars[random.Next(chars.Length)];
            }

            return new string(randomArray);
        }

        private string InsertRandomChars(string inputStr, int numChars)
        {
            string randomChars = GenerateRandomChars(numChars);

            Random random = new();
            int insertPosition = random.Next(0, inputStr.Length);

            string outputStr = inputStr.Insert(insertPosition, randomChars);

            return outputStr;
        }

        public CookieOptions SetCookieOptions(TimeSpan expireTime)
        {
            return new CookieOptions { HttpOnly = true, MaxAge = expireTime, Secure = true, SameSite = SameSiteMode.None };
        }

        public async Task UpdateJwtToken()
        {
            if (!context.HttpContext.Request.Cookies.TryGetValue(ImmutableData.REFRESH_COOKIE_KEY, out string? refresh))
                throw new UnauthorizedAccessException("Refresh Token was not found");

            var userAndToken = await dbContext.Tokens
                .Where(t => t.refresh_token == HashingToken(refresh))
                .Join(dbContext.Users, token => token.user_id, user => user.id, (token, user) => new { token, user })
                .FirstOrDefaultAsync() ?? throw new UnauthorizedAccessException("User was not found");

            if (userAndToken.token.expiry_date < DateTime.UtcNow || userAndToken.user.is_blocked)
            {
                DeleteTokens();
                throw new UnauthorizedAccessException("Refresh Token is invalid");
            }

            if (context.HttpContext.Request.Cookies.ContainsKey(ImmutableData.JWT_COOKIE_KEY))
                context.HttpContext.Response.Cookies.Delete(ImmutableData.JWT_COOKIE_KEY);

            string jwt = GenerateJwtToken(userAndToken.user, ImmutableData.JwtExpiry);
            context.HttpContext.Response.Cookies.Append(ImmutableData.JWT_COOKIE_KEY, jwt, SetCookieOptions(ImmutableData.JwtExpiry));
        }

        public void DeleteTokens()
        {
            context.HttpContext.Response.Cookies.Delete(ImmutableData.REFRESH_COOKIE_KEY);
            context.HttpContext.Response.Cookies.Delete(ImmutableData.JWT_COOKIE_KEY);

            context.HttpContext.Response.Cookies.Delete(ImmutableData.IS_AUTHORIZED);
            context.HttpContext.Response.Cookies.Delete(ImmutableData.USERNAME_COOKIE_KEY);
            context.HttpContext.Response.Cookies.Delete(ImmutableData.USER_ID_COOKIE_KEY);
            context.HttpContext.Response.Cookies.Delete(ImmutableData.ROLE_COOKIE_KEY);
        }

        public void DeleteUserDataSession()
        {
            context.HttpContext.Response.Cookies.Delete(ImmutableData.IS_AUTHORIZED);
            context.HttpContext.Response.Cookies.Delete(ImmutableData.USERNAME_COOKIE_KEY);
            context.HttpContext.Response.Cookies.Delete(ImmutableData.USER_ID_COOKIE_KEY);
            context.HttpContext.Response.Cookies.Delete(ImmutableData.ROLE_COOKIE_KEY);
        }
    }
}
