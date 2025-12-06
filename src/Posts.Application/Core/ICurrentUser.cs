using Posts.Domain.Shared.Enums;
namespace Posts.Application.Core
{
    public interface ICurrentUser
    {
        public Guid? UserId { get; }
        public UserRole? UserRole { get; }
    }
}
