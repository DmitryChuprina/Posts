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

        public static async Task S3ConfigureCleanUp(this WebApplication app)
        {
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;

                var logger = services.GetRequiredService<ILogger<Program>>();

                try
                {
                    var cleanupService = services.GetRequiredService<IS3Client>();
                    await cleanupService.ConfigureCleanupAsync();

                    logger.LogInformation("S3 cleanup configuration successfully completed.");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred while configuring clean up S3.");
                }
            }
        }
    }
}
