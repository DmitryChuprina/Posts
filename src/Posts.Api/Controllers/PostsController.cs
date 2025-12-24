using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Posts.Application.Services;
using Posts.Contract.Models;
using Posts.Contract.Models.Posts;

namespace Posts.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostsController : ControllerBase
    {
        private readonly PostsService _postsService;

        public PostsController(
            PostsService postsService
        )
        {
            _postsService = postsService;
        }

        [HttpGet("{id}")]
        public Task<PostDto> GetById(Guid id)
        {
            return _postsService.GetById(id);
        }

        [HttpGet("replies/{id}")]
        public Task<PaginationResponseDto<PostDto>> GetPostReplies(Guid id, [FromQuery] PaginationRequestDto dto)
        {
            return _postsService.GetPostReplies(id, dto);
        }

        [HttpGet("user/{id}")]
        public Task<PaginationResponseDto<PostDto>> GetUserPosts(Guid id, [FromQuery] PaginationRequestDto dto)
        {
            return _postsService.GetUserPosts(id, dto);
        }

        [HttpGet("user/{id}/replies")]
        public Task<PaginationResponseDto<PostDto>> GetUserReplies(Guid id, [FromQuery] PaginationRequestDto dto)
        {
            return _postsService.GetUserReplies(id, dto);
        }

        [HttpPost]
        [Authorize]
        public Task<PostDto> Create([FromBody] CreatePostDto dto)
        {
            return _postsService.Create(dto);
        }

        [HttpPut]
        [Authorize]
        public Task<PostDto> Update([FromBody] UpdatePostDto dto)
        {
            return _postsService.Update(dto);
        }

        [HttpDelete("{id}")]
        [Authorize]
        public Task Delete(Guid id)
        {
            return _postsService.Delete(id);
        }
    }
}
