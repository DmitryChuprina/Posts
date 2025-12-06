using Posts.Application.Exceptions;
using Posts.Application.Repositories;
using Posts.Domain.Entities;

namespace Posts.Application.Services
{
    public class UsersService
    {
        private readonly IUsersRepository _usersRepository;
        public UsersService(IUsersRepository usersRepository) { 
            _usersRepository = usersRepository;
        }

        public Task ValidateEmailIsTaken(string email, Guid? forUserId = null) =>
                ValidateIsTaken(() => _usersRepository.GetByEmail(email), "Email", forUserId);
        public Task ValidateUsernameIsTaken(string username, Guid? forUserId = null) =>
               ValidateIsTaken(() => _usersRepository.GetByUsername(username), "Username", forUserId);

        private async Task<bool> IsTaken(Func<Task<User?>> userDelegate, Guid? forUserId = null)
        {
            var user = await userDelegate();
            if (user is not null && user.Id != forUserId)
            {
                return true;
            }
            return false;
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
