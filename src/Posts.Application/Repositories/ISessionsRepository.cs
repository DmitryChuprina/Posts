using Posts.Application.Repositories.Base;
using Posts.Domain.Entities;

namespace Posts.Application.Repositories
{
    public interface ISessionsRepository : IBaseRepository<Session>
    {
        public Task<Session?> GetByRefreshToken(string refreshToken);
    }
}
