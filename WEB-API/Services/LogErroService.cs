using System;
using System.Threading.Tasks;
using WEB_API.Models;

namespace WEB_API.Services
{
    public class LogErroService(ProdutosUsuariosContext context) : ILogErroService
    {
        private readonly ProdutosUsuariosContext _context = context;

        public async Task LogErroAsync(Exception ex, string enderecoRequisicao, string metodoRequisicao, string corpoRequisicao, string nomeControlador)
        {
            var logErro = new LogErro
            {
                DataHora = DateTime.UtcNow,
                Mensagem = ex.Message,
                StackTrace = ex.StackTrace,
                EnderecoRequisicao = enderecoRequisicao,
                MetodoRequisicao = metodoRequisicao,
                CorpoRequisicao = corpoRequisicao,
                NomeControlador = nomeControlador
            };

            _context.LogsErros.Add(logErro);
            await _context.SaveChangesAsync();
        }
    }
}