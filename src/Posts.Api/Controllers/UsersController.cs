using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Posts.Application.Services;
using Posts.Contract.Models;
using Posts.Contract.Models.Users;

namespace Posts.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly UsersService _usersService;

        public UsersController(
            UsersService usersService
        ) { 
            _usersService = usersService;
        }

        [HttpGet("is-taken/email")]
        public Task<IsTakenDto> EmailIsTaken([FromQuery] EmailIsTakenDto dto)
        {
            return _usersService.EmailIsTaken(dto);
        }

        [HttpGet("is-taken/username")]
        public Task<IsTakenDto> UsernameIsTaken([FromQuery] UsernameIsTakenDto dto)
        {
            return _usersService.UsernameIsTaken(dto);
        }

        [HttpGet("profile")]
        [Authorize]
        public Task<UserProfileDto> GetCurrentUserProfile()
        {
            return _usersService.GetCurrentUserProfile();
        }

        [HttpGet("profile/{id}")]
        public Task<UserProfileDto> GetUserProfile([FromRoute] Guid id)
        {
            return _usersService.GetUserProfile(id);
        }

        [HttpPut("profile")]
        [Authorize]
        public Task<UserProfileDto> UpdateCurrentUserProfile([FromBody] UpdateUserProfileDto profile)
        {
            return _usersService.UpdateCurrentUserProfile(profile);
        }

        [HttpGet("security")]
        [Authorize]
        public Task<UserSecurityDto> GetCurrentUserSecurity()
        {
            return _usersService.GetCurrentUserSecurity();
        }

        [HttpPut("security")]
        [Authorize]
        public Task<UserSecurityDto> UpdateCurrentUserSecurity([FromBody] UpdateUserSecurityDto dto)
        {
            return _usersService.UpdateCurrentUserSecurity(dto);
        }
    }
}
