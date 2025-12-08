using Microsoft.IdentityModel.Tokens;
using Posts.Application.Core;
using Posts.Application.Core.Models;
using Posts.Infrastructure.Core.Models;
using Posts.Domain.Shared.Enums;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Posts.Infrastructure.Core
{
    internal class JwtTokenGenerator : IJwtTokenGenerator
    {
        private readonly JwtOptions _options;
        private readonly byte[] _keyBytes;

        public JwtTokenGenerator(JwtOptions options)
        {
            _options = options;
            _keyBytes = Encoding.UTF8.GetBytes(_options.Key);
        }

        public string Generate(List<Claim> claims, int? expiresMinutes = null)
        {
            expiresMinutes = expiresMinutes ?? _options.ExpiresMinutes;

            var creds = new SigningCredentials(
                new SymmetricSecurityKey(_keyBytes),
                SecurityAlgorithms.HmacSha256
            );

            var token = new JwtSecurityToken(
                issuer: _options.Issuer,
                audience: _options.Audience,
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: DateTime.UtcNow.AddMinutes((int)expiresMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string GenerateByUser(TokenUser user)
        {
            var userId = user.Id;
            var userRole = user.Role;

            var claims = new List<Claim>
            {
                new("uid", userId.ToString()),
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new Claim(ClaimTypes.Role, userRole.ToString()),
            };
            return Generate(claims);
        }

        public TokenUser ParseUserByClaims(ClaimsPrincipal claims)
        {
            var userIdClaim = claims.FindFirst("uid") ?? claims.FindFirst(JwtRegisteredClaimNames.Sub);
            var userRoleClaim = claims.FindFirst(ClaimTypes.Role);
            if (userIdClaim == null)
            {
                throw new SecurityTokenException("Invalid token: User ID claim not found.");
            }
            if (userRoleClaim == null)
            {
                throw new SecurityTokenException("Invalid token: User Role claim not found.");
            }
            return new TokenUser
            {
                Id = Guid.Parse(userIdClaim.Value),
                Role = Enum.Parse<UserRole>(userRoleClaim.Value)
            };
        }
    }
}
