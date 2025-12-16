using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Posts.Application.Services;
using Posts.Contract.Models.Auth;

namespace Posts.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthController(
            AuthService authService
        ) {
            _authService = authService;
        }


        [HttpPost("sign-in")]
        public Task<SignInResponseDto> SignIn(
            [FromBody] SignInRequestDto dto
        ) { 
            return _authService.SignIn(dto);
        }

        [HttpPost("sign-up")]
        public Task<SignUpResponseDto> SignUp(
            [FromBody] SignUpRequestDto dto
        ) {
            return _authService.SignUp(dto);
        }

        [HttpPost("refresh-token")]
        public Task<AuthTokensDto> RefreshToken(
            [FromBody] AuthTokensDto dto
        )
        {
            return _authService.RefreshToken(dto);
        }

        [HttpGet("me")]
        [Authorize]
        public Task<AuthUserDto> Me()
        {
            return _authService.Me();
        }
    }
}
