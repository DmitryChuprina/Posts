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
        protected override string TableName => "post_medias";
        protected override ColumnDefinition[] Columns { get; } =
        {
            new ColumnDefinition { PropertyName = nameof(PostMedia.PostId), ColumnName = "post_id" },
            new ColumnDefinition { PropertyName = nameof(PostMedia.Key), ColumnName = "key" },
            new ColumnDefinition { PropertyName = nameof(PostMedia.Order), ColumnName = "order" }
        };
    }
}
