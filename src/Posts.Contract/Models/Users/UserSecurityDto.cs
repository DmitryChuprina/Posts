using Posts.Domain.Utils;

namespace Posts.Contract.Models.Users
{
    public class UserSecurityDto
    {
        public string Email { get; set; } = string.Empty;
    }

    public class UpdateUserSecurityDto
    {
        private string _email = string.Empty;
        private string? _password = string.Empty;

        public string Email
        {
            get { return _email; }
            set { _email = Formatting.Email(value); }
        }
        public string? Password
        {
            get { return _password; }
            set { _password = Formatting.NullablePassword(value); }
        }
        public bool RevokeSessions { get; set; } = false;
    }
}
