using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace WEB_API.Middlewares
{
    public class JwtMiddleware
    {
        private readonly RequestDelegate _next;

        public JwtMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var token = context.Request.Headers.Authorization.ToString().Replace("Bearer ", "");

            if (!string.IsNullOrEmpty(token))
            {
                // Adiciona o token ao HttpContext.Items para ser acessado por outros middlewares
                context.Items["JwtToken"] = token;
            }

            await _next(context);
        }
    }
}