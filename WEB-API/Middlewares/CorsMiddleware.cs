namespace WEB_API.Middlewares
{
    public class CorsMiddleware
    {
        private readonly RequestDelegate _next;

        public CorsMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            await _next(context);
        }
    }

    public static class CorsMiddlewareExtensions
    {
        public static IServiceCollection AddCorsConfiguration(this IServiceCollection services) // Modifique aqui
        {
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(builder =>
                {
                    builder.AllowAnyOrigin()
                           .AllowAnyMethod()
                           .AllowAnyHeader();
                });
            });
            return services; // Adicione esta linha
        }

        public static IApplicationBuilder UseCorsConfiguration(this IApplicationBuilder builder)
        {
            return builder.UseCors();
        }
    }
}