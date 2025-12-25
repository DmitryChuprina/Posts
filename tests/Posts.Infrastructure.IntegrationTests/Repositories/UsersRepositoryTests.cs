using FluentAssertions;
using Posts.Application.Repositories;
using Posts.Domain.Entities;
using Posts.Domain.Shared.Enums;
using Posts.Infrastructure.IntegrationTests.Extensions;
using Posts.Infrastructure.IntegrationTests.Fixtures;
using Posts.Infrastructure.IntegrationTests.Repositories.Base;
using Posts.Infrastructure.Repositories;

namespace Posts.Infrastructure.IntegrationTests.Repositories
{
    [Collection("IntegrationTests")]
    public class UsersRepositoryTests : GenericBaseRepositoryTests<IUsersRepository, User>, IAsyncLifetime
    {
        public UsersRepositoryTests(TestInfrastructure infra) : base(infra)
        {
        }

        protected override IUsersRepository CreateRepository()
        {
            return new UsersRepository(_infra.ConnectionFactory, _userMock.Object);
        }

        protected override async Task<User> CreateSampleEntityAsync()
        {
            return new User
            {
                Username = $"testuser{Guid.NewGuid()}",
                Email = $"testuser{Guid.NewGuid()}@example.com",
                Password = $"Password123!{Guid.NewGuid()}",
                Role = UserRole.User,
            };
        }

        protected override async Task MakeChangesForUpdateAsync(User entity)
        {
            entity.FirstName = "UpdatedFirstName";
            entity.LastName = "UpdatedLastName";
            entity.Description = "Updated description.";
        }

        [Fact]
        public async Task GetByEmail_Should_Return_User_If_Exists()
        {
            // Arrange
            var entity = await CreateSampleEntityAsync();
            await _repo.AddAsync(entity);

            // Act
            var result = await _repo.GetByEmailAsync(entity.Email);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(entity, opts => opts.WithTimeTolerance());
        }

        [Fact]
        public async Task GetByEmail_Should_Return_Null_If_Not_Found()
        {
            // Act
            var result = await _repo.GetByEmailAsync("non_existent@example.com");

            // Assert
            result.Should().BeNull();
        }


        [Fact]
        public async Task GetByUsername_Should_Return_User_If_Exists()
        {
            // Arrange
            var entity = await CreateSampleEntityAsync();
            await _repo.AddAsync(entity);

            // Act
            var result = await _repo.GetByUsernameAsync(entity.Username);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(entity, opts => opts.WithTimeTolerance());
        }

        [Fact]
        public async Task GetByUsername_Should_Return_Null_If_Not_Found()
        {
            // Act
            var result = await _repo.GetByUsernameAsync("unknown_user");

            // Assert
            result.Should().BeNull();
        }
    }
}
