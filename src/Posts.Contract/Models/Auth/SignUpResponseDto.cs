namespace Posts.Contract.Models.Auth
{
    public class SignUpResponseDto
    {
        public AuthUserDto User { get; set; } = new AuthUserDto();
    }
}
