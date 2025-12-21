
using Posts.Application.Core;
using Posts.Application.DomainServices;
using Posts.Application.Exceptions;
using Posts.Application.Extensions;
using Posts.Application.Repositories;
using Posts.Contract.Models;
using Posts.Contract.Models.Users;
using Posts.Domain.Entities;
using Posts.Domain.Shared.Enums;
using System.Diagnostics;

namespace Posts.Application.Services
{
    public class UsersService
    {
        private readonly UsersDomainService _usersDomainService;

        private readonly IUsersRepository _usersRepository;

        private readonly ICurrentUser _currentUser;

        private readonly IPasswordHasher _passwordHasher;
        private readonly IS3Client _s3Client;

        public UsersService(
            IUsersRepository usersRepository,
            UsersDomainService usersDomainService,
            ICurrentUser currentUser,
            IS3Client s3Client,
            IPasswordHasher passwordHasher
        )
        {
            _usersRepository = usersRepository;
            _usersDomainService = usersDomainService;
            _currentUser = currentUser;
            _s3Client = s3Client;
            _passwordHasher = passwordHasher;
        }

        public async Task<IsTakenDto> EmailIsTaken(EmailIsTakenDto dto)
        {
            var forUserId = GetForUserId(dto);
            return new IsTakenDto
            {
                IsTaken = await _usersDomainService.EmailIsTaken(dto.Email, forUserId)
            };
        }

        public async Task<IsTakenDto> UsernameIsTaken(UsernameIsTakenDto dto)
        {
            var forUserId = GetForUserId(dto);
            return new IsTakenDto
            {
                IsTaken = await _usersDomainService.UsernameIsTaken(dto.Username, forUserId)
            };
        }

        public Task<UserProfileDto> GetCurrentUserProfile()
        {
            var userId = _currentUser.UserId!.Value;
            return GetUserProfile(userId, false);
        }

        public async Task<UserProfileDto> GetUserProfile(
            Guid id,
            bool needAccessCheck = true
        )
        {
            var user = await _usersRepository.GetById(id);
            if (user is null)
            {
                throw new EntityNotFoundException(typeof(User), id.ToString());
            }
            return ToProfileDto(user);
        }

        public Task<UserProfileDto> UpdateCurrentUserProfile(UpdateUserProfileDto dto)
        {
            var userId = _currentUser.UserId!.Value;
            return UpdateUserProfile(userId, dto);
        }

        public async Task<UserProfileDto> UpdateUserProfile(
            Guid id,
            UpdateUserProfileDto dto
        )
        {
            var user = await _usersRepository.GetById(id);
            if (user is null)
            {
                throw new EntityNotFoundException(typeof(User), id.ToString());
            }

            await _usersDomainService.ValidateUsernameIsTaken(dto.Username, id);

            var oldProfileImageKey = user.ProfileImageKey;
            var oldProfileBannerKey = user.ProfileBannerKey;

            var uploads = await Task.WhenAll(
                _s3Client.PersistFileDtoAsync(dto.ProfileImage, "users/images", true),
                _s3Client.PersistFileDtoAsync(dto.ProfileBanner, "users/banners", true)
            );

            user.FirstName = dto.FirstName;
            user.LastName = dto.LastName;
            user.Username = dto.Username;
            user.Description = dto.Description;

            user.ProfileImageKey = uploads[0];
            user.ProfileBannerKey = uploads[1];

            await _usersRepository.Update(user);

            await Task.WhenAll(
                _s3Client.CleanupOldFileAsync(oldProfileImageKey, user.ProfileImageKey),
                _s3Client.CleanupOldFileAsync(oldProfileBannerKey, user.ProfileBannerKey)
            );

            return ToProfileDto(user);
        }

        public async Task<UserSecurityDto> GetCurrentUserSecurity()
        {
            var userId = _currentUser.UserId!.Value;
            var user = await _usersRepository.GetById(userId);
            if (user is null)
            {
                throw new UnreachableException($"Critical: Auth user {userId} not found in DB.");
            }
            return ToSecutiryDto(user);
        }

        public async Task<UserSecurityDto> UpdateCurrentUserSecurity(UpdateUserSecurityDto dto)
        {
            var userId = _currentUser.UserId!.Value;
            var user = await _usersRepository.GetById(userId);
            if (user is null)
            {
                throw new UnreachableException($"Critical: Auth user {userId} not found in DB.");
            }

            var prevEmail = user.Email;

            user.Email = dto.Email;
            user.EmailIsConfirmed = user.EmailIsConfirmed && prevEmail == dto.Email;
            user.Password = dto.Password is not null ? 
                _passwordHasher.Hash(dto.Password) :
                user.Password;

            if (dto.RevokeSessions)
            {
                //TODO: Revoke sessions exlude existed
            }

            return ToSecutiryDto(user);
        }

        private UserSecurityDto ToSecutiryDto(User user) => new UserSecurityDto
        {
            Email = user.Email
        };

        private UserProfileDto ToProfileDto(User user) => new UserProfileDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Username = user.Username,
            Description = user.Description,
            ProfileImage = _s3Client.GetPublicFileDto(user.ProfileImageKey),
            ProfileBanner = _s3Client.GetPublicFileDto(user.ProfileBannerKey)
        };

        private Guid? GetForUserId(UserIsTakenRequestDto dto)
        {
            UserRole[] allowPassUserIdRoles = [UserRole.Admin, UserRole.Moderator];
            bool isAllowPassUserId =
                _currentUser.UserRole is not null &&
                allowPassUserIdRoles.Contains(_currentUser.UserRole.Value);
            if (dto.ForUserId is not null && !isAllowPassUserId)
            {
                throw new ForbiddenException(
                    "Using forUserId allowed only for " +
                    string.Join(',', allowPassUserIdRoles.Select(c => c.ToString())) +
                    " roles"
                );
            }
            return dto.ForUserId ?? _currentUser.UserId;
        }
    }
}
