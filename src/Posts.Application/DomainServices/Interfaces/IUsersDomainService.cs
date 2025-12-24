namespace Posts.Application.DomainServices.Interfaces
{
    public interface IUsersDomainService
    {
        Task<bool> EmailIsTaken(string email, Guid? forUserId = null);
        Task<bool> UsernameIsTaken(string username, Guid? forUserId = null);
        Task ValidateEmailIsTaken(string email, Guid? forUserId = null);
        Task ValidateUsernameIsTaken(string username, Guid? forUserId = null);
    }
}
