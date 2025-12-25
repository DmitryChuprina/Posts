using FluentAssertions;
using Moq;
using Posts.Application.DomainServices;
using Posts.Application.Exceptions;
using Posts.Application.Repositories;
using Posts.Domain.Entities;
using Posts.Domain.Shared.Enums;

namespace Posts.Application.UnitTests.DomainServices
{
    public class UsersDomainServiceTests
    {
        private readonly Mock<IUsersRepository> _repoMock;
        private readonly UsersDomainService _service;

        public UsersDomainServiceTests()
        {
            _repoMock = new Mock<IUsersRepository>();
            _service = new UsersDomainService(_repoMock.Object);
        }

        private User GenUser(Guid? id = null, string? email = null, string? username = null)
        {
            id = id ?? Guid.NewGuid();
            email = email ?? "random-email@random.com";
            username = username ?? "random_username";

            return new User
            {
                Id = id.Value,
                Email = email,
                Username = username,
                Password = "hashed_password",
                Role = UserRole.User
            };
        }

        [Fact]
        public async Task EmailIsTaken_Should_ReturnFalse_When_EmailNotFound()
        {
            // Arrange
            _repoMock.Setup(x => x.GetByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync((User?)null);

            // Act
            var result = await _service.EmailIsTaken("new@mail.com");

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task EmailIsTaken_Should_ReturnTrue_When_EmailExists_And_ForUserId_IsNull()
        {
            // Arrange
            _repoMock.Setup(x => x.GetByEmailAsync("busy@mail.com"))
                .ReturnsAsync(GenUser());

            // Act
            var result = await _service.EmailIsTaken("busy@mail.com", null);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task EmailIsTaken_Should_ReturnTrue_When_EmailBelongsTo_AnotherUser()
        {
            // Arrange 
            var myId = Guid.NewGuid();
            var otherId = Guid.NewGuid();

            _repoMock.Setup(x => x.GetByEmailAsync("busy@mail.com"))
                .ReturnsAsync(GenUser(id: otherId)); 

            // Act
            var result = await _service.EmailIsTaken("busy@mail.com", forUserId: myId);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task EmailIsTaken_Should_ReturnFalse_When_EmailBelongsTo_SameUser()
        {
            // Arrange 
            var myId = Guid.NewGuid();

            _repoMock.Setup(x => x.GetByEmailAsync("my@mail.com"))
                .ReturnsAsync(GenUser(id: myId));

            // Act
            var result = await _service.EmailIsTaken("my@mail.com", forUserId: myId);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task ValidateEmailIsTaken_Should_Throw_When_Taken()
        {
            // Arrange
            _repoMock.Setup(x => x.GetByEmailAsync("taken@mail.com"))
                .ReturnsAsync(GenUser());

            // Act
            Func<Task> act = async () => await _service.ValidateEmailIsTaken("taken@mail.com");

            // Assert
            await act.Should().ThrowAsync<ValueIsTakenException>();
        }

        [Fact]
        public async Task ValidateEmailIsTaken_Should_NotThrow_When_Free()
        {
            // Arrange
            _repoMock.Setup(x => x.GetByEmailAsync(It.IsAny<string>())).ReturnsAsync((User?)null);

            // Act
            Func<Task> act = async () => await _service.ValidateEmailIsTaken("free@mail.com");

            // Assert
            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task UsernameIsTaken_Should_Call_GetByUsername()
        {
            // Arrange
            var username = "cool_guy";
            _repoMock.Setup(x => x.GetByUsernameAsync(username))
                .ReturnsAsync(GenUser());

            // Act
            var result = await _service.UsernameIsTaken(username);

            // Assert
            result.Should().BeTrue();

            _repoMock.Verify(x => x.GetByUsernameAsync(username), Times.Once);
            _repoMock.Verify(x => x.GetByEmailAsync(It.IsAny<string>()), Times.Never);
        }
    }
}
