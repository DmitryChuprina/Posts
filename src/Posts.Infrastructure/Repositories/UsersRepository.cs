using Dapper;
using Posts.Application.Core;
using Posts.Application.Repositories;
using Posts.Domain.Entities;
using Posts.Infrastructure.Repositories.Base;
using Posts.Infrastructure.Repositories.Models;
using static Dapper.SqlMapper;

namespace Posts.Infrastructure.Repositories
{
    internal class UsersRepository : BaseRepository<User>, IUsersRepository
    {
        public UsersRepository(DbConnectionFactory connectionFactory, ICurrentUser currentUser) : base(connectionFactory, currentUser)
        {
        }

        protected override ColumnDefinition[] Columns { get; } =
        {
            new ColumnDefinition { PropertyName = nameof(User.Username),         ColumnName = "username" },
            new ColumnDefinition { PropertyName = nameof(User.Email),            ColumnName = "email" },
            new ColumnDefinition { PropertyName = nameof(User.Password),         ColumnName = "password" },
            new ColumnDefinition { PropertyName = nameof(User.Role),             ColumnName = "role" },

            new ColumnDefinition { PropertyName = nameof(User.FirstName),        ColumnName = "first_name" },
            new ColumnDefinition { PropertyName = nameof(User.LastName),         ColumnName = "last_name" },

            new ColumnDefinition { PropertyName = nameof(User.Description),      ColumnName = "description" },
            new ColumnDefinition { PropertyName = nameof(User.ProfileImageUrl),  ColumnName = "profile_image_url" },

            new ColumnDefinition { PropertyName = nameof(User.BlockedAt),      ColumnName = "blocked_at" },
            new ColumnDefinition { PropertyName = nameof(User.BlockReason),  ColumnName = "block_reason" },

            new ColumnDefinition { PropertyName = nameof(User.EmailIsConfirmed), ColumnName = "email_is_confirmed" },
        };

        protected override string TableName => "users";

        public Task<User?> GetByEmail(string email)
        {
            var sql = $@"
                SELECT {_selectColumnsSql}
                FROM {TableName}
                WHERE email = @Email
                LIMIT 1;";

            return _connectionFactory.Use((conn, cancellation) =>
                conn.QuerySingleOrDefaultAsync<User>(
                    new CommandDefinition(
                        commandText: sql,
                        parameters: new { Email = email },
                        cancellationToken: cancellation
                    )
                )
            );
        }

        public Task<User?> GetByUsername(string username)
        {
            var sql = $@"
                SELECT {_selectColumnsSql}
                FROM {TableName}
                WHERE username = @Username
                LIMIT 1;";

            return _connectionFactory.Use((conn, cancellation) =>
                conn.QuerySingleOrDefaultAsync<User>(
                    new CommandDefinition(
                        commandText: sql,
                        parameters: new { Username = username },
                        cancellationToken: cancellation
                    )
                )
            );
        }
    }
}
