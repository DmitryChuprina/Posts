namespace Posts.Application.Core
{
    public interface ICancellation
    {
        CancellationToken Token { get; }
    }
}
