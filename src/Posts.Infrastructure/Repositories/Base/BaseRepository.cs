using Dapper;
using Posts.Application.Core;
using Posts.Application.Repositories.Base;
using Posts.Domain.Entities.Base;
using Posts.Infrastructure.Repositories.Models;
using static Dapper.SqlMapper;

namespace Posts.Infrastructure.Repositories.Base
{
    internal abstract class BaseRepository<TEntity> : IBaseRepository<TEntity>
        where TEntity : BaseEntity
    {
        protected readonly DbConnectionFactory _connectionFactory;
        protected readonly ICurrentUser _currentUser;

        protected readonly ColumnDefinition[] BaseColumns =
        {
            new ColumnDefinition{ PropertyName = nameof(BaseEntity.Id), ColumnName = "id" }
        };

        protected readonly ColumnDefinition[] AuditColumns =
        {
            new ColumnDefinition{ PropertyName = nameof(IAuditableEntity.CreatedAt), ColumnName = "created_at" },
            new ColumnDefinition{ PropertyName = nameof(IAuditableEntity.CreatedBy), ColumnName = "created_by" },
            new ColumnDefinition{ PropertyName = nameof(IAuditableEntity.UpdatedAt), ColumnName = "updated_at" },
            new ColumnDefinition{ PropertyName = nameof(IAuditableEntity.UpdatedBy), ColumnName = "updated_by" },
        };

        protected abstract string TableName { get; }
        protected abstract ColumnDefinition[] Columns { get; }

        protected bool IsAuditable => typeof(IAuditableEntity).IsAssignableFrom(typeof(TEntity));

        protected readonly ColumnDefinition[] _allColumns;
        protected readonly string _selectColumnsSql;
        protected readonly string _insertColumnsSql;
        protected readonly string _insertParamsSql;
        protected readonly string _updateAssignmentsSql;

        public BaseRepository(DbConnectionFactory connectionFactory, ICurrentUser currentUser)
        {
            _connectionFactory = connectionFactory;
            _currentUser = currentUser;

            _allColumns = BuildAllColumnsOnce();
            _selectColumnsSql = BuildSelectColumnsOnce(_allColumns);
            _insertColumnsSql = BuildInsertColumnsOnce(_allColumns);
            _insertParamsSql = BuildInsertParamsOnce(_allColumns);
            _updateAssignmentsSql = BuildUpdateAssignmentsOnce(_allColumns);
        }

        private ColumnDefinition[] BuildAllColumnsOnce()
        {
            var result = BaseColumns.Concat(Columns);

            if (IsAuditable)
            {
                result = result.Concat(AuditColumns);
            }

            return result.ToArray();
        }

        private string BuildSelectColumnsOnce(ColumnDefinition[] cols)
            => string.Join(", ", cols.Select(c => $"\"{TableName}\".\"{c.ColumnName}\" as \"{c.PropertyName}\""));

        private string BuildInsertColumnsOnce(ColumnDefinition[] cols)
            => string.Join(", ", cols.Select(c => $"\"{c.ColumnName}\""));

        private string BuildInsertParamsOnce(ColumnDefinition[] cols)
            => string.Join(", ", cols.Select(c => "@" + c.PropertyName));

        private string BuildUpdateAssignmentsOnce(ColumnDefinition[] cols)
            => string.Join(", ",
                cols.Where(c => c.ColumnName != "id")
                    .Select(c => $"\"{c.ColumnName}\" = @{c.PropertyName}")
            );

        public virtual Task<TEntity?> GetById(Guid id)
        {
            var sql = $@"
            SELECT {_selectColumnsSql}
            FROM {TableName}
            WHERE id = @Id
            LIMIT 1;";

            return _connectionFactory.Use((conn, cancellation) =>
                conn.QuerySingleOrDefaultAsync<TEntity>(
                    new CommandDefinition(
                        commandText: sql,
                        parameters: new { Id = id },
                        cancellationToken: cancellation
                    )
                )
            );
        }

        public virtual async Task Add(TEntity entity)
        {
            if (entity.Id == Guid.Empty)
                entity.Id = Guid.NewGuid();

            if (IsAuditable)
            {
                var auditable = (IAuditableEntity)entity;
                auditable.CreatedAt = DateTime.UtcNow;
                auditable.CreatedBy = _currentUser.UserId;
            }

            var sql = $@"
            INSERT INTO {TableName} ({_insertColumnsSql})
            VALUES ({_insertParamsSql});";

            await _connectionFactory.Use((conn, cancellation) =>
                conn.ExecuteAsync(
                    new CommandDefinition(
                        commandText: sql,
                        parameters: entity,
                        cancellationToken: cancellation
                    )
                )
            );
        }

        public virtual async Task Update(TEntity entity)
        {
            var sql = $@"
            UPDATE {TableName}
            SET {_updateAssignmentsSql}
            WHERE id = @Id;";

            await _connectionFactory.Use((conn, cancellation) =>
                conn.ExecuteAsync(
                    new CommandDefinition(
                        commandText: sql,
                        parameters: entity,
                        cancellationToken: cancellation
                    )
                )
            );
        }

        public virtual async Task Delete(Guid id)
        {
            var sql = $@"DELETE FROM {TableName} WHERE id = @Id;";

            await _connectionFactory.Use((conn, cancellation) =>
                conn.ExecuteAsync(
                    new CommandDefinition(
                        commandText: sql,
                        parameters: new { Id = id },
                        cancellationToken: cancellation
                    )
                )
            );
        }
    }
}
