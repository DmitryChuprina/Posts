using Microsoft.AspNetCore.Mvc;
using Posts.Application.Core;
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
        private readonly ICurrentUser _currentUser;

        public UsersController(
            UsersService usersService,
            ICurrentUser currentUser
        ) { 
            _usersService = usersService;
            _currentUser = currentUser;
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
    }
}
