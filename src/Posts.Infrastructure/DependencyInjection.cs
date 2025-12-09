using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Posts.Application.Core;
using Posts.Application.Repositories;
using Posts.Infrastructure.Core;
using Posts.Infrastructure.Core.Models;
using Posts.Infrastructure.Repositories;

namespace Posts.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            string connectionString,
            string encryptionKey,
            JwtOptions jwtOptions)
        {
            AddCore(services, jwtOptions, encryptionKey);
            AddConnectionFactory(services, connectionString);
            ApplyRepositories(services);
            return services;
        }

        private static void AddCore(IServiceCollection services, JwtOptions jwtOptions, string encryptionKey)
        {
            services.TryAddScoped<ICancellation, DefaultCancellation>();

            services.AddSingleton<IJwtTokenGenerator>((sp) => new JwtTokenGenerator(jwtOptions));
            services.AddSingleton<IEncryption>((sp) => new Encryption(encryptionKey));

            services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();
            services.AddSingleton<IRefreshTokenGenerator, RefreshTokenGenerator>();
        }

        private static void AddConnectionFactory(IServiceCollection services, string connectionString)
        {
            services.AddScoped(sp =>
            {
                var cancellation = sp.GetRequiredService<ICancellation>();
                return new DbConnectionFactory(connectionString, cancellation);
            });
        }

        private static void ApplyRepositories(IServiceCollection services)
        {
            services.AddScoped<IUsersRepository, UsersRepository>();
            services.AddScoped<ISessionsRepository, SessionsRepository>();
        }
    }
}
