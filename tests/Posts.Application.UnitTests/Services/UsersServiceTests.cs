using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Posts.Application.Core;
using Posts.Application.DomainServices.Interfaces;
using Posts.Application.Exceptions;
using Posts.Application.Repositories;
using Posts.Application.Services;
using Posts.Contract.Models;
using Posts.Contract.Models.Users;
using Posts.Domain.Entities;
using Posts.Domain.Shared.Enums;
using System.Diagnostics;

namespace Posts.Application.UnitTests.Services
{
    public class UsersServiceTests
    {
        private readonly Mock<IUsersRepository> _repoMock;
        private readonly Mock<IUsersDomainService> _domainServiceMock;
        private readonly Mock<ICurrentUser> _currentUserMock;
        private readonly Mock<IS3Client> _s3ClientMock;
        private readonly Mock<IPasswordHasher> _passwordHasherMock;
        protected readonly Mock<ILogger<UsersService>> _loggerMock = new();

        private readonly UsersService _service;

        public UsersServiceTests()
        {
            _repoMock = new Mock<IUsersRepository>();
            _domainServiceMock = new Mock<IUsersDomainService>();
            _currentUserMock = new Mock<ICurrentUser>();
            _s3ClientMock = new Mock<IS3Client>();
            _passwordHasherMock = new Mock<IPasswordHasher>();

            _service = new UsersService(
                _repoMock.Object,
                _domainServiceMock.Object,
                _currentUserMock.Object,
                _s3ClientMock.Object,
                _passwordHasherMock.Object,
                _loggerMock.Object
            );
        }

        // Helper Method
        private User GenUser(Guid? id = null, string? email = null, string? username = null, string? password = null)
        {
            id = id ?? Guid.NewGuid();
            email = email ?? "random-email@random.com";
            username = username ?? "random_username";
            password = password ?? "random_password";

            return new User
            {
                Id = id.Value,
                Email = email,
                Username = username,
                Password = password,
                Role = UserRole.User,
                ProfileImageKey = "default_image",
                ProfileBannerKey = "default_banner"
            };
        }

        #region IsTaken & GetForUserId Logic

        [Fact]
        public async Task EmailIsTaken_Should_UseCurrentUserId_When_ForUserIdIsNull()
        {
            // Arrange
            var userId = Guid.NewGuid();
            SetupCurrentUser(userId, UserRole.User);

            var dto = new EmailIsTakenDto { Email = "test@mail.com", ForUserId = null };

            _domainServiceMock.Setup(x => x.EmailIsTaken(dto.Email, userId))
                .ReturnsAsync(true);

            // Act
            var result = await _service.EmailIsTaken(dto);

            // Assert
            result.IsTaken.Should().BeTrue();
            _domainServiceMock.Verify(x => x.EmailIsTaken(dto.Email, userId), Times.Once);
        }

        [Fact]
        public async Task EmailIsTaken_Should_Allow_Admin_ToUse_ForUserId()
        {
            // Arrange
            var adminId = Guid.NewGuid();
            var targetUserId = Guid.NewGuid();
            SetupCurrentUser(adminId, UserRole.Admin); // Admin Role

            var dto = new EmailIsTakenDto { Email = "test@mail.com", ForUserId = targetUserId };

            _domainServiceMock.Setup(x => x.EmailIsTaken(dto.Email, targetUserId))
                .ReturnsAsync(false);

            // Act
            var result = await _service.EmailIsTaken(dto);

            // Assert
            result.IsTaken.Should().BeFalse();
            _domainServiceMock.Verify(x => x.EmailIsTaken(dto.Email, targetUserId), Times.Once);
        }

        [Fact]
        public async Task EmailIsTaken_Should_Throw_Forbidden_When_RegularUser_Uses_ForUserId()
        {
            // Arrange
            var userId = Guid.NewGuid();
            SetupCurrentUser(userId, UserRole.User); // Regular User

            var dto = new EmailIsTakenDto { Email = "test@mail.com", ForUserId = Guid.NewGuid() };

            // Act
            Func<Task> act = async () => await _service.EmailIsTaken(dto);

            // Assert
            await act.Should().ThrowAsync<ForbiddenException>();
        }

        #endregion

        #region GetUserProfile

        [Fact]
        public async Task GetUserProfile_Should_ReturnDto_When_UserExists()
        {
            // Arrange
            var user = GenUser();
            _repoMock.Setup(x => x.GetByIdAsync(user.Id)).ReturnsAsync(user);

            _s3ClientMock.Setup(x => x.GetPublicUrl(It.IsAny<string>()))
                .Returns("http://s3.url");

            // Act
            var result = await _service.GetUserProfile(user.Id);

            // Assert
            result.Id.Should().Be(user.Id);
            result.Username.Should().Be(user.Username);
        }

        [Fact]
        public async Task GetUserProfile_Should_Throw_EntityNotFound_When_UserMissing()
        {
            // Arrange
            _repoMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((User?)null);

            // Act
            Func<Task> act = async () => await _service.GetUserProfile(Guid.NewGuid());

            // Assert
            await act.Should().ThrowAsync<EntityNotFoundException>();
        }

        #endregion

        #region UpdateUserProfile (New Logic)

        [Fact]
        public async Task UpdateUserProfile_Should_UpdateDB_And_ReturnDto_OnSuccess()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = GenUser(id: userId, username: "old_name");

            var dto = new UpdateUserProfileDto
            {
                Username = "new_name",
                FirstName = "First",
                ProfileImage = new FileDto { Key = "new.jpg" },
                ProfileBanner = new FileDto { Key = "banner.jpg" }
            };

            _repoMock.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync(user);
            _domainServiceMock.Setup(x => x.ValidateUsernameIsTaken(dto.Username, userId)).Returns(Task.CompletedTask);

            // S3 Uploads
            _s3ClientMock.Setup(x => x.PersistFileAsync(dto.ProfileImage.Key, "users/images", true)).ReturnsAsync("new_img_key");
            _s3ClientMock.Setup(x => x.PersistFileAsync(dto.ProfileBanner.Key, "users/banners", true)).ReturnsAsync("new_banner_key");

            // Act
            var result = await _service.UpdateUserProfile(userId, dto);

            // Assert
            _repoMock.Verify(x => x.UpdateAsync(It.Is<User>(u =>
                u.Username == "new_name" &&
                u.ProfileImageKey == "new_img_key"
            )), Times.Once);

            result.Username.Should().Be("new_name");
        }

        [Fact]
        public async Task UpdateUserProfile_Should_Cleanup_NewFiles_When_DbUpdateFails()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = GenUser(id: userId);
            var dto = new UpdateUserProfileDto
            {
                Username = "test",
                ProfileImage = new FileDto { Key = "new.jpg" },
                ProfileBanner = new FileDto { Key = "new_banner.jpg" }
            };

            _repoMock.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync(user);

            // Mock S3 success
            _s3ClientMock.Setup(x => x.PersistFileAsync(dto.ProfileImage.Key, It.IsAny<string>(), true))
                .ReturnsAsync("uploaded_image_key");
            _s3ClientMock.Setup(x => x.PersistFileAsync(dto.ProfileBanner.Key, It.IsAny<string>(), true))
                .ReturnsAsync("uploaded_banner_key");

            // Mock DB Failure
            _repoMock.Setup(x => x.UpdateAsync(It.IsAny<User>()))
                .ThrowsAsync(new Exception("Database connection lost"));

            // Act
            Func<Task> act = async () => await _service.UpdateUserProfile(userId, dto);

            // Assert
            await act.Should().ThrowAsync<Exception>().WithMessage("Database connection lost");

            // Wait briefly for Task.Run (fire-and-forget) to execute
            await Task.Delay(100);

            // Verify that DeleteFileAsync was called for the NEW keys
            _s3ClientMock.Verify(x => x.DeleteFileAsync("uploaded_image_key"), Times.Once);
            _s3ClientMock.Verify(x => x.DeleteFileAsync("uploaded_banner_key"), Times.Once);
        }

        [Fact]
        public async Task UpdateUserProfile_Should_Throw_EntityNotFound_When_UserNull()
        {
            // Arrange
            _repoMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((User?)null);

            // Act
            Func<Task> act = async () => await _service.UpdateUserProfile(Guid.NewGuid(), new UpdateUserProfileDto());

            // Assert
            await act.Should().ThrowAsync<EntityNotFoundException>();
        }

        #endregion

        #region Security Methods

        [Fact]
        public async Task GetCurrentUserSecurity_Should_Throw_Unreachable_When_UserMissing()
        {
            // Arrange
            var userId = Guid.NewGuid();
            SetupCurrentUser(userId);
            _repoMock.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync((User?)null);

            // Act
            Func<Task> act = async () => await _service.GetCurrentUserSecurity();

            // Assert
            await act.Should().ThrowAsync<UnreachableException>();
        }

        [Fact]
        public async Task UpdateCurrentUserSecurity_Should_ResetConfirmed_When_EmailChanged()
        {
            // Arrange
            var userId = Guid.NewGuid();
            SetupCurrentUser(userId);

            var user = GenUser(id: userId, email: "old@mail.com");
            user.EmailIsConfirmed = true;

            _repoMock.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync(user);

            var dto = new UpdateUserSecurityDto { Email = "new@mail.com" };

            // Act
            await _service.UpdateCurrentUserSecurity(dto);

            // Assert
            _repoMock.Verify(x => x.UpdateAsync(It.Is<User>(u =>
                u.Email == "new@mail.com" &&
                u.EmailIsConfirmed == false
            )), Times.Once);
        }

        [Fact]
        public async Task UpdateCurrentUserSecurity_Should_HashPassword_IfProvided()
        {
            // Arrange
            var userId = Guid.NewGuid();
            SetupCurrentUser(userId);
            var user = GenUser(id: userId, password: "old_hash");
            _repoMock.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync(user);

            _passwordHasherMock.Setup(x => x.Hash("new_pass")).Returns("new_hash");

            var dto = new UpdateUserSecurityDto { Email = user.Email, Password = "new_pass" };

            // Act
            await _service.UpdateCurrentUserSecurity(dto);

            // Assert
            _repoMock.Verify(x => x.UpdateAsync(It.Is<User>(u => u.Password == "new_hash")), Times.Once);
        }

        #endregion

        private void SetupCurrentUser(Guid id, UserRole role = UserRole.User)
        {
            _currentUserMock.Setup(x => x.UserId).Returns(id);
            _currentUserMock.Setup(x => x.UserRole).Returns(role);
        }
    }
}
