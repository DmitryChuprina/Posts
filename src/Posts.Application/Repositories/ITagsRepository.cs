using Posts.Application.Repositories.Base;
using Posts.Domain.Entities;

namespace Posts.Application.Repositories
{
    public interface ITagsRepository : IBaseRepository<Tag>
    {
        Task UpsertTagsStatsAsync(string[] tags);
        Task DecrementTagsUsageAsync(string[] tags);
    }
}
