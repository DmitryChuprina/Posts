using DotNet.Testcontainers.Containers;
using Npgsql;
using Posts.Application.Core;
using Posts.Infrastructure.Core;
using Posts.Infrastructure.Interfaces;
using Testcontainers.PostgreSql;
using Respawn;
using Respawn.Graph;

namespace Posts.Infrastructure.IntegrationTests.Fixtures
{
    public class TestInfrastructure : IAsyncLifetime
    {
        public PostgreSqlContainer Db { get; } = new PostgreSqlBuilder()
            .WithImage("pgvector/pgvector:pg17-trixie")
            .Build();

        public ICancellation Cancelation { get; private set; } = null!;
        public IDbConnectionFactory ConnectionFactory { get; private set; } = null!;

        private Respawner _respawner = null!;

        private IEnumerable<IContainer> Containers => new[]
        {
            Db
        };

        public async Task InitializeAsync()
        {
            await Task.WhenAll(
                Containers
                    .Select(c => c.StartAsync())
            );

            await Migrations.Migrations.RunAsync(Db.GetConnectionString());

            Cancelation = new DefaultCancellation();
            ConnectionFactory = new DbConnectionFactory(
                new DbConnectionOptions { ConnectionString = Db.GetConnectionString() },
                Cancelation
            );

            await InitRespawner();
        }

        public async Task DisposeAsync()
        {
            await Task.WhenAll(
                Containers
                    .Select(c => c.DisposeAsync().AsTask())
            );
            if(ConnectionFactory is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }

        private async Task InitRespawner()
        {
            using var connection = new NpgsqlConnection(Db.GetConnectionString());
            await connection.OpenAsync();

            _respawner = await Respawner.CreateAsync(connection, new RespawnerOptions
            {
                DbAdapter = DbAdapter.Postgres,
                SchemasToInclude = new[] { "public" },
                                                     
                TablesToIgnore = new Table[]
                {
                    "migrations",
                    "schema_versions"
                }
            });
        }

        public async Task ResetDatabaseAsync()
        {
            using var connection = new NpgsqlConnection(Db.GetConnectionString());
            await connection.OpenAsync();
            await _respawner.ResetAsync(connection);
        }
    }
}
