using Posts.Application.Core;
using Posts.Application.Core.Models;
using Posts.Application.Exceptions;
using Posts.Application.Repositories;
using Posts.Contract.Models.Auth;
using Posts.Domain.Entities;
using Posts.Domain.Shared.Enums;
using Posts.Domain.Utils;

namespace Posts.Application.Services
{
    public class AuthService
    {
        private readonly UsersService _usersService;

        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;
        private readonly IRefreshTokenGenerator _refreshTokenGenerator;
        private readonly IEncryption _encryption;

        private readonly IUsersRepository _usersRepository;
        private readonly ISessionsRepository _sessionsRepository;

        public AuthService(
            IUsersRepository usersRepository,
            ISessionsRepository sessionsRepository,
            UsersService usersService,
            IJwtTokenGenerator jwtTokenGenerator,
            IRefreshTokenGenerator refreshTokenGenerator,
            IEncryption encryption,
            IPasswordHasher passwordHasher)
        {
            _usersService = usersService;

            _usersRepository = usersRepository;
            _sessionsRepository = sessionsRepository;

            _passwordHasher = passwordHasher;
            _jwtTokenGenerator = jwtTokenGenerator;
            _refreshTokenGenerator = refreshTokenGenerator;
            _encryption = encryption;
        }

        public async Task<AuthUserDto> SignUp(SignUpRequestDto dto)
        {
            await Task.WhenAll(
                _usersService.ValidateUsernameIsTaken(dto.Username),
                _usersService.ValidateEmailIsTaken(dto.Email)
            );

            var user = new User
            {
                Username = dto.Username,
                Email = dto.Email,
                Password = _passwordHasher.Hash(dto.Password),
                Role = UserRole.User,
            };

            await _usersRepository.Add(user);

            return toDto(user);
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
                IsRevoked = false
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

        public async Task<AuthTokensDto> RefreshToken(AuthTokensDto tokens)
        {
            var session = 
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
