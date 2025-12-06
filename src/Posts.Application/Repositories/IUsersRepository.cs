using Posts.Application.Repositories.Base;
using Posts.Domain.Entities;

namespace Posts.Application.Repositories
{
    public interface IUsersRepository : IBaseRepository<User>
    {
        Task<User?> GetByUsername(string username);
        Task<User?> GetByEmail(string email);
    }
}
