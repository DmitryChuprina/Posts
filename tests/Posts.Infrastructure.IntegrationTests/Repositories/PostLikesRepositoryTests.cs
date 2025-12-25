using Posts.Application.Repositories;
using Posts.Domain.Entities;
using Posts.Domain.Shared.Enums;
using Posts.Infrastructure.IntegrationTests.Fixtures;
using Posts.Infrastructure.IntegrationTests.Repositories.Base;
using Posts.Infrastructure.Repositories;

namespace Posts.Infrastructure.IntegrationTests.Repositories
{
    [Collection("IntegrationTests")]
    public class PostLikesRepositoryTests : GenericBaseRepositoryTests<IPostLikesRepository, PostLike>, IAsyncLifetime
    {
        private readonly IUsersRepository _usersRepository;
        private readonly IPostsRepository _postsRepository;

        private Guid _userId = Guid.Empty;

        public PostLikesRepositoryTests(TestInfrastructure infra) : base(infra)
        {
            _usersRepository = new UsersRepository(_infra.ConnectionFactory, _userMock.Object);
            _postsRepository = new PostsRepository(_infra.ConnectionFactory, _userMock.Object);
        }

        protected override IPostLikesRepository CreateRepository()
        {
            return new PostLikesRepository(_infra.ConnectionFactory, _userMock.Object);
        }

        protected override async Task<PostLike> CreateSampleEntityAsync()
        {
            var post = new Post
            {
                Content = "This is a sample post content.",
                CreatedBy = _userId
            };

            await _postsRepository.AddAsync(post);

            return new PostLike
            {
                PostId = post.Id,
                UserId = _userId,
                LikedAt = DateTime.UtcNow
            };
        }

        protected override async Task MakeChangesForUpdateAsync(PostLike entity)
        {
            entity.LikedAt = DateTime.UtcNow;
        }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();

            var user = new User
            {
                Username = $"testuser{Guid.NewGuid()}",
                Email = $"testuser{Guid.NewGuid()}@example.com",
                Password = $"Password123!{Guid.NewGuid()}",
                Role = UserRole.User,
            };

            await _usersRepository.AddAsync(user);

            _userId = user.Id;
        }
    }
}
