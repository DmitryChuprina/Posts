using Posts.Application.Repositories;
using Posts.Domain.Entities;
using Posts.Domain.Shared.Enums;
using Posts.Infrastructure.IntegrationTests.Fixtures;
using Posts.Infrastructure.IntegrationTests.Repositories.Base;
using Posts.Infrastructure.Repositories;

namespace Posts.Infrastructure.IntegrationTests.Repositories
{
    [Collection("IntegrationTests")]
    public class PostViewsRepositoryTests : GenericBaseRepositoryTests<IPostViewsRepository, PostView>, IAsyncLifetime
    {
        private readonly IUsersRepository _usersRepository;
        private readonly IPostsRepository _postsRepository;

        private Guid _userId = Guid.Empty;

        public PostViewsRepositoryTests(TestInfrastructure infra) : base(infra)
        {
            _usersRepository = new UsersRepository(_infra.ConnectionFactory, _userMock.Object);
            _postsRepository = new PostsRepository(_infra.ConnectionFactory, _userMock.Object);
        }

        protected override IPostViewsRepository CreateRepository()
        {
            return new PostViewsRepository(_infra.ConnectionFactory, _userMock.Object);
        }

        protected override async Task<PostView> CreateSampleEntityAsync()
        {
            var post = new Post
            {
                Content = "This is a sample post content.",
                CreatedBy = _userId
            };

            await _postsRepository.AddAsync(post);

            return new PostView
            {
                PostId = post.Id,
                UserId = _userId,
                FirstViewedAt = DateTime.UtcNow,
                LastViewedAt = DateTime.UtcNow
            };
        }

        protected override async Task MakeChangesForUpdateAsync(PostView entity)
        {
            entity.LastViewedAt = DateTime.UtcNow;
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
