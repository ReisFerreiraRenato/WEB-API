using WEB_API.Utils;

namespace WEB_API.Utils
{
    public static class Funcoes
    {
        public static string ObterMensagemErroAtualizarProduto(int id)
        {
            return string.Format(Constantes.ModeloErroAtualizarProduto, id);
        }
        public static string ObterMensagemErroDeletarProduto(int id)
        {
            return string.Format(Constantes.ModeloErroDeletarProduto, id);
        }
        public static string ObterMensagemErroObterProduto(int id)
        {
            return string.Format(Constantes.ModeloErroObterProduto, id);
        }
    }
}
