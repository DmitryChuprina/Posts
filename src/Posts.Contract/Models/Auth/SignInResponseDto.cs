namespace Posts.Contract.Models.Auth
{
    public class SignInResponseDto
    {
        public required AuthUserDto User { get; set; }
        public required AuthTokensDto Tokens { get; set; }
    }
}
