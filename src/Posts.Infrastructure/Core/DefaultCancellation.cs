using Posts.Application.Core;

namespace Posts.Infrastructure.Core
{
    internal class DefaultCancellation : ICancellation
    {
        public CancellationToken Token { get => CancellationToken.None; }
    }
}
