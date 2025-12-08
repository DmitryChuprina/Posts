using Posts.Application.Core.Models;
using System.Security.Claims;

namespace Posts.Application.Core
{
    public interface IJwtTokenGenerator
    {
        public string Generate(List<Claim> claims, int? expiresMinutes);
        public string GenerateByUser(TokenUser user);
        public TokenUser ParseUserByClaims(ClaimsPrincipal claims);
    }
}
