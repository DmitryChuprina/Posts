namespace Posts.Contract.Models.Auth
{
    public class AuthUserDto
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        public string? FirstName { get; set; }
        public string? LastName { get; set; }

        public bool IsEmailConfirmed { get; set; }

        public string? Description { get; set; }
        public string? ProfileImageUrl { get; set; }
    }
}
