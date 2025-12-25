using Dapper;
using FluentAssertions;
using Moq;
using Posts.Application.Core;
using Posts.Application.Exceptions;
using Posts.Application.Repositories.Base;
using Posts.Domain.Entities.Base;
using Posts.Infrastructure.IntegrationTests.Extensions;
using Posts.Infrastructure.IntegrationTests.Fixtures;
using Posts.Infrastructure.Repositories.Models;
using System.Reflection;

namespace Posts.Infrastructure.IntegrationTests.Repositories.Base
{
    [Collection("IntegrationTests")]
    public abstract class GenericBaseRepositoryTests<TRepository, TEntity> : IAsyncLifetime
        where TEntity : BaseEntity
        where TRepository : IBaseRepository<TEntity>
    {
        protected Type _repType;
        protected Type _entType = typeof(TEntity);

        protected readonly TestInfrastructure _infra;
        protected TRepository _repo;
        protected readonly Mock<ICurrentUser> _userMock = new();

        protected GenericBaseRepositoryTests(TestInfrastructure infra)
        {
            _infra = infra;
            _repo = CreateRepository();
            _repType = _repo.GetType();
        }

        private string GetTableName()
        {;
            var tableNameProp = _repType.GetProperty("TableName", BindingFlags.Instance | BindingFlags.NonPublic);
            if(tableNameProp is null)
            {
                throw new InvalidOperationException($"TableName property not found in {_repType.Name}");
            }
            var tableName = tableNameProp.GetValue(_repo);
            return tableName!.ToString()!;
        }

        private IEnumerable<ColumnDefinition> GetAllColumns()
        {
            var allColumnsField = _repType.GetField("_allColumns", BindingFlags.Instance | BindingFlags.NonPublic);
            if (allColumnsField is null)
            {
                throw new InvalidOperationException($"Field '_allColumns' not found in {_repType.Name}");
            }

            var allColumns = (Array)allColumnsField.GetValue(_repo)!;

            return allColumns.Cast<ColumnDefinition>();
        }

        public virtual async Task InitializeAsync()
        {
            _repo = CreateRepository();
        }

        public virtual async Task DisposeAsync()
        {
            await _infra.ResetDatabaseAsync();
        }

        protected abstract TRepository CreateRepository();
        protected abstract Task<TEntity> CreateSampleEntityAsync();
        protected abstract Task MakeChangesForUpdateAsync(TEntity entity);

        [Fact]
        public virtual async Task GetById_Should_Return_Null_If_Not_Found()
        {
            // Act
            var entity = await _repo.GetByIdAsync(Guid.NewGuid());
            // Assert
            entity.Should().BeNull();
        }

        [Fact]
        public virtual async Task Add_Should_Create_Record()
        {
            // Arrange
            var entity = await CreateSampleEntityAsync();

            // Act
            await _repo.AddAsync(entity);

            // Assert
            entity.Id.Should().NotBeEmpty();
            var fromDb = await _repo.GetByIdAsync(entity.Id);
            fromDb.Should().NotBeNull();
            fromDb.Should().BeEquivalentTo(entity, opts => opts.WithTimeTolerance());
        }

        [Fact]
        public virtual async Task Update_Should_Handle_Concurrency()
        {
            // Arrange
            var entity = await CreateSampleEntityAsync();
            await _repo.AddAsync(entity);

            await _infra.ConnectionFactory.Use(async (conn, ct, tx) =>
            {
                await conn.ExecuteAsync(
                   $"UPDATE {GetTableName()} SET row_version = row_version + 1 WHERE id = @Id",
                   new { entity.Id });
            });

            // Act & Assert
            Func<Task> act = async () => await _repo.UpdateAsync(entity);
            await act.Should().ThrowAsync<ConcurencyException>();
        }

        [Fact]
        public virtual async Task Update_Should_Modify_Data_And_Increment_RowVersion()
        {
            // Arrange
            var entity = await CreateSampleEntityAsync();
            await _repo.AddAsync(entity);
            var originalRowVersion = entity.RowVersion;

            await MakeChangesForUpdateAsync(entity);

            // Act
            await _repo.UpdateAsync(entity);

            // Assert
            var fromDb = await _repo.GetByIdAsync(entity.Id);

            var allColumns = GetAllColumns();
            var skipOnUpdateProps = allColumns
                .Where(c => c.SkipOnUpdate)
                .Select(c => c.PropertyName)
                .ToHashSet();

            fromDb.Should().NotBeNull();
            fromDb.Should().BeEquivalentTo(entity, opts =>
            {
                foreach(var col in skipOnUpdateProps)
                {
                    opts = opts.ExcludingMembersNamed(col);
                }
                return opts.WithTimeTolerance();
            }); 
            fromDb!.RowVersion.Should().BeGreaterThan(originalRowVersion);
        }

        [Fact]
        public virtual async Task Delete_Should_Remove_Record()
        {
            // Arrange
            var entity = await CreateSampleEntityAsync();
            await _repo.AddAsync(entity);

            // Act
            await _repo.DeleteAsync(entity.Id);

            // Assert
            var fromDb = await _repo.GetByIdAsync(entity.Id);
            fromDb.Should().BeNull();
        }

        [Fact]
        public virtual async Task AddMany_Should_Insert_All_Records()
        {
            // Arrange
            var entities = new[]
            {
                await CreateSampleEntityAsync(),
                await CreateSampleEntityAsync(),
                await CreateSampleEntityAsync()
            };

            // Act
            await _repo.AddManyAsync(entities);

            // Assert
            foreach (var entity in entities)
            {
                entity.Id.Should().NotBeEmpty(); 

                var fromDb = await _repo.GetByIdAsync(entity.Id);
                fromDb.Should().NotBeNull();
                fromDb.Should().BeEquivalentTo(entity, opts => opts.WithTimeTolerance());
            }
        }

        [Fact]
        public virtual async Task DeleteMany_Should_Remove_Specific_Records()
        {
            // Arrange
            var entityKeep = await CreateSampleEntityAsync();
            var entityDel1 = await CreateSampleEntityAsync();
            var entityDel2 = await CreateSampleEntityAsync();

            await _repo.AddManyAsync(new[] { entityKeep, entityDel1, entityDel2 });

            // Act
            await _repo.DeleteManyAsync(new[] { entityDel1.Id, entityDel2.Id });

            // Assert
            (await _repo.GetByIdAsync(entityKeep.Id)).Should().NotBeNull();
            (await _repo.GetByIdAsync(entityDel1.Id)).Should().BeNull();
            (await _repo.GetByIdAsync(entityDel2.Id)).Should().BeNull();
        }

        [Fact]
        public virtual async Task Add_Should_Set_Audit_Fields_Created()
        {
            if (!typeof(IAuditableEntity).IsAssignableFrom(typeof(TEntity)))
            {
                return;
            }

            var userId = Guid.NewGuid();
            _userMock.Setup(u => u.UserId).Returns(userId);

            // Arrange
            var entity = await CreateSampleEntityAsync();

            // Act
            await _repo.AddAsync(entity);

            // Assert
            var fromDb = await _repo.GetByIdAsync(entity.Id);
            var auditable = (IAuditableEntity)fromDb!;

            auditable.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
            auditable.CreatedBy.Should().Be(userId);
        }

        [Fact]
        public virtual async Task Update_Should_Set_Audit_Fields_Updated()
        {
            if (!typeof(IAuditableEntity).IsAssignableFrom(typeof(TEntity)))
            {
                return;
            }

            var userId = Guid.NewGuid();
            _userMock.Setup(u => u.UserId).Returns(userId);

            // Arrange
            var entity = await CreateSampleEntityAsync();
            await _repo.AddAsync(entity);
            await MakeChangesForUpdateAsync(entity);

            await Task.Delay(100);

            // Act
            await _repo.UpdateAsync(entity);

            // Assert
            var fromDb = await _repo.GetByIdAsync(entity.Id);
            var auditable = (IAuditableEntity)fromDb!;

            auditable.UpdatedAt.Should().NotBeNull();
            auditable.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
            auditable.UpdatedAt.Should().BeAfter((DateTime)auditable.CreatedAt!);
            auditable.UpdatedBy.Should().NotBeNull();
        }

        [Fact]
        public void BuildUpdateAssignments_Should_Exclude_SkipOnUpdate_Columns_Reflection()
        {
            var sqlField = _repType.GetField("_updateAssignmentsSql", BindingFlags.Instance | BindingFlags.NonPublic);
            if (sqlField == null)
            {
                throw new InvalidOperationException($"Field '_updateAssignmentsSql' not found in {_repType.Name}");
            }

            var generatedSql = (string)sqlField.GetValue(_repo)!;
            var allColumns = GetAllColumns();

            // Act & Assert
            foreach (ColumnDefinition col in allColumns)
            {
                string columnName = col.ColumnName;
                string propertyName = col.PropertyName;
                bool skipOnUpdate = col.SkipOnUpdate;

                if (propertyName == "Id" || propertyName == "RowVersion")
                {
                    generatedSql.Should().NotContain($"\"{columnName}\" =",
                        $"System column '{propertyName}' should not be in UPDATE SET clause");
                    continue;
                }

                if (skipOnUpdate)
                {
                    generatedSql.Should().NotContain($"\"{columnName}\"",
                        $"Column '{columnName}' (Prop: {propertyName}) is marked SkipOnUpdate but was found in SQL");
                    continue;
                }

                generatedSql.Should().Contain($"\"{columnName}\" = @{propertyName}",
                        $"Standard column '{columnName}' should be present in SQL");
            }

        }

        [Fact]
        public virtual async Task Update_Should_Not_Change_SkipOnUpdate_Columns_Integration()
        {
            // Arrange
            var entity = await CreateSampleEntityAsync();
            await _repo.AddAsync(entity);

            var originalValues = new Dictionary<string, object?>();
            var allColumns = GetAllColumns();
            bool hasSkippedColumns = false;

            foreach (ColumnDefinition col in allColumns)
            {
                if (col.SkipOnUpdate == true)
                {
                    hasSkippedColumns = true;
                    string propName = col.PropertyName;

                    var propInfo = _entType.GetProperty(propName);
                    if (propInfo == null)
                    {
                        continue;
                    }

                    var originalValue = propInfo.GetValue(entity);
                    originalValues[propName] = originalValue;

                    var newValue = GenerateNewValue(propInfo.PropertyType, originalValue);
                    if(newValue is null)
                    {
                        throw new InvalidOperationException(
                            $"Test setup error: Cannot generate new value for property '{propName}' of type '{propInfo.PropertyType.Name}'"
                        );
                    }
                    propInfo.SetValue(entity, newValue);
                }
            }

            if (!hasSkippedColumns)
            {
                return;
            }

            // Act
            await _repo.UpdateAsync(entity);

            // Assert
            var fromDb = await _repo.GetByIdAsync(entity.Id);
            fromDb.Should().NotBeNull();

            foreach (var kvp in originalValues)
            {
                var propName = kvp.Key;
                var originalValue = kvp.Value;

                var propInfo = _entType.GetProperty(propName);
                var dbValue = propInfo!.GetValue(fromDb);

                dbValue.Should().BeEquivalentTo(
                    originalValue,
                    config => config.WithTimeTolerance(),
                    $"Column '{propName}' is SkipOnUpdate, so DB value should not change even if entity property was modified."
                );

                var currentInMemoryValue = propInfo.GetValue(entity);
                if (originalValue != null)
                {
                    dbValue.Should().NotBeEquivalentTo(currentInMemoryValue,
                       "Test setup error: checking logic requires in-memory value to be different from DB value");
                }
            }
        }

        private object? GenerateNewValue(Type type, object? oldValue)
        {
            if (type == typeof(string)) return $"changed_{Guid.NewGuid()}";
            if (type == typeof(Guid)) return Guid.NewGuid();
            if (type == typeof(int)) return ((int)(oldValue ?? 0)) + 1;
            if (type == typeof(long)) return ((long)(oldValue ?? 0)) + 1;
            if (type == typeof(bool)) return !((bool)(oldValue ?? false));
            if (type == typeof(DateTime) || type == typeof(DateTime?)) 
                return DateTime.UtcNow.AddYears(1);

            return null;
        }
    }
}
