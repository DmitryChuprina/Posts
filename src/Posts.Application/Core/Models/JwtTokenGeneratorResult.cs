namespace Posts.Application.Core.Models
{
    public class JwtTokenGeneratorResult
    {
        public DateTime ExpiresAt { get; set; }
        public string Token { get; set; } = string.Empty;
    }
}
