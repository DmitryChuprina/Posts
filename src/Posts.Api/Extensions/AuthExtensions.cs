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
                         logger.LogWarning(context.Exception, "JWT authentication failed");
                         return Task.CompletedTask;
                     },
                     OnChallenge = context =>
                     {
                         context.HandleResponse();

                         var isInvalidToken = context.AuthenticateFailure != null;

                         return context.HttpContext.WriteError(
                             HttpStatusCode.Unauthorized,
                             isInvalidToken ? "Invalid access token" : "Authentication required",
                             isInvalidToken ? "InvalidAccessToken" : "AuthenticationRequired",
                             isInvalidToken ? 
                                new { tokenIsExpired = context.AuthenticateFailure is SecurityTokenExpiredException } : 
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
