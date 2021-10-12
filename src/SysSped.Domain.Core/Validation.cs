using SysSped.Domain.Core.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace SysSped.Domain.Core
{
    public abstract class Validation : IValidation
    {
        public List<RequestResult> Erros { get; set; } = new List<RequestResult>();

        public void AddErro(string erro)
        {
            AddErro(new RequestResult(erro));
        }

        public void AddErro(RequestResult erro)
        {
            Erros.Add(erro);
        }

        public void AddErro(List<RequestResult> erros)
        {
            Erros.AddRange(erros);
        }

        public bool IsValid()
        {
            return !Erros.Any();
        }
    }
}
