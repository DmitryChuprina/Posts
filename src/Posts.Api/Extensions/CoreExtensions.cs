using Posts.Api.Core;
using Posts.Application.Core;

namespace Posts.Api.Extensions
{
    public static class CoreExtensions
    {
        public static IServiceCollection AddCore(this IServiceCollection services)
        {
            services.AddHttpContextAccessor();
            services.AddScoped<ICurrentUser, CurrentUser>();
            services.AddScoped<ICancellation, Cancellation>();

            return services;
        }

        public static async Task S3Configure(this WebApplication app)
        {
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;

                var logger = services.GetRequiredService<ILogger<Program>>();

                try
                {
                    var s3 = services.GetRequiredService<IS3Client>();
                    await s3.ConfigureBucketAsync();
                    logger.LogInformation($"S3 configuration successfully completed.");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"An error occurred while configuring S3.");
                }
            }
        }
    }
}
