using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Posts.Infrastructure.Core.Models;
using System.Net;

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
                 options.Events = new JwtBearerEvents
                 {
                     OnAuthenticationFailed = (context) =>
                     {
                         var logger = context.HttpContext.GetLogger();
                         var tokenIsExpired = context.Exception is SecurityTokenExpiredException;

                         context.NoResult();

                         logger.LogWarning(context.Exception, "JWT authentication failed");

                         return context.HttpContext
                            .WriteError(
                                HttpStatusCode.Unauthorized,
                                "Invalid access token",
                                "InvalidAccessToken",
                                new { tokenIsExpired });
                     },
                     OnChallenge = context =>
                     {
                         context.HandleResponse();

                         return context.HttpContext.WriteError(
                             HttpStatusCode.Unauthorized,
                             "Authentication required",
                             "AuthenticationRequired",
                             null
                         );
                     }
                 };
             });
            return services;
        }

        private static ILogger GetLogger(this HttpContext context)
        {
            return context
              .RequestServices
              .GetRequiredService<ILoggerFactory>()
              .CreateLogger("Auth");
        }
    }
}
