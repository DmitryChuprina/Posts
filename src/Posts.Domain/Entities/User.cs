using Posts.Domain.Entities.Base;
using Posts.Domain.Shared.Enums;

namespace Posts.Domain.Entities
{
    public class User : BaseEntity, IAuditableEntity
    {
        public required string Username { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; }

        public required UserRole Role { get; set; }

        public string? FirstName { get; set; }
        public string? LastName { get; set; }

        public string? Description { get; set; }
        public string? ProfileImageKey { get; set; }
        public string? ProfileBannerKey { get; set; }

        public bool EmailIsConfirmed { get; set; }

        public DateTime? BlockedAt { get; set; }
        public string? BlockReason { get; set; }

        public Guid? CreatedBy { get; set; }
        public DateTime? CreatedAt { get; set; }
        public Guid? UpdatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
