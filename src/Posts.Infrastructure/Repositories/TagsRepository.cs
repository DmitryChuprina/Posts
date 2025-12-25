using Dapper;
using Posts.Application.Core;
using Posts.Application.Repositories;
using Posts.Domain.Entities;
using Posts.Infrastructure.Interfaces;
using Posts.Infrastructure.Repositories.Base;
using Posts.Infrastructure.Repositories.Models;

namespace Posts.Infrastructure.Repositories
{
    internal class TagsRepository : BaseRepository<Tag>, ITagsRepository
    {
        public TagsRepository(IDbConnectionFactory connectionFactory, ICurrentUser currentUser) : base(connectionFactory, currentUser)
        {
        }
        protected override string TableName => "tags";
        protected override ColumnDefinition[] Columns { get; } =
        {
            new ColumnDefinition { PropertyName = nameof(Tag.Name), ColumnName = "name" },
            new ColumnDefinition { PropertyName = nameof(Tag.UsageCount), ColumnName = "usage_count", SkipOnUpdate = true },
            new ColumnDefinition { PropertyName = nameof(Tag.LastUsedAt), ColumnName = "last_used_at", SkipOnUpdate = true }
        };

        public async Task UpsertTagsStatsAsync(string[] tags)
        {
            if (tags.Length == 0)
            {
                return;
            }

            var parameters = new { Tags = tags, Now = DateTime.UtcNow };
            var sql = $@"
                INSERT INTO {TableName} (name, usage_count, last_used_at)
                SELECT 
                    t,
                    1,
                    @Now
                FROM unnest(@Tags) as t
                ON CONFLICT (name) 
                DO UPDATE SET 
                    usage_count = {TableName}.usage_count + 1,
                    last_used_at = EXCLUDED.last_used_at,
                    row_version = {TableName}.row_version + 1;
            ";
            await _connectionFactory.Use((conn, cancellation, tr) =>
                    conn.ExecuteAsync(
                        new CommandDefinition(
                            commandText: sql,
                            parameters,
                            cancellationToken: cancellation,
                            transaction: tr
                        )
                    )
                );
        }

        public async Task DecrementTagsUsageAsync(string[] tags)
        {
            if (tags.Length == 0)
            {
                return;
            }

            var parameters = new { Tags = tags };
            var sql = @"
                UPDATE tags
                SET 
                    usage_count = GREATEST(0, usage_count - 1),
                    row_version = row_version + 1
                WHERE name = ANY(@Tags)";

            await _connectionFactory.Use((conn, cancellation, tr) =>
                    conn.ExecuteAsync(
                        new CommandDefinition(
                            commandText: sql,
                            parameters,
                            cancellationToken: cancellation,
                            transaction: tr
                        )
                    )
                );
        }
    }
}
