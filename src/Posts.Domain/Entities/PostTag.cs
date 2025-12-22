using Posts.Domain.Entities.Base;

namespace Posts.Domain.Entities
{
    public class PostTag : BaseEntity
    {
        public Guid PostId { get; set; }
        public Guid TagId { get; set; }
    }
}
