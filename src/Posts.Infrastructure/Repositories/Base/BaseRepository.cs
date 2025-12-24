using Dapper;
using Posts.Application.Core;
using Posts.Application.Exceptions;
using Posts.Application.Repositories.Base;
using Posts.Contract.Models;
using Posts.Domain.Entities.Base;
using Posts.Infrastructure.Repositories.Models;
using System.Reflection;
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
            new ColumnDefinition{ PropertyName = nameof(BaseEntity.Id), ColumnName = "id" },
            new ColumnDefinition{ PropertyName = nameof(BaseEntity.RowVersion), ColumnName = "row_version" },
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
                cols.Where(c => nameof(BaseEntity.Id) != c.PropertyName)
                    .Where(c => nameof(BaseEntity.RowVersion) != c.PropertyName)
                    .Where(c => !c.SkipOnUpdate)
                    .Select(c => $"\"{c.ColumnName}\" = @{c.PropertyName}")
            );

        public virtual Task<TEntity?> GetByIdAsync(Guid id)
        {
            var sql = $@"
            SELECT {_selectColumnsSql}
            FROM {TableName}
            WHERE id = @Id
            LIMIT 1;";

            return _connectionFactory.Use((conn, cancellation, tx) =>
                conn.QuerySingleOrDefaultAsync<TEntity>(
                    new CommandDefinition(
                        commandText: sql,
                        parameters: new { Id = id },
                        cancellationToken: cancellation,
                        transaction: tx
                    )
                )
            );
        }

        public virtual async Task AddAsync(TEntity entity)
        {
            NoramlizeEntityForAdd(entity);

            var sql = $@"
            INSERT INTO {TableName} ({_insertColumnsSql})
            VALUES ({_insertParamsSql});";

            await _connectionFactory.Use((conn, cancellation, tx) =>
                conn.ExecuteAsync(
                    new CommandDefinition(
                        commandText: sql,
                        parameters: entity,
                        cancellationToken: cancellation,
                        transaction: tx
                    )
                )
            );
        }
        public virtual async Task AddManyAsync(IEnumerable<TEntity> entities)
        {
            var list = entities.ToList();
            if (list.Count == 0)
            {
                return;
            }

            foreach (var entity in list)
            {
                NoramlizeEntityForAdd(entity);
            }

            var parameters = new DynamicParameters();
            var valuesList = new List<string>();

            var type = typeof(TEntity);
            // TODO: Need add _insertColumns/_updateColums props for a clear separation
            // Currently used _allColumns beacause _allColumns used for insert
            var propMap = _allColumns
                .Select(c => new
                    {
                        Def = c,
                        PropInfo = type.GetProperty(c.PropertyName)
                    })
                .ToArray();

            for (int i = 0; i < list.Count; i++)
            {
                var entity = list[i];
                var rowParams = new List<string>();

                foreach (var prop in propMap)
                {
                    if(prop.PropInfo is null)
                    {
                        continue;
                    }

                    var paramName = $"@{prop.Def.PropertyName}_{i}";
                    parameters.Add(paramName, prop.PropInfo.GetValue(entity));
                    rowParams.Add(paramName);
                }

                valuesList.Add($"({string.Join(", ", rowParams)})");
            }

            var sql = $@"
                INSERT INTO {TableName} ({_insertColumnsSql}) VALUES {string.Join(", ", valuesList)}
             ";

            await _connectionFactory.Use((conn, cancellation, tx) =>
                conn.ExecuteAsync(
                    new CommandDefinition(
                        commandText: sql,
                        parameters: parameters,
                        cancellationToken: cancellation,
                        transaction: tx
                    )
                )
            );
        }

        protected virtual void NoramlizeEntityForAdd(TEntity entity)
        {
            if (entity.Id == Guid.Empty)
            {
                entity.Id = Guid.NewGuid();
            }

            if (IsAuditable)
            {
                var auditable = (IAuditableEntity)entity;
                auditable.CreatedAt = DateTime.UtcNow;
                auditable.CreatedBy = _currentUser.UserId;
            }
        }

        public virtual async Task UpdateAsync(TEntity entity)
        {
            var sql = $@"
            UPDATE {TableName}
            SET {_updateAssignmentsSql},
                row_version = row_version + 1
            WHERE id = @Id AND row_version = @RowVersion;";

            if (IsAuditable)
            {
                var auditable = (IAuditableEntity)entity;
                auditable.UpdatedAt = DateTime.UtcNow;
                auditable.UpdatedBy = _currentUser.UserId;
            }

            var affected = await _connectionFactory.Use((conn, cancellation, tx) =>
                conn.ExecuteAsync(
                    new CommandDefinition(
                        commandText: sql,
                        parameters: entity,
                        cancellationToken: cancellation,
                        transaction: tx
                    )
                )
            );

            if (affected == 0)
            {
                throw new ConcurencyException("Data was modified by another request");
            }

            entity.RowVersion += 1;
        }

        public virtual async Task DeleteAsync(Guid id)
        {
            var sql = $@"DELETE FROM {TableName} WHERE id = @Id;";

            await _connectionFactory.Use((conn, cancellation, tx) =>
                conn.ExecuteAsync(
                    new CommandDefinition(
                        commandText: sql,
                        parameters: new { Id = id },
                        cancellationToken: cancellation,
                        transaction: tx
                    )
                )
            );
        }

        public virtual async Task DeleteManyAsync(IEnumerable<Guid> ids)
        {
            var sql = $@"DELETE FROM {TableName} WHERE id in @Ids;";

            await _connectionFactory.Use((conn, cancellation, tx) =>
                conn.ExecuteAsync(
                    new CommandDefinition(
                        commandText: sql,
                        parameters: new { Ids = ids },
                        cancellationToken: cancellation,
                        transaction: tx
                    )
                )
            );
        }
    }
}
