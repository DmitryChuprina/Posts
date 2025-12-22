using Npgsql;
using Posts.Application.Core;
using System.Data;
using System.Data.Common;

namespace Posts.Infrastructure
{
    internal class DbConnectionTransactionEntry : IAsyncDisposable, IDisposable
    {
        private bool _disposed = false;

        public DbConnectionTransactionEntry(NpgsqlConnection connection, NpgsqlTransaction transaction)
        {
            Connection = connection;
            Transaction = transaction;
        }

        public NpgsqlConnection Connection { get; }
        public NpgsqlTransaction Transaction { get; }

        public async Task CommitAsync()
        {
            await Transaction.CommitAsync();
        }

        public async Task RollbackAsync()
        {
            await Transaction.RollbackAsync();
        }

        public async ValueTask DisposeAsync()
        {
            if (_disposed) return;

            await Transaction.DisposeAsync();
            await Connection.DisposeAsync();

            _disposed = true;
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            Connection.Dispose();
            Transaction.Dispose();

            _disposed = true;
        }
    }

    internal class DbConnectionFactory : IDisposable, IUnitOfWork
    {
        private readonly string _connectionString;
        private DbConnectionTransactionEntry? _transactionEntry;
        private readonly ICancellation _cancellation;

        public DbConnectionFactory(DbConnectionOptions connOpts, ICancellation cancellation)
        {
            if (string.IsNullOrWhiteSpace(connOpts.ConnectionString))
            {
                throw new ArgumentNullException(nameof(connOpts), "ConnectionString is empty");
            }

            _connectionString = connOpts.ConnectionString;
            _cancellation = cancellation;
        }

        public async Task BeginTransactionAsync()
        {
            if (_transactionEntry != null)
            {
                throw new InvalidOperationException("Transaction is already in progress");
            }

            var conn = await CreateOpenConnectionAsync();
            var transaction = await conn.BeginTransactionAsync(_cancellation.Token);
            _transactionEntry = new DbConnectionTransactionEntry(conn, transaction);
        }

        public async Task CommitAsync()
        {
            if (_transactionEntry == null)
            {
                throw new InvalidOperationException("No transaction to commit");
            }

            try
            {
                await _transactionEntry.CommitAsync();
            }
            catch
            {
                await _transactionEntry.RollbackAsync();
                throw;
            }
            finally
            {
                await _transactionEntry.DisposeAsync();
                _transactionEntry = null;
            }
        }

        public async Task RollbackAsync()
        {
            if (_transactionEntry == null)
            {
                return;
            }

            await _transactionEntry.RollbackAsync();
            await _transactionEntry.DisposeAsync();
            _transactionEntry = null;
        }

        public async Task<TRes?> Use<TRes>(Func<IDbConnection, CancellationToken, DbTransaction?, Task<TRes>> func)
        {
            if (_transactionEntry is not null)
            {
                return await func(
                    _transactionEntry.Connection,
                    _cancellation.Token,
                    _transactionEntry.Transaction
                );
            }

            using (var conn = await CreateOpenConnectionAsync())
            {
                return await func(conn, _cancellation.Token, null);
            }
        }

        public async Task Use(Func<IDbConnection, CancellationToken, DbTransaction?, Task> func)
        {
            if (_transactionEntry is not null)
            {
                await func(
                    _transactionEntry.Connection,
                    _cancellation.Token,
                    _transactionEntry.Transaction
                );
                return;
            }

            using (var conn = await CreateOpenConnectionAsync())
            {
                await func(conn, _cancellation.Token, null);
            }
        }

        public void Dispose()
        {
            _transactionEntry?.Dispose();
        }

        private async Task<NpgsqlConnection> CreateOpenConnectionAsync()
        {
            var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync(_cancellation.Token);
            return conn;
        }
    }
}
