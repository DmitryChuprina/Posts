using Dapper;
using Npgsql;
using System.Reflection;

namespace Posts.Migrations
{
    public class Migrator
    {
        private readonly string _connectionString;

        public Migrator(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task MigrateAsync()
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            await EnsureMigrationsTable(conn);

            var assembly = Assembly.GetExecutingAssembly();
            var resources = assembly
                .GetManifestResourceNames()
                .Where(r => r.EndsWith(".sql"))
                .OrderBy(r => r)
                .ToArray();

            foreach (var resource in resources)
            {
                var name = string.Join('.', resource.Split('.').TakeLast(2).Take(1));

                if (await IsAlreadyApplied(conn, name))
                {
                    Console.WriteLine($"Skipping: {name}");
                    continue;
                }

                Console.WriteLine($"Applying: {name}");

                using var stream = assembly.GetManifestResourceStream(resource);
                using var reader = new StreamReader(stream!);

                var sql = await reader.ReadToEndAsync();

                await using var tx = await conn.BeginTransactionAsync();

                try
                {
                    await conn.ExecuteAsync(sql, transaction: tx);

                    await conn.ExecuteAsync(
                        "INSERT INTO migrations (name) VALUES (@name)",
                        new { name },
                        transaction: tx
                    );

                    await tx.CommitAsync();
                }
                catch
                {
                    await tx.RollbackAsync();
                    Console.WriteLine($"Migration failed: {name}");
                    throw;
                }
            }
        }

        private async Task EnsureMigrationsTable(NpgsqlConnection conn)
        {
            await conn.ExecuteAsync("""
                 CREATE TABLE IF NOT EXISTS migrations (
                     name TEXT PRIMARY KEY,
                     applied_at TIMESTAMP NOT NULL DEFAULT NOW()
                 );
             """);
        }

        private async Task<bool> IsAlreadyApplied(NpgsqlConnection conn, string name)
        {
            return await conn.ExecuteScalarAsync<bool>(
                "SELECT EXISTS (SELECT 1 FROM migrations WHERE name = @name)",
                new { name }
            );
        }
    }
}
