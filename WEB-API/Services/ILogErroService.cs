namespace WEB_API.Services
{
    public interface ILogErroService
    {
        Task LogErroAsync(Exception ex, string enderecoRequisicao, string metodoRequisicao, string corpoRequisicao, string nomeControlador);
    }
}