using Posts.Application.Core;
using Posts.Application.Core.Models;
using Posts.Application.Exceptions;
using Posts.Application.Repositories;
using Posts.Application.Rules;
using Posts.Contract.Models.Auth;
using Posts.Domain.Entities;
using Posts.Domain.Shared.Enums;
using Posts.Domain.Utils;

namespace Posts.Application.Services
{
    public class AuthService
    {
        private readonly UsersDomainService _usersDomainService;

        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;
        private readonly IRefreshTokenGenerator _refreshTokenGenerator;
        private readonly IEncryption _encryption;
        private readonly ICurrentUser _currentUser;

        private readonly IUsersRepository _usersRepository;
        private readonly ISessionsRepository _sessionsRepository;

        public AuthService(
            IUsersRepository usersRepository,
            ISessionsRepository sessionsRepository,
            UsersDomainService usersDomainService,
            ICurrentUser currentUser,
            IJwtTokenGenerator jwtTokenGenerator,
            IRefreshTokenGenerator refreshTokenGenerator,
            IEncryption encryption,
            IPasswordHasher passwordHasher)
        {
            _usersDomainService = usersDomainService;

            _usersRepository = usersRepository;
            _sessionsRepository = sessionsRepository;

            _passwordHasher = passwordHasher;
            _jwtTokenGenerator = jwtTokenGenerator;
            _refreshTokenGenerator = refreshTokenGenerator;
            _encryption = encryption;
            _currentUser = currentUser;
        }

        public async Task<SignUpResponseDto> SignUp(SignUpRequestDto dto)
        {
            await Task.WhenAll(
                _usersDomainService.ValidateUsernameIsTaken(dto.Username),
                _usersDomainService.ValidateEmailIsTaken(dto.Email)
            );

            var user = new User
            {
                Username = dto.Username,
                Email = dto.Email,
                Password = _passwordHasher.Hash(dto.Password),
                Role = UserRole.User,
            };

            await _usersRepository.Add(user);

            return new SignUpResponseDto
            {
                User = toDto(user)
            };
        }

        public async Task<SignInResponseDto> SignIn(SignInRequestDto dto)
        {
            var isEmail = Validators.IsEmail(dto.EmailOrUsername);

            var user = isEmail ? await _usersRepository.GetByEmail(dto.EmailOrUsername) 
                               : await _usersRepository.GetByUsername(dto.EmailOrUsername);

            if (user is null || !_passwordHasher.Verify(dto.Password, user.Password))
            {
                throw new InvalidCredentialsException();
            }

            var (accessToken, refreshToken) = GenerateTokens(user);

            Session session = new Session {
                UserId = user.Id,
                AccessToken = _encryption.Encrypt(accessToken),
                RefreshToken = _encryption.Encrypt(refreshToken),
                IsRevoked = false,
                ExpiresAt = dto.RememberMe ? null : DateTime.Now.AddDays(1)
            };

            await _sessionsRepository.Add(session);

            return new SignInResponseDto
            {
                User = toDto(user),
                Tokens = new AuthTokensDto
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken
                }
            };
        }

        public async Task<AuthTokensDto> RefreshToken(AuthTokensDto dto)
        {
            var session = await _sessionsRepository
                .GetByRefreshToken(_encryption.Encrypt(dto.RefreshToken));

            if (session is null)
            {
                throw new InvalidRefreshTokenException("Session not found.");
            }
            if (session.IsRevoked)
            {
                throw new InvalidRefreshTokenException("Session has been revoked.");
            }
            if (session.ExpiresAt is not null && session.ExpiresAt < DateTime.Now)
            {
                throw new InvalidRefreshTokenException("Session is expired.");
            }
            if(_encryption.Encrypt(dto.AccessToken) != session.AccessToken)
            {
                throw new InvalidRefreshTokenException("Invalid session data");
            }

            var user = await _usersRepository.GetById(session.UserId);
            if (user is null) { 
                throw new EntityNotFoundException(typeof(User), session.UserId);
            }

            var (accessToken, refreshToken) = GenerateTokens(user);

            session.AccessToken = _encryption.Encrypt(accessToken);
            session.RefreshToken = _encryption.Encrypt(refreshToken);

            await _sessionsRepository.Update(session);

            return new AuthTokensDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
        }

        public async Task<AuthUserDto> Me()
        {
            var userId = _currentUser.UserId;
            var user = userId is not null ? 
                await _usersRepository.GetById(userId.Value) : 
                null;
            if (user is null) { 
                throw new EntityNotFoundException(typeof(User), userId);
            }
            return toDto(user);
        }

        private (string, string) GenerateTokens(User user)
        {
            var tokenUser = new TokenUser
            {
                Id = user.Id,
                Role = user.Role
            };

            var accessToken = _jwtTokenGenerator.GenerateByUser(tokenUser);
            var refreshToken = _refreshTokenGenerator.Generate(tokenUser);

            return (accessToken, refreshToken);
        }

        private AuthUserDto toDto(User user) => new AuthUserDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Description = user.Description,
            ProfileImageUrl = user.ProfileImageUrl,
        };
    }
}
