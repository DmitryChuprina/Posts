namespace Posts.Contract.Models.Auth
{
    public class SignInResponseDto
    {
        public AuthUserDto User { get; set; } = new AuthUserDto();
        public AuthTokensDto Tokens { get; set; } = new AuthTokensDto();
    }
}
