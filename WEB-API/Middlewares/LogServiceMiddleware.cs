using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using WEB_API.Services;

namespace WEB_API.Middlewares
{
    public class LogServiceMiddleware
    {
        private readonly RequestDelegate _next;

        public LogServiceMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            await _next(context);
        }
    }

    public static class LogServiceMiddlewareExtensions
    {
        public static IApplicationBuilder UseLogServiceConfiguration(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<LogServiceMiddleware>();
        }
    }
}