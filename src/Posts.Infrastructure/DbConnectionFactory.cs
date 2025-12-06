using Npgsql;
using Posts.Application.Core;
using System.Data;

namespace Posts.Infrastructure
{
    internal class DbConnectionFactory
    {
        private readonly string _connectionString;
        private readonly ICancellation _cancellation;

        public DbConnectionFactory(string connectionString, ICancellation cancellation) {
            _connectionString = connectionString;
            _cancellation = cancellation;
        }

        public async Task<TRes?> Use<TRes>(Func<IDbConnection, CancellationToken, Task<TRes>> func)
        {
            using(var conn = new NpgsqlConnection(_connectionString))
            {
                await conn.OpenAsync(_cancellation.Token);
                var res = await func(conn, _cancellation.Token);
                return res;
            }
        }

        public async Task Use(Func<IDbConnection, CancellationToken, Task> func)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                await conn.OpenAsync(_cancellation.Token);
                await func(conn, _cancellation.Token);
            }
        }

        public async Task UseTransaction(Func<IDbConnection, IDbTransaction, CancellationToken, Task> func)
        {
            using(var conn = new NpgsqlConnection(_connectionString))
            {
                await conn.OpenAsync(_cancellation.Token);
                using(var transaction = await conn.BeginTransactionAsync(_cancellation.Token))
                {
                    await func(conn, transaction, _cancellation.Token);
                    await transaction.CommitAsync(_cancellation.Token);
                }
            }
        }
    }
}
