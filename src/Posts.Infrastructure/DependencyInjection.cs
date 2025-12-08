using Dapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Posts.Application.Core;
using Posts.Application.Repositories;
using Posts.Infrastructure.Core;
using Posts.Infrastructure.Core.Models;
using Posts.Infrastructure.Repositories;
using Posts.Infrastructure.Utils;
using System.Reflection;

namespace Posts.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services, 
            string connectionString,
            JwtOptions jwtOptions)
        {
            AddCore(services, jwtOptions);
            AddConnectionFactory(services, connectionString);
            ApplyRepositories(services);
            RegisterEnumHandlers();
            return services;
        }

        private static void AddCore(IServiceCollection services, JwtOptions jwtOptions)
        {
            services.TryAddScoped<ICancellation, DefaultCancellation>();

            services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();
            services.AddSingleton<IJwtTokenGenerator>((sp) => new JwtTokenGenerator(jwtOptions));
            services.AddSingleton<IRefreshTokenGenerator, RefreshTokenGenerator>();
            services.AddSingleton<IEncryption, Encryption>();
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

        private static void RegisterEnumHandlers()
        {
            var enums = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => t.IsEnum);

            foreach (var enumType in enums)
            {
                var handlerType = typeof(EnumTypeHandler<>).MakeGenericType(enumType);
                var handler = (SqlMapper.ITypeHandler)Activator.CreateInstance(handlerType)!;

                SqlMapper.AddTypeHandler(enumType, handler);
            }
        }
    }
}
