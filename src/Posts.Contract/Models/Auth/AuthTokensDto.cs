namespace Posts.Contract.Models.Auth
{
    public class AuthTokensDto
    {
        public required string AccessToken { get; set; }
        public required string RefreshToken { get; set; }
    }
}
