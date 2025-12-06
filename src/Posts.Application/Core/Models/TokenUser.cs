using Posts.Domain.Shared.Enums;

namespace Posts.Application.Core.Models
{
    public class TokenUser
    {
        public Guid Id { get; set; }
        public UserRole Role { get; set; }
    }
}
