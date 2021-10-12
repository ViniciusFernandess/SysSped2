using SysSped.Domain.Core;
using SysSped.Domain.Entities.CorrecaoSped;
using SysSped.Domain.Entities.Relatorio;
using System.Collections.Generic;

namespace SysSped.Domain.Interfaces
{
    public interface ILogRepository
    {
        void RegistrarLog(Bloco0000 bloco0000, EnumTipoSped tipoBloco, int indiceCampo, string nomeCampo, int indiceLinha, string codigoInterno, string ean, string campoAntigo, string campoNovo, string regra);
        int RegistrarLogBloco0000(Bloco0000 bloco0000);
        IEnumerable<Bloco0000> ObterBloco0000Ativos();
        IEnumerable<Bloco0000> ObterBloco0000Ativos(int idBloco0000);
        IEnumerable<Bloco0000> ObterBloco0000Ativos(IEnumerable<int> idBsloco0000);
        List<AlteracoesSped> ObterAlteracoesPorIdBloco0000(IEnumerable<int> ids);
    }
}
