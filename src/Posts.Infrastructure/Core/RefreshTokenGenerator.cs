using Posts.Application.Core;
using Posts.Application.Core.Models;

namespace Posts.Infrastructure.Core
{
    public class RefreshTokenGenerator : IRefreshTokenGenerator
    {
        public string Generate(TokenUser user)
        {
            var randomBytes = new byte[64];
            using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }
    }
}
