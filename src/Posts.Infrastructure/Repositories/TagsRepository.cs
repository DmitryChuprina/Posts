using Posts.Application.Core;
using Posts.Application.Repositories;
using Posts.Domain.Entities;
using Posts.Infrastructure.Repositories.Base;
using Posts.Infrastructure.Repositories.Models;

namespace Posts.Infrastructure.Repositories
{
    internal class TagsRepository : BaseRepository<Tag>, ITagsRepository
    {
        public TagsRepository(DbConnectionFactory connectionFactory, ICurrentUser currentUser) : base(connectionFactory, currentUser)
        {
        }
        protected override string TableName => "tags";
        protected override ColumnDefinition[] Columns { get; } =
        {
            new ColumnDefinition { PropertyName = nameof(Tag.Name), ColumnName = "name" },
            new ColumnDefinition { PropertyName = nameof(Tag.UsageCount), ColumnName = "usage_count", SkipOnUpdate = true },
            new ColumnDefinition { PropertyName = nameof(Tag.LastUsedAt), ColumnName = "last_usage_at" }
        };
    }
}
