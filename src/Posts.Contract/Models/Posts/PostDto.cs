using Posts.Domain.Utils;

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
        public string? _content { get; set; }

        public string? Content
        {
            get { return _content; }
            set { _content = Formatting.NullableDefaultString(value); }
        }
        public Guid? ReplyForId { get; set; }
        public Guid? RepostId { get; set; }
        public FileDto[] Media { get; set; } = [];
    }

    public class UpdatePostDto
    {
        public string? _content { get; set; }

        public Guid Id { get; set; }
        public string? Content
        {
            get { return _content; }
            set { _content = Formatting.NullableDefaultString(value); }
        }
        public FileDto[] Media { get; set; } = [];
    }
}
