using Posts.Api.Extensions;
using Posts.Application.Exceptions;
using System.Net;

namespace Posts.Api.Middlewares
{
    public class ExceptionMiddleware : IMiddleware
    {
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(ILogger<ExceptionMiddleware> logger)
        {
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            var status = ex switch
            {
                UnauthorizedAccessException => HttpStatusCode.Unauthorized,
                InvalidRefreshTokenException => HttpStatusCode.Unauthorized,
                ValidationException => HttpStatusCode.BadRequest,
                InvalidCredentialsException => HttpStatusCode.BadRequest,
                ValueIsTakenException => HttpStatusCode.BadRequest,
                EntityNotFoundException => HttpStatusCode.NotFound,
                ForbiddenException => HttpStatusCode.Forbidden,
                _ => HttpStatusCode.InternalServerError
            };

            var isInternal = status == HttpStatusCode.InternalServerError;
            var message = isInternal ? "Internal server error." : ex.Message;
            var type = isInternal ? "InternalServerError" : ex.GetType().Name;

            Log(ex, context, status, type);

            if (context.Response.HasStarted)
            {
                _logger.LogWarning("Response already started, cannot write error. TraceId: {TraceId}", context.GenTraceId());
                throw ex;
            }

            object? details = ex switch
            {
                EntityNotFoundException nf => new { Entity = nf.EntityType.Name, nf.Key },
                ValueIsTakenException tk => new { Entity = tk.EntityType.Name, tk.EntityKey, tk.PropertyName },
                ValidationException v => v.Errors,
                _ => null
            };

            return context.WriteError(status, message, type, details);
        }

        private void Log(
            Exception ex, 
            HttpContext context, 
            HttpStatusCode status,
            string type
        )
        {
            var traceId = context.GenTraceId();

            if (status == HttpStatusCode.InternalServerError)
            {
                _logger.LogError(ex, "Unhandled exception. TraceId: {TraceId}", traceId);
                return;
            }
            _logger.LogWarning(ex, "Handled exception {ExceptionType}. TraceId: {TraceId}", type, traceId);
        }
    }
}
