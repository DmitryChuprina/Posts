using Microsoft.Extensions.DependencyInjection;
using Posts.Application.DomainServices;
using Posts.Application.DomainServices.Interfaces;
using Posts.Application.Services;

namespace Posts.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<IUsersDomainService, UsersDomainService>();

            services.AddScoped<AuthService>();
            services.AddScoped<UsersService>();
            services.AddScoped<PostsService>();

            return services;
        }

    }
}
