using Posts.Domain.Utils;

namespace Posts.Contract.Models.Users
{
    public abstract class UserIsTakenRequestDto
    {
        public Guid? ForUserId { get; set; }
    }

    public class EmailIsTakenDto : UserIsTakenRequestDto
    {
        private string _email = string.Empty;

        public string Email
        {
            get { return _email; }
            set { _email = Formatting.Email(value); }
        }
    }

    public class UsernameIsTakenDto : UserIsTakenRequestDto
    {
        private string _username = string.Empty;

        public string Username
        {
            get { return _username; }
            set { _username = Formatting.Username(value);  }
        }
    }
}
