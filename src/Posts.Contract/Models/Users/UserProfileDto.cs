using Posts.Domain.Utils;

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
        private string? _firstName;
        private string? _lastName;
        private string _username = string.Empty;
        private string? _description;

        public string? FirstName
        {
            get { return _firstName; }
            set { _firstName = Formatting.NullableDefaultString(value); }
        }
        public string? LastName
        {
            get { return _lastName; }
            set { _lastName = Formatting.NullableDefaultString(value); }
        }
        public string Username
        {
            get { return _username; }
            set { _username = Formatting.Username(value); }
        }
        public string? Description 
        { 
            get { return _description; }
            set { _description = Formatting.NullableDefaultString(value); }
        }
        public FileDto? ProfileImage { get; set; }
        public FileDto? ProfileBanner { get; set; }
    }
}
