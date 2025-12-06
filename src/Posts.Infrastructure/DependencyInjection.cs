using Dapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Posts.Application.Core;
using Posts.Application.Repositories;
using Posts.Infrastructure.Core;
using Posts.Infrastructure.Repositories;
using Posts.Infrastructure.Utils;
using System.Reflection;

namespace Posts.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
        {
            AddDefaultCancelation(services);
            AddConnectionFactory(services, connectionString);
            ApplyRepositories(services);
            RegisterEnumHandlers();
            return services;
        }

        private static void AddDefaultCancelation(IServiceCollection services)
        {
            services.TryAddScoped<ICancellation, DefaultCancellation>();
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
