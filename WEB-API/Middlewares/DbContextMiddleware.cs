using Microsoft.EntityFrameworkCore;
using WEB_API.Models;

namespace WEB_API.Middlewares
{
    public class DbContextMiddleware
    {
        private readonly RequestDelegate _next;

        public DbContextMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            await _next(context);
        }
    }

    public static class DbContextMiddlewareExtensions
    {
        public static IServiceCollection AddDbContextConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ProdutosUsuariosContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("ProdutosUsuariosDB")));

            return services;
        }

        public static IApplicationBuilder UseDbContextConfiguration(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<DbContextMiddleware>();
        }
    }
}