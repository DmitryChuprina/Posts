using Posts.Domain.Utils;

namespace Posts.Contract.Models.Auth
{
    public class SignInRequestDto
    {
        public string _emailOrUsername = string.Empty;
        public string _password = string.Empty;

        public string EmailOrUsername 
        { 
            get { return _emailOrUsername; }
            set { _emailOrUsername = Formatting.EmailOrUsername(value); }
        }
        public string Password
        {
            get { return _password; }
            set { _password = Formatting.Password(value); }
        }
        public bool RememberMe { get; set; }
    }
    public class SignInResponseDto
    {
        public AuthUserDto User { get; set; } = new AuthUserDto();
        public AuthTokensDto Tokens { get; set; } = new AuthTokensDto();
    }
}
