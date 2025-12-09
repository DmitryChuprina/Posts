namespace Posts.Contract.Models.Auth
{
    public class SignUpRequestDto
    {
        public required string Email { get; set; }

        public required string Username { get; set; }

        public required string Password { get; set; }
    }
}
