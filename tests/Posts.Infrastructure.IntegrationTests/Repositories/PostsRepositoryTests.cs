using Dapper;
using FluentAssertions;
using Posts.Application.Repositories;
using Posts.Contract.Models;
using Posts.Domain.Entities;
using Posts.Domain.Shared.Enums;
using Posts.Infrastructure.IntegrationTests.Fixtures;
using Posts.Infrastructure.IntegrationTests.Repositories.Base;
using Posts.Infrastructure.Repositories;

namespace Posts.Infrastructure.IntegrationTests.Repositories
{
    [Collection("IntegrationTests")]
    public class PostsRepositoryTests : GenericBaseRepositoryTests<IPostsRepository, Post>, IAsyncLifetime
    {
        private readonly IUsersRepository _usersRepository;
        private readonly IPostMediaRepository _postMediaRepository;

        private Guid _userId;
        private User _user = null!;

        public PostsRepositoryTests(TestInfrastructure infra) : base(infra)
        {
            _usersRepository = new UsersRepository(_infra.ConnectionFactory, _userMock.Object);
            _postMediaRepository = new Infrastructure.Repositories.PostMediaRepository(_infra.ConnectionFactory, _userMock.Object);
        }

        protected override IPostsRepository CreateRepository()
        {
            return new PostsRepository(_infra.ConnectionFactory, _userMock.Object);
        }

        protected override async Task<Post> CreateSampleEntityAsync()
        {
            return new Post
            {
                Content = $"Sample content {Guid.NewGuid()} #tag1 #tag2",
                Tags = new[] { "tag1", "tag2" },
                CreatedBy = _userId
            };
        }

        protected override async Task MakeChangesForUpdateAsync(Post entity)
        {
            entity.Content = $"Updated content {Guid.NewGuid()}";
        }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();

            _user = new User
            {
                Username = $"testuser{Guid.NewGuid()}",
                Email = $"testuser{Guid.NewGuid()}@example.com",
                Password = $"Password123!{Guid.NewGuid()}",
                Role = UserRole.User,
            };

            await _usersRepository.AddAsync(_user);

            _userId = _user.Id;
        }

        [Fact]
        public async Task GetReadModelsByIds_Should_Return_Full_Data_With_Joins()
        {
            // Arrange
            var post = await CreateSampleEntityAsync();
            await _repo.AddAsync(post);

            var mediaId = Guid.NewGuid();
            await AddPostMediaAsync(post.Id, mediaId, "s3_key_video.mp4", 0);

            // Act
            var results = await _repo.GetReadModelsByIdsAsync(new[] { post.Id });

            // Assert
            var readModel = results.Should().HaveCount(1).And.Subject.First();

            readModel.Id.Should().Be(post.Id);
            readModel.Content.Should().Be(post.Content);
            readModel.LikesCount.Should().Be(0);

            readModel.CreatorId.Should().Be(_user.Id);
            readModel.CreatorUsername.Should().Be(_user.Username);
            readModel.CreatorFirstName.Should().Be(_user.FirstName);
            readModel.CreatorProfileImageKey.Should().Be(_user.ProfileImageKey);

            readModel.MediaId.Should().Be(mediaId);
            readModel.MediaKey.Should().Be("s3_key_video.mp4");
        }


        [Fact]
        public async Task GetPostsByCreator_Should_Filter_Replies_And_Reposts()
        {
            _userMock.Setup(u => u.UserId).Returns(_userId);

            // Arrange
            var original = await CreateSampleEntityAsync();
            await _repo.AddAsync(original);

            var reply = await CreateSampleEntityAsync();
            reply.ReplyForId = original.Id;
            await _repo.AddAsync(reply);

            var repost = await CreateSampleEntityAsync();
            repost.RepostId = original.Id;
            await _repo.AddAsync(repost);

            var pagination = new PaginationRequestDto { Limit = 10, From = 0 };

            // Act & Assert

            var allCount = await _repo.GetPostsByCreatorCountAsync(_userId, withRepliesOrRepost: null);
            var allPosts = await _repo.GetPostsByCreatorAsync(_userId, pagination, withRepliesOrRepost: null);
            allCount.Should().Be(3);
            allPosts.Should().HaveCount(3);

            var originalsCount = await _repo.GetPostsByCreatorCountAsync(_userId, withRepliesOrRepost: false);
            var originalPosts = await _repo.GetPostsByCreatorAsync(_userId, pagination, withRepliesOrRepost: false);
            originalsCount.Should().Be(1);
            originalPosts.Should().ContainSingle(p => p.Id == original.Id);

            var repliesCount = await _repo.GetPostsByCreatorCountAsync(_userId, withRepliesOrRepost: true);
            var replyPosts = await _repo.GetPostsByCreatorAsync(_userId, pagination, withRepliesOrRepost: true);
            repliesCount.Should().Be(2);
            replyPosts.Select(p => p.Id).Should().Contain(new[] { reply.Id, repost.Id });
        }

        [Fact]
        public async Task GetPostsByCreator_Should_Respect_Pagination()
        {
            // Arrange
            for (int i = 0; i < 5; i++)
            {
                var p = await CreateSampleEntityAsync();
                p.CreatedAt = DateTime.UtcNow.AddSeconds(-i);
                await _repo.AddAsync(p);
            }

            // Act
            var page1 = await _repo.GetPostsByCreatorAsync(_userId, new PaginationRequestDto { Limit = 2, From = 0 });
            var page2 = await _repo.GetPostsByCreatorAsync(_userId, new PaginationRequestDto { Limit = 2, From = 2 });

            // Assert
            page1.Should().HaveCount(2);
            page2.Should().HaveCount(2);

            page1.Intersect(page2).Should().BeEmpty();
        }

        [Fact]
        public async Task GetPostReplies_Should_Return_Only_Children()
        {
            // Arrange
            var parent = await CreateSampleEntityAsync();
            await _repo.AddAsync(parent);

            var reply1 = await CreateSampleEntityAsync();
            reply1.ReplyForId = parent.Id;
            await _repo.AddAsync(reply1);

            var reply2 = await CreateSampleEntityAsync();
            reply2.ReplyForId = parent.Id;
            await _repo.AddAsync(reply2);

            var unrelated = await CreateSampleEntityAsync();
            await _repo.AddAsync(unrelated);

            // Act
            var count = await _repo.GetPostRepliesCountAsync(parent.Id);
            var replies = await _repo.GetPostRepliesAsync(parent.Id, new PaginationRequestDto { Limit = 10, From = 0 });

            // Assert
            count.Should().Be(2);
            replies.Should().HaveCount(2);
            replies.Should().Contain(p => p.Id == reply1.Id);
            replies.Should().Contain(p => p.Id == reply2.Id);
            replies.Should().NotContain(p => p.Id == unrelated.Id);
        }

        [Fact]
        public async Task Counters_Should_Increment_And_Decrement_Correctly()
        {
            // Arrange
            var post = await CreateSampleEntityAsync();
            await _repo.AddAsync(post);
            var originalVersion = post.RowVersion;

            // Act - Increment Likes
            await _repo.IncrementLikesCountAsync(post.Id);
            var postAfterLike = await _repo.GetByIdAsync(post.Id);

            // Act - Increment Views
            await _repo.IncrementViewsCountAsync(post.Id);
            var postAfterView = await _repo.GetByIdAsync(post.Id);

            // Assert
            postAfterLike!.LikesCount.Should().Be(1);
            postAfterLike.RowVersion.Should().BeGreaterThan(originalVersion);

            postAfterView!.ViewsCount.Should().Be(1);
        }

        [Fact]
        public async Task Decrement_Counter_Should_Not_Go_Below_Zero()
        {
            // Arrange
            var post = await CreateSampleEntityAsync();
            post.LikesCount = 0;
            await _repo.AddAsync(post);

            // Act
            await _repo.DecrementLikesCountAsync(post.Id);
            await _repo.DecrementLikesCountAsync(post.Id);

            // Assert
            var fromDb = await _repo.GetByIdAsync(post.Id);
            fromDb!.LikesCount.Should().Be(0); 

            fromDb.RowVersion.Should().BeGreaterThan(post.RowVersion);
        }

        [Fact]
        public async Task Counters_Logic_Check_For_Reposts_And_Replies()
        {
            var post = await CreateSampleEntityAsync();
            await _repo.AddAsync(post);

            // Reposts
            await _repo.IncrementRepostsCountAsync(post.Id);
            (await _repo.GetByIdAsync(post.Id))!.RepostsCount.Should().Be(1);

            await _repo.DecrementRepostsCountAsync(post.Id);
            (await _repo.GetByIdAsync(post.Id))!.RepostsCount.Should().Be(0);

            // Replies
            await _repo.IncrementRepliesCountAsync(post.Id);
            (await _repo.GetByIdAsync(post.Id))!.RepliesCount.Should().Be(1);
        }

        private async Task AddPostMediaAsync(Guid postId, Guid mediaId, string key, int order)
        {
            await _postMediaRepository.AddAsync(new PostMedia
            {
                Id = mediaId,
                PostId = postId,
                Key = key,
                SortOrder = order
            });
        }
    }
}
