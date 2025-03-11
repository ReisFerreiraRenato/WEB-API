using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using WEB_API.Services;
using WEB_API.Utils;

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
            HttpStatusCode statusCode;
            string mensagemErro;

            // Tratamento específico para DbUpdateException
            if (ex is DbUpdateException dbUpdateEx)
            {
                statusCode = HttpStatusCode.InternalServerError;
                mensagemErro = "Erro ao atualizar o banco de dados.";

                // Log mais detalhado para DbUpdateException
                _logger.LogError(dbUpdateEx, "Erro ao atualizar o banco de dados: {Message}", dbUpdateEx.Message);
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var logErroService = scope.ServiceProvider.GetRequiredService<ILogErroService>();
                    await logErroService.LogErroAsync(dbUpdateEx, context.Request.Path, context.Request.Method, await ObterCorpoRequisicao(context), nameof(ErrorHandlerMiddleware));
                }
            }
            // Outros tipos de exceções
            else
            {
                statusCode = HttpStatusCode.InternalServerError;
                mensagemErro = "Ocorreu um erro interno no servidor.";

                // Log para outras exceções
                _logger.LogError(ex, "Erro não tratado: {Message}", ex.Message);
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var logErroService = scope.ServiceProvider.GetRequiredService<ILogErroService>();
                    await logErroService.LogErroAsync(ex, context.Request.Path, context.Request.Method, await ObterCorpoRequisicao(context), nameof(ErrorHandlerMiddleware));
                }
            }

            context.Response.StatusCode = (int)statusCode;
            var response = new { mensagem = mensagemErro };
            var json = JsonSerializer.Serialize(response);

            await context.Response.WriteAsync(json);
        }

        private async Task<string> ObterCorpoRequisicao(HttpContext context)
        {
            if (context.Request.ContentLength > 0)
            {
                context.Request.EnableBuffering();
                context.Request.Body.Position = 0;
                using var reader = new System.IO.StreamReader(context.Request.Body);
                var corpoRequisicao = await reader.ReadToEndAsync();
                context.Request.Body.Position = 0;
                return corpoRequisicao;
            }
            return string.Empty;
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