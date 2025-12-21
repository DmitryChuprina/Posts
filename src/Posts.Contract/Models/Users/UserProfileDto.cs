namespace Posts.Contract.Models.Users
{
    public class UserProfileDto
    {
        public Guid Id { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string Username { get; set; } = string.Empty;
        public string? Description { get; set; }
        public FileDto? ProfileImage { get; set; }
        public FileDto? ProfileBanner { get; set; }
    }
    public class UpdateUserProfileDto
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string Username { get; set; } = string.Empty;
        public string? Description { get; set; }
        public FileDto? ProfileImage { get; set; }
        public FileDto? ProfileBanner { get; set; }
    }
}
