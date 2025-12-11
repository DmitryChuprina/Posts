using Posts.Application.Exceptions;
using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Net;
using System.Text.Json;

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
            var traceId = Activity.Current?.Id ?? context.TraceIdentifier;

            _logger.LogError(ex, "Unhandled exception. TraceId: {TraceId}", traceId);

            var status = ex switch
            {
                UnauthorizedAccessException => HttpStatusCode.Unauthorized,
                ArgumentException => HttpStatusCode.BadRequest,
                ValidationException => HttpStatusCode.BadRequest,
                InvalidCredentialsException => HttpStatusCode.BadRequest,
                InvalidRefreshTokenException => HttpStatusCode.BadRequest,
                ValueIsTakenException => HttpStatusCode.BadRequest,
                EntityNotFoundException => HttpStatusCode.NotFound,
                _ => HttpStatusCode.InternalServerError
            };

            object? details = ex switch
            {
                EntityNotFoundException nf => new { Entity = nf.EntityType.Name, nf.Key },
                ValueIsTakenException tk => new { Entity = tk.EntityType.Name, tk.EntityKey, tk.PropertyName },
                _ => null
            };

            var isInternal = status == HttpStatusCode.InternalServerError;
            var response = new
            {
                Status = (int)status,
                Message = isInternal ? "Internal server error." : ex.Message,
                TraceId = traceId,
                Details = details,
                Type = isInternal ? "InternalServerError" : ex.GetType().Name
            };

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = response.Status;

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            };
            var json = JsonSerializer.Serialize(response, options);

            return context.Response.WriteAsync(json);
        }
    }
}
