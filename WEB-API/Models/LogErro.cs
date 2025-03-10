using System;

namespace WEB_API.Models
{
    public class LogErro
    {
        private int _id;
        private DateTime _dataHora;
        private string? _mensagem;
        private string? _stackTrace;
        private string? _enderecoRequisicao;
        private string? _metodoRequisicao;
        private string? _corpoRequisicao;
        private string? _nomeControlador;

        public int Id
        {
            get { return _id; }
            set { _id = value; }
        }

        public DateTime DataHora
        {
            get { return _dataHora; }
            set { _dataHora = value; }
        }

        public string? Mensagem
        {
            get { return _mensagem; }
            set { _mensagem = value; }
        }

        public string? StackTrace
        {
            get { return _stackTrace; }
            set { _stackTrace = value; }
        }

        public string? EnderecoRequisicao
        {
            get { return _enderecoRequisicao; }
            set { _enderecoRequisicao = value; }
        }

        public string? MetodoRequisicao
        {
            get { return _metodoRequisicao; }
            set { _metodoRequisicao = value; }
        }

        public string? CorpoRequisicao
        {
            get { return _corpoRequisicao; }
            set { _corpoRequisicao = value; }
        }
        public string? NomeControlador
        {
            get { return _nomeControlador; }
            set { _nomeControlador = value; }
        }
    }
}