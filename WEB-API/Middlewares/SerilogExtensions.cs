using Microsoft.AspNetCore.Builder;
using Serilog;

namespace WEB_API.Middlewares
{
    public static class SerilogExtensions
    {
        public static WebApplicationBuilder AddSerilog(this WebApplicationBuilder builder)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console() // Exemplo: escreve logs no console
                .WriteTo.File("logs/myapp.txt", rollingInterval: RollingInterval.Day) // Exemplo: escreve logs em arquivos
                .CreateLogger();

            builder.Host.UseSerilog();
            return builder;
        }
    }
}