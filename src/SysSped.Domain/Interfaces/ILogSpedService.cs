using SysSped.Domain.Entities.CorrecaoSped;
using System.Collections.Generic;

namespace SysSped.Domain.Interfaces
{
    public interface ILogSpedService
    {
        string ExtrairRelatorioAlteracoesSped(IEnumerable<Bloco0000> blocos0000, string caminhoPasta);
        string ExtrairRelatorioC100NaoTratado(Sped sped, string caminhoPasta);
    }
}
