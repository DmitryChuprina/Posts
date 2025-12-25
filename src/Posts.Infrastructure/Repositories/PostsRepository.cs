using Dapper;
using Posts.Application.Core;
using Posts.Application.Repositories;
using Posts.Application.Repositories.Models;
using Posts.Contract.Models;
using Posts.Domain.Entities;
using Posts.Infrastructure.Interfaces;
using Posts.Infrastructure.Repositories.Base;
using Posts.Infrastructure.Repositories.Models;

namespace Posts.Infrastructure.Repositories
{
    internal class PostsRepository : BaseRepository<Post>, IPostsRepository
    {
        public PostsRepository(IDbConnectionFactory connectionFactory, ICurrentUser currentUser) : base(connectionFactory, currentUser)
        {
        }

        protected override string TableName => "posts";

        protected override ColumnDefinition[] Columns { get; } =
        {
            new ColumnDefinition { PropertyName = nameof(Post.Content),    ColumnName = "content" },
            new ColumnDefinition { PropertyName = nameof(Post.Tags),       ColumnName = "tags" },

            new ColumnDefinition { PropertyName = nameof(Post.ReplyForId), ColumnName = "reply_for_id" },
            new ColumnDefinition { PropertyName = nameof(Post.RepostId),   ColumnName = "repost_id" },

            new ColumnDefinition { PropertyName = nameof(Post.Depth),      ColumnName = "depth" },

            new ColumnDefinition { PropertyName = nameof(Post.LikesCount), ColumnName = "likes_count", SkipOnUpdate = true },
            new ColumnDefinition { PropertyName = nameof(Post.ViewsCount), ColumnName = "views_count", SkipOnUpdate = true },
            new ColumnDefinition { PropertyName = nameof(Post.RepliesCount), ColumnName = "replies_count", SkipOnUpdate = true },
            new ColumnDefinition { PropertyName = nameof(Post.RepostsCount), ColumnName = "reposts_count", SkipOnUpdate = true },
        };

        protected string PostReadSql => $@"
            SELECT 
                p.id as {nameof(PostReadModel.Id)},
                p.content as {nameof(PostReadModel.Content)},
                p.tags as {nameof(PostReadModel.Tags)},
                p.reply_for_id as {nameof(PostReadModel.ReplyForId)},
                p.repost_id as {nameof(PostReadModel.RepostId)},
                p.depth as {nameof(PostReadModel.Depth)},
                p.likes_count as {nameof(PostReadModel.LikesCount)},
                p.views_count as {nameof(PostReadModel.ViewsCount)},
                p.reposts_count as {nameof(PostReadModel.RepostsCount)},
                p.replies_count as {nameof(PostReadModel.RepliesCount)},
                pm.id as {nameof(PostReadModel.MediaId)},
                pm.key as {nameof(PostReadModel.MediaKey)},
                pm.sort_order as {nameof(PostReadModel.MediaOrder)},
                u.id as {nameof(PostReadModel.CreatorId)},
                u.username as {nameof(PostReadModel.CreatorUsername)},
                u.first_name as {nameof(PostReadModel.CreatorFirstName)},
                u.last_name as {nameof(PostReadModel.CreatorLastName)},
                u.profile_image_key as {nameof(PostReadModel.CreatorProfileImageKey)}
            FROM {TableName} p
            LEFT JOIN post_media pm ON pm.post_id = p.id
            LEFT JOIN users u ON u.id = p.created_by";

        public async Task<IEnumerable<PostReadModel>> GetReadModelsByIdsAsync(IEnumerable<Guid> ids)
        {
            if (!ids.Any())
            {
                return [];
            }

            var sql = $@"
                {PostReadSql} 
                WHERE p.id = ANY(@Ids)
            ";
            var parameters = new { Ids = ids.ToArray() };
            return await _connectionFactory.Use(async (conn, cancellation, tr) =>
            {
                return await conn.QueryAsync<PostReadModel>(
                    new CommandDefinition(
                        commandText: sql,
                        parameters,
                        cancellationToken: cancellation,
                        transaction: tr
                    )
                );
            });
        }

        private string GetPostsByCreatorWhere(
           Guid creatorId,
           bool? withRepliesOrRepost = null
        ){
            var sql = "created_by = @CreatorId";

            if (withRepliesOrRepost is true)
            {
                return $"{sql} AND (reply_for_id is not null OR repost_id is not null)";
            }

            if( withRepliesOrRepost is false)
            {
                return $"{sql} AND (reply_for_id is null AND repost_id is null)";
            }

            return sql;
        }

        public async Task<int> GetPostsByCreatorCountAsync(
           Guid creatorId,
           bool? withRepliesOrRepost = null
        )
        {
            var sql = $@"
                SELECT count(id)
                FROM {TableName}
                WHERE {GetPostsByCreatorWhere(creatorId, withRepliesOrRepost)};
            ";

            return await _connectionFactory.Use(async (conn, cancellation, tr) =>
            {
                return await conn.ExecuteScalarAsync<int>(
                    new CommandDefinition(
                        commandText: sql,
                        parameters: new { CreatorId = creatorId },
                        cancellationToken: cancellation,
                        transaction: tr
                    )
                );
            });
        }

        public async Task<IEnumerable<Post>> GetPostsByCreatorAsync(
            Guid creatorId,
            PaginationRequestDto pagination,
            bool? withRepliesOrRepost = null
        )
        {
            var sql = $@"
                SELECT {_selectColumnsSql}
                FROM {TableName}
                WHERE {GetPostsByCreatorWhere(creatorId, withRepliesOrRepost)}
                ORDER BY created_at DESC
                LIMIT @Limit 
                OFFSET @From
            ";

            return await _connectionFactory.Use(async (conn, cancellation, tr) =>
            {
                return await conn.QueryAsync<Post>(
                    new CommandDefinition(
                        commandText: sql,
                        parameters: new { CreatorId = creatorId, pagination.Limit, pagination.From },
                        cancellationToken: cancellation,
                        transaction: tr
                    )
                );
            });
        }

        public async Task<int> GetPostRepliesCountAsync(Guid replyForId)
        {
            var sql = $@"
                SELECT count(*)
                FROM {TableName}
                WHERE reply_for_id = @ReplyForId
            ";

            return await _connectionFactory.Use(async (conn, cancellation, tr) =>
            {
                return await conn.ExecuteScalarAsync<int>(
                    new CommandDefinition(
                        commandText: sql,
                        parameters: new { ReplyForId = replyForId },
                        cancellationToken: cancellation,
                        transaction: tr
                    )
                );
            });
        }

        public async Task<IEnumerable<Post>> GetPostRepliesAsync(
            Guid replyForId,
            PaginationRequestDto pagination
        )
        {
            var sql = $@"
                SELECT {_selectColumnsSql}
                FROM {TableName}
                WHERE reply_for_id = @ReplyForId
                ORDER BY created_at DESC
                LIMIT @Limit 
                OFFSET @From
            ";

            return await _connectionFactory.Use(async (conn, cancellation, tr) =>
            {
                return await conn.QueryAsync<Post>(
                    new CommandDefinition(
                        commandText: sql,
                        parameters: new { ReplyForId = replyForId, pagination.Limit, pagination.From },
                        cancellationToken: cancellation,
                        transaction: tr
                    )
                );
            });
        }

        public async Task IncrementLikesCountAsync(Guid postId)
        {
            await ChangeCounterAsync(postId, "likes_count");
        }

        public async Task DecrementLikesCountAsync(Guid postId)
        {
            await ChangeCounterAsync(postId, "likes_count", true);
        }

        public async Task IncrementViewsCountAsync(Guid postId)
        {
            await ChangeCounterAsync(postId, "views_count");
        }

        public async Task IncrementRepostsCountAsync(Guid postId)
        {
            await ChangeCounterAsync(postId, "reposts_count");
        }

        public async Task DecrementRepostsCountAsync(Guid postId)
        {
            await ChangeCounterAsync(postId, "reposts_count", true);
        }

        public async Task IncrementRepliesCountAsync(Guid postId)
        {
            await ChangeCounterAsync(postId, "replies_count");
        }

        public async Task DecrementRepliesCountAsync(Guid postId)
        {
            await ChangeCounterAsync(postId, "replies_count", true);
        }

        private async Task ChangeCounterAsync(Guid postId, string col, bool isDecrement = false)
        {
            var sign = isDecrement ? "-" : "+";
            var sql = $@"
                UPDATE {TableName}
                SET {col} = GREATEST(0, {col} {sign} 1),
                    row_version = row_version + 1
                WHERE id = @PostId
            ";

            await _connectionFactory.Use(async (conn, cancellation, tr) =>
            {
                await conn.ExecuteAsync(
                    new CommandDefinition(
                        commandText: sql,
                        parameters: new { PostId = postId },
                        cancellationToken: cancellation,
                        transaction: tr
                    )
                );
            });
        }
    }
}
