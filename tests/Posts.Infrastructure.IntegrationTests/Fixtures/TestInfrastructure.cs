using Npgsql;
using Posts.Application.Core;
using Posts.Infrastructure.Core;
using Posts.Infrastructure.Interfaces;
using Respawn;
using Respawn.Graph;
using Testcontainers.Minio;
using Testcontainers.PostgreSql;
using IContainer = DotNet.Testcontainers.Containers.IContainer;

namespace Posts.Infrastructure.IntegrationTests.Fixtures
{
    public class TestInfrastructure : IAsyncLifetime
    {
        public PostgreSqlContainer Db { get; } = new PostgreSqlBuilder()
            .WithImage("pgvector/pgvector:pg17-trixie")
            .Build();

        public MinioContainer Minio { get; } = new MinioBuilder()
            .WithImage("minio/minio:latest")
            .WithPortBinding(9000, true)
            .WithEnvironment("MINIO_ROOT_USER", "minioadmin")
            .WithEnvironment("MINIO_ROOT_PASSWORD", "minioadmin")
            .WithEnvironment("MINIO_BROWSER", "off")
            .WithEnvironment("MINIO_API_CORS_ALLOW_ORIGIN", "*")
            .WithStartupCallback(async (c, ct) => {
                var bucketName = "integration-test-bucket";
                var setupCommand =
                        $"mc alias set myminio http://localhost:9000 minioadmin minioadmin && " +
                        $"mc mb myminio/{bucketName} && " +
                        $"mc anonymous set public myminio/{bucketName}";

                var result = await c.ExecAsync(new[] { "/bin/sh", "-c", setupCommand }, ct);

                if (result.ExitCode != 0)
                {
                    throw new Exception($"MinIO setup failed: {result.Stderr}");
                }
            })
            .Build();

        public ICancellation Cancelation { get; private set; } = null!;
        public IDbConnectionFactory ConnectionFactory { get; private set; } = null!;

        private Respawner _respawner = null!;

        private IEnumerable<IContainer> Containers => new []
        {
            (IContainer)Db,
            (IContainer)Minio
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
