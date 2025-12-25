using Dapper;
using Npgsql;
using Posts.Application.Core;
using Posts.Application.Repositories.Base;
using Posts.Domain.Entities.Base;
using Posts.Infrastructure.IntegrationTests.Fixtures;
using Posts.Infrastructure.Interfaces;
using Posts.Infrastructure.Repositories.Base;
using Posts.Infrastructure.Repositories.Models;

namespace Posts.Infrastructure.IntegrationTests.Repositories.Base
{
    public class TestEntity : BaseEntity, IAuditableEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime? CreatedAt { get; set; }
        public Guid? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid? UpdatedBy { get; set; }
    }

    public interface ITestRepository : IBaseRepository<TestEntity>
    {
    }

    internal class TestRepository : BaseRepository<TestEntity>, ITestRepository
    {
        protected override string TableName => "test_entity";
        protected override ColumnDefinition[] Columns => new[]
        {
            new ColumnDefinition { PropertyName = nameof(TestEntity.Name), ColumnName = "name" },
            new ColumnDefinition { PropertyName = nameof(TestEntity.Description), ColumnName = "description", SkipOnUpdate = true },
        };

        public TestRepository(IDbConnectionFactory factory, ICurrentUser user) : base(factory, user) { }
    }

    [Collection("IntegrationTests")]
    public class BaseRepositoryTests: GenericBaseRepositoryTests<ITestRepository, TestEntity>, IAsyncLifetime
    {
        public BaseRepositoryTests(TestInfrastructure infra) : base(infra)
        {
        }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();
            using (var connection = new NpgsqlConnection(_infra.Db.GetConnectionString()))
            {
                await connection.ExecuteAsync($@"
                    CREATE TABLE IF NOT EXISTS test_entity (
                        id UUID PRIMARY KEY,
                        name TEXT NOT NULL,
                        description TEXT,
                        row_version integer DEFAULT 0 NOT NULL,
                        created_at TIMESTAMPTZ,
                        created_by UUID,
                        updated_at TIMESTAMPTZ,
                        updated_by UUID
                    ); 
                ");
            }
        }

        public override async Task DisposeAsync()
        {
            await base.DisposeAsync();
        }

        protected override ITestRepository CreateRepository()
        {
            var rep = new TestRepository(_infra.ConnectionFactory, _userMock.Object) as ITestRepository;
            return rep;
        }

        protected override async Task<TestEntity> CreateSampleEntityAsync()
        {
            return new TestEntity
            {
                Name = $"Test {Guid.NewGuid()}",
                Description = "Test Description"
            };
        }

        protected override async Task MakeChangesForUpdateAsync(TestEntity entity)
        {
            entity.Name = $"Updated Name {Guid.NewGuid()}";
            entity.Description = $"Updated Description {Guid.NewGuid()}";
        }

    }
}
