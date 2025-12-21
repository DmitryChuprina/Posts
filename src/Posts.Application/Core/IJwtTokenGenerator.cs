using Posts.Application.Core.Models;
using System.Security.Claims;

namespace Posts.Application.Core
{
    public interface IJwtTokenGenerator
    {
        public JwtTokenGeneratorResult Generate(List<Claim> claims, int? expiresMinutes);
        public JwtTokenGeneratorResult GenerateByUser(TokenUser user);
        public TokenUser ParseUserByClaims(ClaimsPrincipal claims);
    }
}
