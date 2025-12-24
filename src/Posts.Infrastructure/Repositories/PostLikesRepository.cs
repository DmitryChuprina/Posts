using Posts.Application.Core;
using Posts.Application.Repositories;
using Posts.Domain.Entities;
using Posts.Infrastructure.Repositories.Base;
using Posts.Infrastructure.Repositories.Models;

namespace Posts.Infrastructure.Repositories
{
    internal class PostLikesRepository : BaseRepository<PostLike>, IPostLikesRepository
    {
        public PostLikesRepository(DbConnectionFactory connectionFactory, ICurrentUser currentUser) : base(connectionFactory, currentUser)
        {
        }

        protected override string TableName => "post_likes";

        protected override ColumnDefinition[] Columns { get; } = {
            new ColumnDefinition { PropertyName = nameof(PostLike.PostId), ColumnName = "post_id" },
            new ColumnDefinition { PropertyName = nameof(PostLike.UserId), ColumnName = "user_id" },
            new ColumnDefinition { PropertyName = nameof(PostLike.LikedAt), ColumnName = "liked_at" }
        };
    }
}
