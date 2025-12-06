using Posts.Domain.Entities.Base;

namespace Posts.Application.Repositories.Base
{
    public interface IBaseRepository<TEntity> 
        where TEntity : BaseEntity
    {
        Task<TEntity?> GetById(Guid id);
        Task Add(TEntity entity);
        Task Update(TEntity entity);
        Task Delete(Guid id);
    }
}
