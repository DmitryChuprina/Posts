using Posts.Application.Repositories.Base;
using Posts.Domain.Entities;

namespace Posts.Application.Repositories
{
    public interface IUsersRepository : IBaseRepository<User>
    {
        Task<User?> GetByUsernameAsync(string username);
        Task<User?> GetByEmailAsync(string email);
    }
}
