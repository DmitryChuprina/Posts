using Posts.Domain.Entities.Base;

namespace Posts.Domain.Entities
{
    public class PostLike : BaseEntity
    {
        public Guid PostId { get; set; }
        public Guid UserId { get; set; }
        public DateTime LikedAt { get; set; }
    }
}
