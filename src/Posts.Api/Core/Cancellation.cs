using Posts.Application.Core;

namespace Posts.Api.Core
{
    public class Cancellation : ICancellation
    {
        public CancellationToken Token { get; }
        public Cancellation(IHttpContextAccessor httpContextAccessor)
        {
            Token = httpContextAccessor.HttpContext?.RequestAborted ?? CancellationToken.None;
        }
    }
}
