using Posts.Application.Core;

namespace Posts.Infrastructure.Core
{
    internal class BCryptPasswordHasher : IPasswordHasher
    {
        public string Hash(string rawPassword)
            => BCrypt.Net.BCrypt.HashPassword(rawPassword);

        public bool Verify(string rawPassword, string hash)
            => BCrypt.Net.BCrypt.Verify(rawPassword, hash);
    }
}
