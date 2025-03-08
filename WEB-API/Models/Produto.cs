namespace WEB_API.Models
{
    public class Produto
    {
        private int _id;
        private string? _nome;
        private double _preco;
        private byte[]? _imagem;

        public int Id
        {
            get { return _id; }
            set { _id = value; }
        }

        public string? Nome
        {
            get { return _nome; }
            set { _nome = value; }
        }

        public double Preco
        {
            get { return _preco; }
            set { _preco = value; }
        }

        public byte[]? Imagem
        {
            get { return _imagem; }
            set { _imagem = value; }
        }

        // Método de fábrica para criar um novo produto com validação
        public static Produto CriarNovoProduto(string nome, double preco, byte[] imagem)
        {
            if (string.IsNullOrEmpty(nome))
            {
                throw new ArgumentException("O nome do produto não pode ser vazio.");
            }

            if (preco < 0.50)
            {
                throw new ArgumentException("O preço do produto não pode ser inferior a 0,50.");
            }

            return new Produto { Nome = nome, Preco = preco, Imagem = imagem };
        }

        // Método de fábrica para criar um produto existente com ID
        public static Produto CriarProdutoExistente(int id, string nome, double preco, byte[] imagem)
        {
            return new Produto { Id = id, Nome = nome, Preco = preco, Imagem = imagem };
        }
    }
}
