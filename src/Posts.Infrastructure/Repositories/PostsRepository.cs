using Posts.Application.Core;
using Posts.Application.Repositories;
using Posts.Domain.Entities;
using Posts.Infrastructure.Repositories.Base;
using Posts.Infrastructure.Repositories.Models;

namespace Posts.Infrastructure.Repositories
{
    internal class PostsRepository : BaseRepository<Post>, IPostsRepository
    {
        internal PostsRepository(DbConnectionFactory connectionFactory, ICurrentUser currentUser) : base(connectionFactory, currentUser)
        {
        }

        protected override string TableName => "posts";

        protected override ColumnDefinition[] Columns { get; } =
        {
            new ColumnDefinition { PropertyName = nameof(Post.Content),         ColumnName = "content" },
            new ColumnDefinition { PropertyName = nameof(Post.Tags),           ColumnName = "tags" },

            new ColumnDefinition { PropertyName = nameof(Post.ReplyForId),             ColumnName = "reply_for_id" },
            new ColumnDefinition { PropertyName = nameof(Post.RepostId),        ColumnName = "repost_id" },

            new ColumnDefinition { PropertyName = nameof(Post.Depth),         ColumnName = "depth" },

            new ColumnDefinition { PropertyName = nameof(Post.LikesCount),      ColumnName = "likes_count", SkipOnUpdate = true },
            new ColumnDefinition { PropertyName = nameof(Post.ViewsCount),  ColumnName = "views_count", SkipOnUpdate = true },
        };
    }
}
