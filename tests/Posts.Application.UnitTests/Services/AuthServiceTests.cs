using FluentAssertions;
using Moq;
using Posts.Application.Core;
using Posts.Application.Core.Models;
using Posts.Application.DomainServices;
using Posts.Application.DomainServices.Interfaces;
using Posts.Application.Exceptions;
using Posts.Application.Repositories;
using Posts.Application.Services;
using Posts.Contract.Models;
using Posts.Contract.Models.Auth;
using Posts.Domain.Entities;
using Posts.Domain.Shared.Enums;

namespace Posts.Application.UnitTests.Services
{
    public class AuthServiceTests
    {
        private readonly Mock<IUsersRepository> _usersRepoMock;
        private readonly Mock<ISessionsRepository> _sessionsRepoMock;
        private readonly Mock<IUsersDomainService> _usersDomainServiceMock;
        private readonly Mock<ICurrentUser> _currentUserMock;
        private readonly Mock<IJwtTokenGenerator> _jwtGeneratorMock;
        private readonly Mock<IRefreshTokenGenerator> _refreshTokenGeneratorMock;
        private readonly Mock<ITokenHasher> _tokenHasherMock;
        private readonly Mock<IPasswordHasher> _passwordHasherMock;
        private readonly Mock<IS3Client> _s3ClientMock;

        private readonly AuthService _service;

        public AuthServiceTests()
        {
            _usersRepoMock = new Mock<IUsersRepository>();
            _sessionsRepoMock = new Mock<ISessionsRepository>();

            _usersDomainServiceMock = new Mock<IUsersDomainService>();

            _currentUserMock = new Mock<ICurrentUser>();
            _jwtGeneratorMock = new Mock<IJwtTokenGenerator>();
            _refreshTokenGeneratorMock = new Mock<IRefreshTokenGenerator>();
            _tokenHasherMock = new Mock<ITokenHasher>();
            _passwordHasherMock = new Mock<IPasswordHasher>();
            _s3ClientMock = new Mock<IS3Client>();

            _service = new AuthService(
                _usersRepoMock.Object,
                _sessionsRepoMock.Object,
                _usersDomainServiceMock.Object,
                _currentUserMock.Object,
                _jwtGeneratorMock.Object,
                _refreshTokenGeneratorMock.Object,
                _tokenHasherMock.Object,
                _passwordHasherMock.Object,
                _s3ClientMock.Object
            );
        }

        #region SignUp

        [Fact]
        public async Task SignUp_Should_CreateUser_When_ValidationPasses()
        {
            // Arrange
            var req = new SignUpRequestDto { Username = "user", Email = "test@mail.com", Password = "123" };

            _usersDomainServiceMock.Setup(x => x.ValidateUsernameIsTaken(req.Username, null)).Returns(Task.CompletedTask);
            _usersDomainServiceMock.Setup(x => x.ValidateEmailIsTaken(req.Email, null)).Returns(Task.CompletedTask);

            _passwordHasherMock.Setup(x => x.Hash(req.Password)).Returns("hashed_pass");

            // Act
            var result = await _service.SignUp(req);

            // Assert
            result.User.Should().NotBeNull();
            result.User.Username.Should().Be(req.Username);

            _usersRepoMock.Verify(x => x.AddAsync(It.Is<User>(u =>
                u.Username == req.Username &&
                u.Email == req.Email &&
                u.Password == "hashed_pass" &&
                u.Role == UserRole.User
            )), Times.Once);
        }

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
                Role = UserRole.User
            };
        }

        [Fact]
        public async Task SignUp_Should_Throw_When_DomainValidationFails()
        {
            // Arrange
            var req = new SignUpRequestDto { Username = "taken", Email = "test@mail.com", Password = "123" };

            _usersDomainServiceMock.Setup(x => x.ValidateUsernameIsTaken(req.Username, null))
                .ThrowsAsync(new ValueIsTakenException(typeof(User), "Username", null));
            _usersDomainServiceMock.Setup(x => x.ValidateEmailIsTaken(It.IsAny<string>(), null))
                .Returns(Task.CompletedTask);

            // Act
            Func<Task> act = async () => await _service.SignUp(req);

            // Assert
            await act.Should().ThrowAsync<ValueIsTakenException>();
            _usersRepoMock.Verify(x => x.AddAsync(It.IsAny<User>()), Times.Never);
        }

        #endregion

        #region SignIn

        [Fact]
        public async Task SignIn_Should_Authenticate_And_CreateSession_When_CredentialsAreValid()
        {
            // Arrange
            var req = new SignInRequestDto { EmailOrUsername = "test@mail.com", Password = "123", RememberMe = false };
            var user = GenUser(email: "test@mail.com", password: "hashed_password");

            // Setup retrieval by email
            _usersRepoMock.Setup(x => x.GetByEmail(req.EmailOrUsername)).ReturnsAsync(user);

            // Setup password verification
            _passwordHasherMock.Setup(x => x.Verify(req.Password, user.Password)).Returns(true);

            // Setup tokens
            var jwtResult = new JwtTokenGeneratorResult { Token = "access_token", ExpiresAt = DateTime.UtcNow.AddMinutes(15) };
            _jwtGeneratorMock.Setup(x => x.GenerateByUser(It.Is<TokenUser>(t => t.Id == user.Id))).Returns(jwtResult);
            _refreshTokenGeneratorMock.Setup(x => x.Generate(It.IsAny<TokenUser>())).Returns("refresh_token");

            // Setup hashing for storage
            _tokenHasherMock.Setup(x => x.Hash("access_token")).Returns("hashed_access");
            _tokenHasherMock.Setup(x => x.Hash("refresh_token")).Returns("hashed_refresh");

            // Act
            var result = await _service.SignIn(req);

            // Assert
            result.Tokens.AccessToken.Should().Be("access_token");
            result.Tokens.RefreshToken.Should().Be("refresh_token");

            // Verify Session Creation
            _sessionsRepoMock.Verify(x => x.AddAsync(It.Is<Session>(s =>
                s.UserId == user.Id &&
                s.AccessToken == "hashed_access" &&
                s.RefreshToken == "hashed_refresh" &&
                s.ExpiresAt != null // Because RememberMe is false
            )), Times.Once);
        }

        [Fact]
        public async Task SignIn_Should_Throw_InvalidCredentials_When_UserNotFound()
        {
            // Arrange
            _usersRepoMock.Setup(x => x.GetByUsername("unknown")).ReturnsAsync((User?)null);

            var req = new SignInRequestDto { EmailOrUsername = "unknown", Password = "123" };

            // Act
            Func<Task> act = async () => await _service.SignIn(req);

            // Assert
            await act.Should().ThrowAsync<InvalidCredentialsException>();
        }

        [Fact]
        public async Task SignIn_Should_Throw_InvalidCredentials_When_PasswordMismatch()
        {
            // Arrange
            var user = GenUser(password: "correct_hash");
            _usersRepoMock.Setup(x => x.GetByUsername("user")).ReturnsAsync(user);
            _passwordHasherMock.Setup(x => x.Verify("wrong_pass", "correct_hash")).Returns(false);

            var req = new SignInRequestDto { EmailOrUsername = "user", Password = "wrong_pass" };

            // Act
            Func<Task> act = async () => await _service.SignIn(req);

            // Assert
            await act.Should().ThrowAsync<InvalidCredentialsException>();
        }

        #endregion

        #region RefreshToken

        [Fact]
        public async Task RefreshToken_Should_RotateTokens_When_SessionIsValid()
        {
            // Arrange
            var dto = new AuthTokensDto { AccessToken = "old_access", RefreshToken = "old_refresh" };
            var session = new Session
            {
                UserId = Guid.NewGuid(),
                AccessToken = "hashed_old_access",
                RefreshToken = "hashed_old_refresh",
                IsRevoked = false,
                ExpiresAt = DateTime.UtcNow.AddDays(1)
            };
            var user = GenUser(id: session.UserId);

            // Mock Hashing calls
            _tokenHasherMock.Setup(x => x.Hash("old_refresh")).Returns("hashed_old_refresh");
            _tokenHasherMock.Setup(x => x.Hash("old_access")).Returns("hashed_old_access");

            // Mock Session Retrieval
            _sessionsRepoMock.Setup(x => x.GetByRefreshToken("hashed_old_refresh")).ReturnsAsync(session);

            // Mock User Retrieval
            _usersRepoMock.Setup(x => x.GetByIdAsync(session.UserId)).ReturnsAsync(user);

            // Mock New Token Generation
            var newJwt = new JwtTokenGeneratorResult { Token = "new_access", ExpiresAt = DateTime.UtcNow.AddMinutes(15) };
            _jwtGeneratorMock.Setup(x => x.GenerateByUser(It.IsAny<TokenUser>())).Returns(newJwt);
            _refreshTokenGeneratorMock.Setup(x => x.Generate(It.IsAny<TokenUser>())).Returns("new_refresh");

            // Mock New Token Hashing
            _tokenHasherMock.Setup(x => x.Hash("new_access")).Returns("hashed_new_access");
            _tokenHasherMock.Setup(x => x.Hash("new_refresh")).Returns("hashed_new_refresh");

            // Act
            var result = await _service.RefreshToken(dto);

            // Assert
            result.AccessToken.Should().Be("new_access");
            result.RefreshToken.Should().Be("new_refresh");

            // Verify Session Update
            _sessionsRepoMock.Verify(x => x.UpdateAsync(It.Is<Session>(s =>
                s.AccessToken == "hashed_new_access" &&
                s.RefreshToken == "hashed_new_refresh"
            )), Times.Once);
        }

        [Fact]
        public async Task RefreshToken_Should_Throw_When_SessionNotFound()
        {
            // Arrange
            _tokenHasherMock.Setup(x => x.Hash(It.IsAny<string>())).Returns("hash");
            _sessionsRepoMock.Setup(x => x.GetByRefreshToken(It.IsAny<string>())).ReturnsAsync((Session?)null);

            // Act
            Func<Task> act = async () => await _service.RefreshToken(new AuthTokensDto());

            // Assert
            await act.Should().ThrowAsync<InvalidRefreshTokenException>().WithMessage("Session not found.");
        }

        [Fact]
        public async Task RefreshToken_Should_Throw_When_SessionIsRevoked()
        {
            // Arrange
            var session = new Session {
                ExpiresAt = DateTime.UtcNow.AddDays(2),
                IsRevoked = true,
                AccessToken = "random_token",
                RefreshToken = "random_token"
            };
            _tokenHasherMock.Setup(x => x.Hash(It.IsAny<string>())).Returns("hash");
            _sessionsRepoMock.Setup(x => x.GetByRefreshToken(It.IsAny<string>())).ReturnsAsync(session);

            // Act
            Func<Task> act = async () => await _service.RefreshToken(new AuthTokensDto());

            // Assert
            await act.Should().ThrowAsync<InvalidRefreshTokenException>().WithMessage("*revoked*");
        }

        [Fact]
        public async Task RefreshToken_Should_Throw_When_SessionIsExpired()
        {
            // Arrange
            var session = new Session { 
                ExpiresAt = DateTime.UtcNow.AddDays(-1), 
                IsRevoked = false, 
                AccessToken = "random_token",
                RefreshToken = "random_token" 
            };
            _tokenHasherMock.Setup(x => x.Hash(It.IsAny<string>())).Returns("hash");
            _sessionsRepoMock.Setup(x => x.GetByRefreshToken(It.IsAny<string>())).ReturnsAsync(session);

            // Act
            Func<Task> act = async () => await _service.RefreshToken(new AuthTokensDto());

            // Assert
            await act.Should().ThrowAsync<InvalidRefreshTokenException>().WithMessage("*expired*");
        }

        [Fact]
        public async Task RefreshToken_Should_Throw_When_AccessTokenDoesNotMatchSession()
        {
            // Arrange
            var dto = new AuthTokensDto { AccessToken = "wrong_token", RefreshToken = "valid_refresh" };
            var session = new Session
            {
                UserId = Guid.NewGuid(),
                AccessToken = "hashed_correct_token", // Mismatch
                RefreshToken = "hashed_valid_refresh",
                ExpiresAt = DateTime.UtcNow.AddDays(1),
                IsRevoked = false
            };

            _tokenHasherMock.Setup(x => x.Hash("valid_refresh")).Returns("hashed_valid_refresh");
            _tokenHasherMock.Setup(x => x.Hash("wrong_token")).Returns("hashed_wrong_token");

            _sessionsRepoMock.Setup(x => x.GetByRefreshToken("hashed_valid_refresh")).ReturnsAsync(session);

            // Act
            Func<Task> act = async () => await _service.RefreshToken(dto);

            // Assert
            await act.Should().ThrowAsync<InvalidRefreshTokenException>().WithMessage("Invalid session data");
        }

        #endregion

        #region Me

        [Fact]
        public async Task Me_Should_ReturnUser_When_UserExists()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = GenUser(id: userId, username: "test");

            _currentUserMock.Setup(x => x.UserId).Returns(userId);
            _usersRepoMock.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync(user);

            // Act
            var result = await _service.Me();

            // Assert
            result.Id.Should().Be(userId);
            result.Username.Should().Be("test");
        }

        [Fact]
        public async Task Me_Should_Throw_EntityNotFound_When_UserIdIsNull()
        {
            // Arrange
            _currentUserMock.Setup(x => x.UserId).Returns((Guid?)null);

            // Act
            Func<Task> act = async () => await _service.Me();

            // Assert
            await act.Should().ThrowAsync<EntityNotFoundException>();
        }

        [Fact]
        public async Task Me_Should_Throw_EntityNotFound_When_UserDoesNotExistInDb()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _currentUserMock.Setup(x => x.UserId).Returns(userId);
            _usersRepoMock.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync((User?)null);

            // Act
            Func<Task> act = async () => await _service.Me();

            // Assert
            await act.Should().ThrowAsync<EntityNotFoundException>();
        }

        #endregion
    }
}
