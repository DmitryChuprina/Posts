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
    }
}
