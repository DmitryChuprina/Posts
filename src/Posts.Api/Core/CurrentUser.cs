using Posts.Application.Core;
using Posts.Application.Core.Models;
using Posts.Domain.Shared.Enums;

namespace Posts.Api.Core
{
    public class CurrentUser : ICurrentUser
    {
        private IHttpContextAccessor _accesor;
        private IJwtTokenGenerator _jwtTokenGenerator;

        private bool _isParsed = false;
        private TokenUser? _tokenUser = null;

        private TokenUser? TokenUser
        {
            get
            {
                if (!_isParsed)
                {
                    _tokenUser = ParseUser();
                    _isParsed = true;
                }
                return _tokenUser!;
            }
        }

        public Guid? UserId { get => TokenUser?.Id; }

        public UserRole? UserRole { get => TokenUser?.Role; }

        public CurrentUser(
            IHttpContextAccessor accesor,
            IJwtTokenGenerator jwtTokenGenerator
        ) {
            _accesor = accesor;
            _jwtTokenGenerator = jwtTokenGenerator;
        }

        private TokenUser? ParseUser()
        {
            var identityUser = _accesor.HttpContext?.User;
            if (identityUser == null) {
                return null;
            }
            return _jwtTokenGenerator.ParseUserByClaims(identityUser);
        }
    }
}
