namespace Posts.Contract.Models.Users
{
    public class UserSecurityDto
    {
        public string Email { get; set; } = string.Empty;
    }

    public class UpdateUserSecurityDto
    {
        public string Email { get; set; } = string.Empty;
        public string? Password { get; set; }
        public bool RevokeSessions { get; set; } = false;
    }
}
