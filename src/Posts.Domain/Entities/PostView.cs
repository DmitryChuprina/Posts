using Posts.Domain.Entities.Base;

namespace Posts.Domain.Entities
{
    public class PostView : BaseEntity
    {
        public Guid PostId { get; set; }
        public Guid UserId { get; set; }

        public DateTime FirstViewedAt { get; set; }
        public DateTime LastViewedAt { get; set; }
    }
}
