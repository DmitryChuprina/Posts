namespace Posts.Contract.Models.Users
{
    public abstract class UserIsTakenRequestDto
    {
        public Guid? ForUserId { get; set; }
    }

    public class EmailIsTakenDto : UserIsTakenRequestDto
    {
        public required string Email { get; set; }
    }

    public class UsernameIsTakenDto : UserIsTakenRequestDto
    {
        public required string Username { get; set; }
    }
}
