using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using application.DTO.Inner;
using application.Abstractions.Services.Inner;

namespace application.Helpers
{
    public class TokenComparator : ITokenComparator
    {
        public string Key { get; set; }
        public string Issuer { get; set; }
        public string Audience { get; set; }

        public string CreateJWT(JwtDTO dto)
        {
            var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Key));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, dto.Username),
                new Claim(ClaimTypes.NameIdentifier, dto.UserId.ToString()),
                new Claim(ClaimTypes.Email, dto.Email),
                new Claim(ClaimTypes.Role, dto.Role)
            };

            var token = new JwtSecurityToken(
                issuer: Issuer,
                audience: Audience,
                claims: claims,
                expires: DateTime.UtcNow + dto.Expires,
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string CreateRefresh()
        {
            var builder = new StringBuilder();
            for (int i = 0; i < 3; i++)
                builder.Append(Guid.NewGuid().ToString());

            return builder.ToString();
        }
    }
}
