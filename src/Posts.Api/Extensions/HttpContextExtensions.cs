using System.Diagnostics;
using System.Net;
using System.Text.Json;

namespace Posts.Api.Extensions
{
    public static class HttpContextExtensions
    {
        public static Task WriteError(
            this HttpContext context,
            HttpStatusCode status,
            string message,
            string type,
            object? details
        )
        {
            var response = new
            {
                status = (int)status,
                message,
                details,
                type,
                traceId = context.GenTraceId()
            };

            context.Response.StatusCode = response.status;
            context.Response.ContentType = "application/json";

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            };
            var json = JsonSerializer.Serialize(response, options);

            return context.Response.WriteAsync(json);
        }

        public static string GenTraceId(
            this HttpContext context
        )
        {
            return Activity.Current?.Id ?? context.TraceIdentifier;
        }
    }
}
