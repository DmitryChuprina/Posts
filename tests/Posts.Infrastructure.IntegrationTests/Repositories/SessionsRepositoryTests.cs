using FluentAssertions;
using Posts.Application.Repositories;
using Posts.Domain.Entities;
using Posts.Infrastructure.IntegrationTests.Extensions;
using Posts.Infrastructure.IntegrationTests.Fixtures;
using Posts.Infrastructure.IntegrationTests.Repositories.Base;
using Posts.Infrastructure.Repositories;

namespace Posts.Infrastructure.IntegrationTests.Repositories
{
    [Collection("IntegrationTests")]
    public class SessionsRepositoryTests : GenericBaseRepositoryTests<ISessionsRepository, Session>, IAsyncLifetime
    {
        private readonly IUsersRepository _usersRepository;

        private Guid _userId = Guid.Empty;

        public SessionsRepositoryTests(TestInfrastructure infra) : base(infra)
        {
            _usersRepository = new UsersRepository(_infra.ConnectionFactory, _userMock.Object);
        }

        protected override ISessionsRepository CreateRepository()
        {
            return new SessionsRepository(_infra.ConnectionFactory, _userMock.Object);
        }

        protected override async Task<Session> CreateSampleEntityAsync()
        {
            return new Session
            {
                UserId = _userId,
                AccessToken = $"access-token-{Guid.NewGuid()}",
                RefreshToken = $"refresh-token-{Guid.NewGuid()}"
            };
        }

        protected override async Task MakeChangesForUpdateAsync(Session entity)
        {
            entity.AccessToken = $"updated-access-token-{Guid.NewGuid()}";
            entity.RefreshToken = $"updated-refresh-token-{Guid.NewGuid()}";
        }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();
            // Create a user to associate with sessions
            var user = new User
            {
                Username = $"testuser{Guid.NewGuid()}",
                Email = $"testuser{Guid.NewGuid()}@example.com",
                Password = $"Password123!{Guid.NewGuid()}",
                Role = Domain.Shared.Enums.UserRole.User,
            };
            await _usersRepository.AddAsync(user);
            _userId = user.Id;
        }

        [Fact]
        public async Task GetByRefreshToken_Should_Return_Session_If_Found()
        {
            // Arrange
            var entity = await CreateSampleEntityAsync();
            await _repo.AddAsync(entity);

            // Act
            var result = await _repo.GetByRefreshToken(entity.RefreshToken);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(entity, opts => opts.WithTimeTolerance());
        }

        [Fact]
        public async Task GetByRefreshToken_Should_Return_Null_If_Not_Found()
        {
            // Act
            var result = await _repo.GetByRefreshToken($"non_existent_token_{Guid.NewGuid()}");

            // Assert
            result.Should().BeNull();
        }
    }
}
