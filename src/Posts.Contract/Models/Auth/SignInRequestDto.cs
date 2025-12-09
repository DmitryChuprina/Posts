namespace Posts.Contract.Models.Auth
{
    public class SignInRequestDto
    {
        public required string EmailOrUsername { get; set; }
        public required string Password { get; set; }
        public bool RememberMe { get; set; }
    }
}
