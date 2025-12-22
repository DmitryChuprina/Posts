using Posts.Domain.Utils;

namespace Posts.Contract.Models.Auth
{
    public class SignUpRequestDto
    {
        private string _email = string.Empty;
        private string _username = string.Empty;
        private string _password = string.Empty;

        public string Email
        {
            get { return _email; }
            set { _email = Formatting.Email(value); }
        }

        public string Username
        {
            get { return _username; }
            set { _username = Formatting.Username(value); }
        }

        public string Password
        {
            get { return _password; }
            set { _password = Formatting.Password(value); }
        }
    }
    public class SignUpResponseDto
    {
        public AuthUserDto User { get; set; } = new AuthUserDto();
    }
}
