using Posts.Application.Repositories.Base;
using Posts.Domain.Entities;

namespace Posts.Application.Repositories
{
    public interface IPostsRepository : IBaseRepository<Post>
    {
    }
}
