using Posts.Application.Core;
using System.Security.Cryptography;
using System.Text;

namespace Posts.Infrastructure.Core
{
    internal class TokenHasher : ITokenHasher
    {
        public string Hash(string rawToken)
        {
            if (string.IsNullOrWhiteSpace(rawToken))
            {
                throw new ArgumentException("Token cannot be empty.", nameof(rawToken));
            }

            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(rawToken);
            var hashBytes = sha.ComputeHash(bytes);
            return Convert.ToBase64String(hashBytes);
        }
    }
}
