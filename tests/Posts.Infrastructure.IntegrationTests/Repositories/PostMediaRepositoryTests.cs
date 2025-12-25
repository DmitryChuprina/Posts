using FluentAssertions;
using Posts.Application.Repositories;
using Posts.Domain.Entities;
using Posts.Infrastructure.IntegrationTests.Fixtures;
using Posts.Infrastructure.IntegrationTests.Repositories.Base;
using Posts.Infrastructure.Repositories;

namespace Posts.Infrastructure.IntegrationTests.Repositories
{
    [Collection("IntegrationTests")]
    public class PostMediaRepositoryTests : GenericBaseRepositoryTests<IPostMediaRepository, PostMedia>, IAsyncLifetime
    {
        private IPostsRepository _postsRepository;

        private Guid _postId;
        private int _order = 0;

        public PostMediaRepositoryTests(TestInfrastructure infra) : base(infra)
        {
            _postsRepository = new PostsRepository(_infra.ConnectionFactory, _userMock.Object);
        }

        protected override IPostMediaRepository CreateRepository()
        {
            return new PostMediaRepository(_infra.ConnectionFactory, _userMock.Object);
        }

        protected override async Task<PostMedia> CreateSampleEntityAsync()
        {
            var post = new PostMedia
            {
                PostId = _postId,
                Key = $"sample_key_{_order}.jpg",
                SortOrder = _order
            };
            _order++;
            return post;
        }

        protected override async Task MakeChangesForUpdateAsync(PostMedia entity)
        {
            entity.Key = "updated_key.jpg";
        }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();

            var post = new Post
            {
                Content = "This is a sample post for testing."
            };

            await _postsRepository.AddAsync(post);
            _postId = post.Id;
        }

        [Fact]
        public async Task GetByPostId_Should_Return_Media_Ordered_By_SortOrder()
        {
            // Arrange
            var media1 = await CreateSampleEntityAsync();
            media1.SortOrder = 5;
            media1.Key = "last.jpg";

            var media2 = await CreateSampleEntityAsync();
            media2.SortOrder = 1;
            media2.Key = "first.jpg";

            var media3 = await CreateSampleEntityAsync();
            media3.SortOrder = 3;
            media3.Key = "middle.jpg";

            await _repo.AddManyAsync(new[] { media1, media2, media3 });

            // Act
            var result = await _repo.GetByPostIdAsync(_postId);

            // Assert
            var list = result.ToList();
            list.Should().HaveCount(3);

            list[0].SortOrder.Should().Be(1);
            list[0].Key.Should().Be("first.jpg");

            list[1].SortOrder.Should().Be(3);
            list[1].Key.Should().Be("middle.jpg");

            list[2].SortOrder.Should().Be(5);
            list[2].Key.Should().Be("last.jpg");
            result.Should().BeInAscendingOrder(x => x.SortOrder);
        }

        [Fact]
        public async Task GetByPostId_Should_Not_Return_Media_From_Other_Posts()
        {
            // Arrange
            var targetMedia = await CreateSampleEntityAsync();
            await _repo.AddAsync(targetMedia);


            var otherPost = new Post
            {
                Content = "Other post content"
            };
            await _postsRepository.AddAsync(otherPost);

            var otherMedia = await CreateSampleEntityAsync();
            otherMedia.PostId = otherPost.Id;
            otherMedia.Key = "other_post_media.jpg";
            await _repo.AddAsync(otherMedia);

            // Act
            var result = await _repo.GetByPostIdAsync(_postId);

            // Assert
            result.Should().HaveCount(1);
            result.First().Id.Should().Be(targetMedia.Id);
            result.Should().NotContain(x => x.Id == otherMedia.Id);
        }

        [Fact]
        public async Task GetByPostId_Should_Return_Empty_List_If_No_Media()
        {
            // Act
            var result = await _repo.GetByPostIdAsync(_postId);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }
    }
}
