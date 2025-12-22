using Posts.Domain.Entities.Base;

namespace Posts.Domain.Entities
{
    public class PostMedia : BaseEntity, IAuditableEntity
    {
        public Guid PostId { get; set; }
        public string Key { get; set; } = string.Empty;
        public int Order { get; set; } = 0;
        public Guid? CreatedBy { get; set; }
        public DateTime? CreatedAt { get; set; }
        public Guid? UpdatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
