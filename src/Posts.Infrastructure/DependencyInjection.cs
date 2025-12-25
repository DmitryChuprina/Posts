using Amazon.Runtime;
using Amazon.S3;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Posts.Application.Core;
using Posts.Application.Repositories;
using Posts.Infrastructure.Core;
using Posts.Infrastructure.Core.Models;
using Posts.Infrastructure.Extensions;
using Posts.Infrastructure.Interfaces;
using Posts.Infrastructure.Repositories;

namespace Posts.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            DbConnectionOptions connectionOptions,
            EncryptionOptions encryptionOptions,
            JwtOptions jwtOptions,
            S3Options s3Options
        )
        {
            AddOptions(services, connectionOptions, encryptionOptions, jwtOptions, s3Options);
            AddCore(services);
            AddConnectionFactory(services);
            ApplyRepositories(services);
            return services;
        }

        private static void AddOptions(
            this IServiceCollection services,
            DbConnectionOptions dbConnectionOptions,
            EncryptionOptions encryptionOptions,
            JwtOptions jwtOptions,
            S3Options s3Options
        ){
            services.AddSingleton(dbConnectionOptions);
            services.AddSingleton(encryptionOptions);
            services.AddSingleton(jwtOptions);
            services.AddSingleton(s3Options);
        }

        private static void AddCore(
            IServiceCollection services
        ){
            services.TryAddScoped<ICancellation, DefaultCancellation>();

            services.AddSingleton<IAmazonS3>(sp =>
            {
                var options = sp.GetService<S3Options>();
                if (options is null)
                {
                    throw new ArgumentNullException(nameof(options), "S3 configuration is missing");
                }
                return options.CreateAmazonS3Client();
            });

            services.AddKeyedSingleton<IAmazonS3>(DependencyInjectionTokens.S3_CLIENT_SIGNER, (sp, key) =>
            {
                var options = sp.GetService<S3Options>();
                if (options is null)
                {
                    throw new ArgumentNullException(nameof(options), "S3 configuration is missing");
                }
                return options.CreateAmazonS3Client(true);
            });

            services.AddSingleton<IS3Client, S3Client>();
            services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();
            services.AddSingleton<IEncryption, Encryption>();

            services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();
            services.AddSingleton<ITokenHasher, TokenHasher>();
            services.AddSingleton<IRefreshTokenGenerator, RefreshTokenGenerator>();
            services.AddSingleton<IFileOptimizer, FileOptimizer>();
        }

        private static void AddConnectionFactory(IServiceCollection services)
        {
            services.AddScoped<DbConnectionFactory>();
            services.AddScoped<IUnitOfWork>(sp => sp.GetService<DbConnectionFactory>()!);
            services.AddScoped<IDbConnectionFactory>(sp => sp.GetService<DbConnectionFactory>()!);
        }

        private static void ApplyRepositories(IServiceCollection services)
        {
            services.AddScoped<IUsersRepository, UsersRepository>();
            services.AddScoped<ISessionsRepository, SessionsRepository>();
            services.AddScoped<IPostsRepository, PostsRepository>();
            services.AddScoped<IPostLikesRepository, PostLikesRepository>();
            services.AddScoped<IPostMediaRepository, PostMediaRepository>();
            services.AddScoped<IPostViewsRepository, PostViewsRepository>();
            services.AddScoped<ITagsRepository, TagsRepository>();
        }
    }
}
