namespace Posts.Contract.Models.Auth
{
    public class AuthUserDto
    {
        public Guid Id { get; set; }
        public required string Username { get; set; }
        public required string Email { get; set; }

        public string? FirstName { get; set; }
        public string? LastName { get; set; }

        public bool IsEmailConfirmed { get; set; }

        public string? Description { get; set; }
        public string? ProfileImageUrl { get; set; }
    }
}
