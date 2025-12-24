namespace Posts.Application.Repositories.Models
{
    public class PostReadModel
    {
        public Guid Id { get; set; }

        public string? Content { get; set; }
        public string[] Tags { get; set; } = [];

        public Guid? ReplyForId { get; set; }
        public Guid? RepostId { get; set; }

        public int Depth { get; set; } = 0;
        public int LikesCount { get; set; } = 0;
        public int ViewsCount { get; set; } = 0;
        public int RepliesCount { get; set; } = 0;
        public int RepostsCount { get; set; } = 0;

        public Guid? MediaId { get; set; }
        public string? MediaKey { get; set; }
        public int? MediaOrder { get; set; }

        public Guid CreatorId { get; set; }
        public string CreatorUsername { get; set; } = string.Empty;
        public string? CreatorFirstName { get; set; }
        public string? CreatorLastName { get; set; }
        public string? CreatorProfileImageKey { get; set; }
    }
}
