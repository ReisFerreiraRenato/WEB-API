namespace WEB_API.Models
{
    public class Usuario
    {
        private int _id;
        private string? _email;
        private string? _senha;

        public int ID
        {
            get { return _id; }
            set { _id = value; }
        }

        public string? Email
        {
            get { return _email; }
            set { _email = value; }
        }

        public string? Senha
        {
            get { return _senha; }
            set { _senha = value; }
        }

        // Método para criar um novo usuário com validação
        public static Usuario CriarNovoUsuario(string? email, string? senha)
        {
            if (string.IsNullOrEmpty(email))
            {
                throw new ArgumentException("O e-mail não pode ser vazio.");
            }

            if (string.IsNullOrEmpty(senha))
            {
                throw new ArgumentException("A senha não pode ser vazia.");
            }

            return new Usuario { Email = email, Senha = senha };
        }

        // Método para criar um usuário existente com ID
        public static Usuario CriarUsuarioExistente(int id, string email, string senha)
        {
            return new Usuario { ID = id, Email = email, Senha = senha };
        }
    }
}
