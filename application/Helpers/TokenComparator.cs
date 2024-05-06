using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using application.DTO.Inner;
using application.Abstractions.Inner;
using Microsoft.Extensions.Configuration;

namespace application.Helpers
{
    public class TokenComparator(IConfiguration configuration) : ITokenComparator
    {
        public string CreateJWT(JwtDTO dto)
        {
            var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(configuration[App.SECRET_KEY]!));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, dto.Username),
                new Claim(ClaimTypes.NameIdentifier, dto.UserId.ToString()),
                new Claim(ClaimTypes.Email, dto.Email),
                new Claim(ClaimTypes.Role, dto.Role)
            };

            var token = new JwtSecurityToken(
                issuer: configuration[App.ISSUER]!,
                audience: configuration[App.AUDIENCE],
                claims: claims,
                expires: DateTime.UtcNow + dto.Expires,
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string CreateRefresh()
        {
            var builder = new StringBuilder();
            for (int i = 0; i < 5; i++)
                builder.Append(Guid.NewGuid().ToString());

            return builder.ToString();
        }
    }
}
