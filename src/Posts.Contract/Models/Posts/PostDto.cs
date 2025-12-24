namespace Posts.Contract.Models.Posts
{
    public class PostDto
    {
        public Guid Id { get; set; }
        public PostAuthorDto Author { get; set; } = new PostAuthorDto();
        public string? Content { get; set; }
        public PostDto? Repost { get; set;  } = null;
        public FileDto[] Media { get; set; } = [];

        public int LikesCount { get; set; } = 0;
        public int RepostsCount { get; set; } = 0;
        public int ViewsCount { get; set; } = 0;

        public int Depth { get; set; } = 0;
    }

    public class CreatePostDto
    {
        public string? Content { get; set; }
        public Guid? ReplyForId { get; set; }
        public Guid? RepostId { get; set; }
        public FileDto[] Media { get; set; } = [];
    }

    public class UpdatePostDto
    {
        public Guid Id { get; set; }
        public string? Content { get; set; }
        public FileDto[] Media { get; set; } = [];
    }

    public class DeletePostDto
    {
        public Guid Id { get; set; }
    }
}
