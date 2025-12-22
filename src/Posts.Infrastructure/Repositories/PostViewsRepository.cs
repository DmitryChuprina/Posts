using Posts.Application.Core;
using Posts.Application.Repositories;
using Posts.Domain.Entities;
using Posts.Infrastructure.Repositories.Base;
using Posts.Infrastructure.Repositories.Models;

namespace Posts.Infrastructure.Repositories
{
    internal class PostViewsRepository : BaseRepository<PostView>, IPostViewsRepository
    {
        public PostViewsRepository(DbConnectionFactory connectionFactory, ICurrentUser currentUser) : base(connectionFactory, currentUser)
        {
        }

        protected override string TableName => "post_views";

        protected override ColumnDefinition[] Columns => new ColumnDefinition[] {
            new ColumnDefinition { PropertyName = nameof(PostView.PostId), ColumnName = "post_id" },
            new ColumnDefinition { PropertyName = nameof(PostView.UserId), ColumnName = "user_id" },
            new ColumnDefinition { PropertyName = nameof(PostView.FirstViewedAt), ColumnName = "first_viewed_at" },
            new ColumnDefinition { PropertyName = nameof(PostView.FirstViewedAt), ColumnName = "last_viewed_at" },
        };
    }
}
