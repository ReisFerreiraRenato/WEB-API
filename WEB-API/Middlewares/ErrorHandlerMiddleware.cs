using System.Net;
using System.Text.Json;
using WEB_API.Services;

namespace WEB_API.Middlewares
{
    public class ErrorHandlerMiddleware(RequestDelegate next, ILogger<ErrorHandlerMiddleware> logger, IServiceScopeFactory serviceScopeFactory)
    {
        private readonly RequestDelegate _next = next;
        private readonly ILogger<ErrorHandlerMiddleware> _logger = logger;
        private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            _logger.LogError(ex, "Erro não tratado: {Message}", ex.Message);

            string enderecoRequisicao = $"{context.Request.Scheme}://{context.Request.Host}{context.Request.Path}{context.Request.QueryString}";
            string metodoRequisicao = context.Request.Method;
            string corpoRequisicao = "";

            if (context.Request.ContentLength > 0)
            {
                context.Request.EnableBuffering();
                context.Request.Body.Position = 0;
                using (var reader = new System.IO.StreamReader(context.Request.Body))
                {
                    corpoRequisicao = await reader.ReadToEndAsync();
                }
                context.Request.Body.Position = 0;
            }

            // Cria um escopo para o serviço scoped
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var logErroService = scope.ServiceProvider.GetRequiredService<ILogErroService>();

                // Registre o erro no banco de dados
                await logErroService.LogErroAsync(ex, enderecoRequisicao, metodoRequisicao, corpoRequisicao, nameof(ErrorHandlerMiddleware));
            }

            var response = new { message = "Ocorreu um erro interno no servidor." };
            var json = JsonSerializer.Serialize(response);

            await context.Response.WriteAsync(json);
        }
    }

    public static class ErrorHandlerMiddlewareExtensions
    {
        public static IApplicationBuilder UseErrorHandler(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ErrorHandlerMiddleware>();
        }
    }
}