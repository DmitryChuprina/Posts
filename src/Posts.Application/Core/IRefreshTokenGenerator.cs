using Posts.Application.Core.Models;

namespace Posts.Application.Core
{
    public interface IRefreshTokenGenerator
    {
        string Generate(TokenUser user);
    }
}
