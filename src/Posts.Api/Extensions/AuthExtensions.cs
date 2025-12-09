using Microsoft.AspNetCore.Authentication.JwtBearer;
using Posts.Infrastructure.Core.Models;

namespace Posts.Api.Extensions
{
    public static class AuthExtensions
    {
        public static IServiceCollection AddAuth(this IServiceCollection services, JwtOptions jwtOpts)
        {
            services
             .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
             .AddJwtBearer(options =>
             {
                 options.TokenValidationParameters = jwtOpts.ValidationParameters;
             });
            return services;
        }
    }
}
