using Dapper;
using Posts.Application.Core;
using Posts.Application.Repositories;
using Posts.Domain.Entities;
using Posts.Infrastructure.Repositories.Base;
using Posts.Infrastructure.Repositories.Models;

namespace Posts.Infrastructure.Repositories
{
    internal class SessionsRepository : BaseRepository<Session>, ISessionsRepository
    {
        public SessionsRepository(DbConnectionFactory connectionFactory, ICurrentUser currentUser) : base(connectionFactory, currentUser)
        {
        }

        protected override ColumnDefinition[] Columns { get; } =
        {
            new ColumnDefinition { PropertyName = nameof(Session.UserId),         ColumnName = "user_id" },
            new ColumnDefinition { PropertyName = nameof(Session.AccessToken),    ColumnName = "access_token" },
            new ColumnDefinition { PropertyName = nameof(Session.RefreshToken),   ColumnName = "refresh_token" },
            new ColumnDefinition { PropertyName = nameof(Session.ExpiresAt),      ColumnName = "expires_at" },
            new ColumnDefinition { PropertyName = nameof(Session.IsRevoked),      ColumnName = "is_revoked" }
        };

        protected override string TableName { get; } = "sessions";

        public Task<Session?> GetByRefreshToken(string refreshToken)
        {
            var query = $@"
                SELECT {_selectColumnsSql}
                FROM {TableName}
                WHERE refresh_token = @RefreshToken
                LIMIT 1;
            ";
            var parameters = new { RefreshToken = refreshToken };
            return _connectionFactory.Use((conn, cancellation) =>
                conn.QueryFirstOrDefaultAsync<Session>(
                    new CommandDefinition(
                        commandText: query,
                        parameters,
                        cancellationToken: cancellation
                    )
                )
            );
        }
    }
}
