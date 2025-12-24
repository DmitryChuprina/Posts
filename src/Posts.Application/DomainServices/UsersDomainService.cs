using Posts.Application.DomainServices.Interfaces;
using Posts.Application.Exceptions;
using Posts.Application.Repositories;
using Posts.Domain.Entities;

namespace Posts.Application.DomainServices
{
    public class UsersDomainService: IUsersDomainService
    {
        private readonly IUsersRepository _usersRepository;

        public UsersDomainService(IUsersRepository usersRepository) { 
            _usersRepository = usersRepository;
        }

        public Task<bool> EmailIsTaken(string email, Guid? forUserId = null) =>
            IsTaken(() => _usersRepository.GetByEmail(email), forUserId);

        public Task<bool> UsernameIsTaken(string username, Guid? forUserId = null) =>
            IsTaken(() => _usersRepository.GetByUsername(username), forUserId);

        public Task ValidateEmailIsTaken(string email, Guid? forUserId = null) =>
                ValidateIsTaken(() => _usersRepository.GetByEmail(email), "Email", forUserId);
        public Task ValidateUsernameIsTaken(string username, Guid? forUserId = null) =>
               ValidateIsTaken(() => _usersRepository.GetByUsername(username), "Username", forUserId);

        private async Task<bool> IsTaken(Func<Task<User?>> userDelegate, Guid? forUserId = null)
        {
            var user = await userDelegate();
            var isTaken = user is not null && user.Id != forUserId;
            return isTaken;
        }

        private async Task ValidateIsTaken(Func<Task<User?>> userDelegate, string valueName, Guid? forUserId = null)
        {
            var isTaken = await IsTaken(userDelegate, forUserId);
            if (isTaken)
            {
                throw new ValueIsTakenException(typeof(User), valueName, forUserId);
            }
        }
    }
}
