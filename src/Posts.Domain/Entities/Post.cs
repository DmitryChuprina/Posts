using Posts.Domain.Entities.Base;

namespace Posts.Domain.Entities
{
    public class Post : BaseEntity, IAuditableEntity
    {
        public string? Content { get; set; }
        public string[] Tags { get; set; } = [];

        public Guid? ReplyForId { get; set; }
        public Guid? RepostId { get; set; }

        public int Depth { get; set; } = 0;
        public int LikesCount { get; set; } = 0;
        public int ViewsCount { get; set; } = 0;
        public int RepostsCount { get; set; } = 0;
        public int RepliesCount { get; set; } = 0;

        public Guid? CreatedBy { get; set; }
        public DateTime? CreatedAt { get; set; }
        public Guid? UpdatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
