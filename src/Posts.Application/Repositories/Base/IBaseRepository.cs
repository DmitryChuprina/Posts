using Posts.Domain.Entities.Base;

namespace Posts.Application.Repositories.Base
{
    public interface IBaseRepository<TEntity> 
        where TEntity : BaseEntity
    {
        Task<TEntity?> GetByIdAsync(Guid id);
        Task AddAsync(TEntity entity);
        Task AddManyAsync(IEnumerable<TEntity> entities);
        Task UpdateAsync(TEntity entity);
        Task DeleteAsync(Guid id);
        Task DeleteManyAsync(IEnumerable<Guid> ids);
    }
}
