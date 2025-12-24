using Posts.Application.Repositories.Base;
using Posts.Application.Repositories.Models;
using Posts.Contract.Models;
using Posts.Domain.Entities;

namespace Posts.Application.Repositories
{
    public interface IPostsRepository : IBaseRepository<Post>
    {
        Task<IEnumerable<PostReadModel>> GetReadModelsByIdsAsync(IEnumerable<Guid> ids);
        Task<int> GetPostsByCreatorCountAsync(Guid creatorId, bool? withRepliesOrRepost = null);
        Task<IEnumerable<Post>> GetPostsByCreatorAsync(Guid creatorId, PaginationRequestDto pagination, bool? withRepliesOrRepost = null);
        Task<int> GetPostRepliesCountAsync(Guid replyForId);
        Task<IEnumerable<Post>> GetPostRepliesAsync(Guid replyForId, PaginationRequestDto pagination);

        Task IncrementLikesCountAsync(Guid postId);
        Task DecrementLikesCountAsync(Guid postId);
        Task IncrementViewsCountAsync(Guid postId);
        Task IncrementRepostsCountAsync(Guid postId);
        Task DecrementRepostsCountAsync(Guid postId);
        Task IncrementRepliesCountAsync(Guid postId);
        Task DecrementRepliesCountAsync(Guid postId);
    }
}
