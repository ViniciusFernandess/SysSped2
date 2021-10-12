using System.Collections.Generic;

namespace SysSped.Domain.Core.Interfaces
{
    public interface IValidation
    {
        List<RequestResult> Erros { get; set; }

        void AddErro(RequestResult erro);
        void AddErro(List<RequestResult> erros);
        bool IsValid();
    }
}
