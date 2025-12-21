namespace Posts.Application.Core
{
    public interface ITokenHasher
    {
        string Hash(string rawPassword);
    }
}
