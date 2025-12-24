using Dapper;
using Posts.Application.Core;
using Posts.Application.Repositories;
using Posts.Domain.Entities;
using Posts.Infrastructure.Repositories.Base;
using Posts.Infrastructure.Repositories.Models;

namespace Posts.Infrastructure.Repositories
{
    internal class PostMediaRepository : BaseRepository<PostMedia> , IPostMediaRepository
    {
        public PostMediaRepository(DbConnectionFactory connectionFactory, ICurrentUser currentUser) : base(connectionFactory, currentUser)
        {
        }
        protected override string TableName => "post_media";
        protected override ColumnDefinition[] Columns { get; } =
        {
            new ColumnDefinition { PropertyName = nameof(PostMedia.PostId), ColumnName = "post_id" },
            new ColumnDefinition { PropertyName = nameof(PostMedia.Key), ColumnName = "key" },
            new ColumnDefinition { PropertyName = nameof(PostMedia.SortOrder), ColumnName = "sort_order" }
        };

        public async Task<IEnumerable<PostMedia>> GetByPostIdAsync(Guid postId)
        {
            var sql = $"SELECT {_selectColumnsSql} FROM {TableName} WHERE post_id = @PostId ORDER BY sort_order;";
            var parameters = new { PostId = postId };

            return await _connectionFactory.Use(async (conn, cancellation, tr) =>
            {
                return await conn.QueryAsync<PostMedia>(
                    new CommandDefinition(
                        commandText: sql,
                        parameters,
                        cancellationToken: cancellation,
                        transaction: tr
                    )
                );
            });
        }
    }
}
