using Posts.Domain.Entities.Base;

namespace Posts.Domain.Entities
{
    public class Tag : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public int UsageCount { get; set; } = 0;
        public DateTime LastUsedAt { get; set; }
    }
}
