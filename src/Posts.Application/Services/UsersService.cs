
using Posts.Application.Core;
using Posts.Application.Exceptions;
using Posts.Application.Repositories;
using Posts.Application.Rules;
using Posts.Contract.Models;
using Posts.Contract.Models.Users;
using Posts.Domain.Shared.Enums;
using System.Linq;

namespace Posts.Application.Services
{
    public class UsersService
    {
        private readonly UsersDomainService _userHelper;

        private readonly IUsersRepository _usersRepository;

        private readonly ICurrentUser _currentUser;
        public UsersService(
            IUsersRepository usersRepository,
            UsersDomainService usersHelper,
            ICurrentUser currentUser
        ){
            _usersRepository = usersRepository;
            _userHelper = usersHelper;
            _currentUser = currentUser;
        }

        public async Task<IsTakenDto> EmailIsTaken(EmailIsTakenDto dto)
        {
            var forUserId = GetForUserId(dto);
            return new IsTakenDto
            {
                IsTaken = await _userHelper.EmailIsTaken(dto.Email, forUserId)
            };
        }

        public async Task<IsTakenDto> UsernameIsTaken(UsernameIsTakenDto dto)
        {
            var forUserId = GetForUserId(dto);
            return new IsTakenDto
            {
                IsTaken = await _userHelper.UsernameIsTaken(dto.Username, forUserId)
            };
        }

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
