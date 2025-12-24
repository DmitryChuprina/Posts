namespace Posts.Contract.Models.Posts
{
    public class PostAuthorDto
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string? FirstName { get; set; } = string.Empty;
        public string? LastName { get; set; } = string.Empty;

        public FileDto? ProfileImage { get; set; } = null;
    }
}
