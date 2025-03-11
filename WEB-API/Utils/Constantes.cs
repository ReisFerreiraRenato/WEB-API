namespace WEB_API.Utils
{
    public static class Constantes
    {
        //Favor colocar em ordem alfabética
        public const string EmailDesconhecido = "email_desconhecido";

        public const string ErroCriarNovoProduto = "Erro ao criar um novo produto.";
        public const string ErroIdRotaDiferenteCorpo = "O ID do produto na rota não corresponde ao ID no corpo da requisição.";
        public const string ErroInternoRequisicao = "Ocorreu um erro interno ao processar a requisição.";
        public const string ErroListaProdutos = "Erro ao obter a lista de produtos.";
        public const string ErroLogin = "Ocorreu um erro interno ao processar a solicitação de login.";
        public const string ErroLoginCredenciaisInvalidas = "Falha na autenticação. Por favor, verifique suas credenciais.";
        public const string ErroNomeProdutoObrigatorio = "O nome do produto é obrigatório.";
        public const string ErroNomeProduto3Caracteres = "O nome do produto deve ter pelo menos 3 caracteres.";
        public const string ErroPrecoMinimoProduto = "O preço do produto não pode ser inferior a 0,50.";
        public const string ErroProdutoSemNome = "Produto sem Nome";
        
        public const string GetProduto = "GetProduto";

        public const string LoginRealizadoSucesso = "Login realizado com sucesso.";

        public const string ModeloErroObterProduto = "Erro ao obter o produto com ID {0}.";
        public const string ModeloErroAtualizarProduto = "Erro ao atualizar o produto com ID {0}.";
        public const string ModeloErroDeletarProduto = "Erro ao deletar o produto com ID {0}.";

        public const string ProdutoNaoEncontrado = "Produto não encontrado";
    }
}
