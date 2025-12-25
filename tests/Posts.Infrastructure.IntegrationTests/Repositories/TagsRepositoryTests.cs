using Dapper;
using FluentAssertions;
using Posts.Application.Repositories;
using Posts.Domain.Entities;
using Posts.Infrastructure.IntegrationTests.Fixtures;
using Posts.Infrastructure.IntegrationTests.Repositories.Base;
using Posts.Infrastructure.Repositories;

namespace Posts.Infrastructure.IntegrationTests.Repositories
{
    [Collection("IntegrationTests")]
    public class TagsRepositoryTests : GenericBaseRepositoryTests<ITagsRepository, Tag>, IAsyncLifetime
    {
        public TagsRepositoryTests(TestInfrastructure infra) : base(infra)
        {
        }
        protected override ITagsRepository CreateRepository()
        {
            return new TagsRepository(_infra.ConnectionFactory, _userMock.Object);
        }
        protected override async Task<Tag> CreateSampleEntityAsync()
        {
            return new Tag
            {
                Name = $"tag{Guid.NewGuid()}",
                LastUsedAt = DateTime.UtcNow,
            };
        }
        protected override async Task MakeChangesForUpdateAsync(Tag entity)
        {
            entity.Name = $"updated-{entity.Name}";
        }

        [Fact]
        public async Task UpsertTagsStats_Should_Create_New_Tags_With_Count_1()
        {
            // Arrange
            var tags = new[] { "c#", "docker", "testing" };

            // Act
            await _repo.UpsertTagsStatsAsync(tags);

            // Assert
            foreach (var tag in tags)
            {
                var fromDb = await GetTagByName(tag);
                fromDb.Should().NotBeNull();
                fromDb!.UsageCount.Should().Be(1);
                fromDb.LastUsedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
            }
        }

        [Fact]
        public async Task UpsertTagsStats_Should_Increment_Count_For_Existing_Tags()
        {
            // Arrange
            var tagName = "existing_tag";
            await _repo.UpsertTagsStatsAsync(new[] { tagName }); // Count = 1

            await Task.Delay(100);

            // Act
            await _repo.UpsertTagsStatsAsync(new[] { tagName, "new_tag" });

            // Assert
            var existingTag = await GetTagByName(tagName);
            existingTag.Should().NotBeNull();
            existingTag!.UsageCount.Should().Be(2); // 1 + 1

            existingTag.LastUsedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));

            var newTag = await GetTagByName("new_tag");
            newTag!.UsageCount.Should().Be(1);
        }

        [Fact]
        public async Task DecrementTagsUsage_Should_Decrease_Count()
        {
            // Arrange
            var tagName = "popular_tag";
            await _repo.UpsertTagsStatsAsync(new[] { tagName });
            await _repo.UpsertTagsStatsAsync(new[] { tagName });
            await _repo.UpsertTagsStatsAsync(new[] { tagName });

            var before = await GetTagByName(tagName);
            before!.UsageCount.Should().Be(3);

            // Act
            await _repo.DecrementTagsUsageAsync(new[] { tagName });

            // Assert
            var after = await GetTagByName(tagName);
            after!.UsageCount.Should().Be(2);
        }

        [Fact]
        public async Task DecrementTagsUsage_Should_Not_Go_Below_Zero()
        {
            // Arrange
            var tagName = "rare_tag";
            await _repo.UpsertTagsStatsAsync(new[] { tagName }); // Count = 1

            // Act
            await _repo.DecrementTagsUsageAsync(new[] { tagName });
            await _repo.DecrementTagsUsageAsync(new[] { tagName });

            // Assert
            var after = await GetTagByName(tagName);
            after!.UsageCount.Should().Be(0);
        }

        [Fact]
        public async Task DecrementTagsUsage_Should_Update_RowVersion()
        {
            // Arrange
            var tagName = "version_check";
            await _repo.UpsertTagsStatsAsync(new[] { tagName });
            var original = await GetTagByName(tagName);

            // Act
            await _repo.DecrementTagsUsageAsync(new[] { tagName });

            // Assert
            var updated = await GetTagByName(tagName);
            updated!.RowVersion.Should().BeGreaterThan(original!.RowVersion);
        }

        private async Task<Tag?> GetTagByName(string name)
        {
            var id = await _infra.ConnectionFactory.Use((conn, ct, tx) =>
                conn.ExecuteScalarAsync<Guid?>(
                    new CommandDefinition(
                        $"SELECT id FROM tags WHERE name = @Name",
                        new { Name = name },
                        cancellationToken: ct
                    )
                ));
            if(id is null)
            {
                return null;
            }
            return await _repo.GetByIdAsync(id.Value);
        }
    }
}
