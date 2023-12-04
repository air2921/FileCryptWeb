using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using webapi.DB;
using webapi.Interfaces.Services;
using webapi.Models;

namespace webapi.Services.Security
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;
        private readonly IGenerateKey _generateKey;
        private readonly FileCryptDbContext _dbContext;
        private readonly IHttpContextAccessor _context;

        public TokenService(
            IConfiguration configuration,
            IGenerateKey generateKey,
            FileCryptDbContext dbContext,
            IHttpContextAccessor context)
        {
            _configuration = configuration;
            _generateKey = generateKey;
            _dbContext = dbContext;
            _context = context;
        }

        public string GenerateJwtToken(UserModel userModel, int lifetimeInMin)
        {
            SymmetricSecurityKey key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_configuration["SecretKey"]!));
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
                expires: DateTime.UtcNow.AddMinutes(lifetimeInMin),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string GenerateRefreshToken()
        {
            return Guid.NewGuid().ToString() + "_" + _generateKey.GenerateKey();
        }

        public CookieOptions SetCookieOptions(TimeSpan expireTime)
        {
            return new CookieOptions { HttpOnly = true, Expires = DateTime.UtcNow.Add(expireTime), Secure = true, SameSite = SameSiteMode.Lax };
        }

        public async Task UpdateJwtToken()
        {
            if (!_context.HttpContext.Request.Cookies.TryGetValue("RefreshToken", out string? RefreshToken))
                throw new UnauthorizedAccessException("Refresh Token was not found");

            var token = await _dbContext.Tokens.FirstOrDefaultAsync(t => t.refresh_token == HashingToken(RefreshToken)) ??
                throw new UnauthorizedAccessException("User was not found");

            if ((DateTime)token.expiry_date! < DateTime.UtcNow)
                throw new UnauthorizedAccessException("Refresh Token timed out");

            if (_context.HttpContext.Request.Cookies.ContainsKey("JwtToken"))
                _context.HttpContext.Response.Cookies.Delete("JwtToken");

            var user = await _dbContext.Users.FindAsync(token.user_id) ??
                throw new UnauthorizedAccessException("User was not found");

            var userModel = new UserModel { id = user.id, username = user.username, email = user.email, role = user.role };

            string NewJwtToken = GenerateJwtToken(userModel, 20);
            var JwtCookieOptions = SetCookieOptions(TimeSpan.FromMinutes(20));
            _context.HttpContext.Response.Cookies.Append("JwtToken", NewJwtToken, JwtCookieOptions);
        }

        public string HashingToken(string token)
        {
            using SHA512 sha = SHA512.Create();
            byte[] hashBytes = sha.ComputeHash(Encoding.UTF8.GetBytes(token));

            return Convert.ToBase64String(hashBytes);
        }

        public void DeleteTokens()
        {
            _context.HttpContext.Response.Cookies.Delete("RefreshToken");
            _context.HttpContext.Response.Cookies.Delete("JwtToken");
        }
    }
}
