namespace SysSped.Domain.Core
{
    public class RequestResult
    {
        public string Mensagem { get; set; }

        public RequestResult(string mensagem)
        {
            Mensagem = mensagem;
        }
    }
}
