using System.Data;
using System.Data.Common;

namespace Posts.Infrastructure.Interfaces
{
    public interface IDbConnectionFactory
    {
        Task<TRes> Use<TRes>(Func<IDbConnection, CancellationToken, DbTransaction?, Task<TRes>> func);
        Task Use(Func<IDbConnection, CancellationToken, DbTransaction?, Task> func);
    }
}
