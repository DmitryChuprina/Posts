using Microsoft.Extensions.DependencyInjection;
using Posts.Application.DomainServices;
using Posts.Application.Services;

namespace Posts.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<UsersDomainService>();

            services.AddScoped<AuthService>();
            services.AddScoped<UsersService>();
            services.AddScoped<PostsService>();

            return services;
        }

    }
}
