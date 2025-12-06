namespace Posts.Infrastructure.Core.Models
{
    public class JwtOptions
    {
        public const string SectionName = "Jwt";

        public string Issuer { get; init; } = null!;
        public string Audience { get; init; } = null!;
        public string Key { get; init; } = null!;
        public int ExpiresMinutes { get; init; }
    }
}
