using SysSped.Domain.Entities.Importacao;
using System.Collections.Generic;

namespace SysSped.Domain.Interfaces
{
    public interface IImportacaoRepository
    {
        void Importar(ArquivoImportacao model);
        void InativarImportacaoAtual();
        bool VerificarSeTemImportacaoAtiva();
        void AtualizarDics();

        string ObterCST_PisProdutoPorNcm(string ncm);
        string ObterCST_CofinsProdutoPorNcm(string ncm);
        string ObterAliquotaPisProdutoPorNcm(string ncm);
        string ObterAliquotaCofinsProdutoPorNcm(string ncm);

        string ObterCST_PisProdutoPorEan(string ean);
        string ObterCST_CofinsProdutoPorEan(string ean);
        string ObterAliquotaPisProdutoPorEan(string ean);
        string ObterAliquotaCofinsProdutoPorEan(string ean);

        string ObterCST_PisProdutoPorCod_Item(string codInternoItem);
        string ObterCST_CofinsProdutoPorCod_Item(string codInternoItem);
        string ObterAliquotaPisProdutoPorCod_Item(string codInternoItem);
        string ObterAliquotaCofinsProdutoPorCod_Item(string codInternoItem);

        List<RowImportacao> ObterImportacaoAtiva();
        RowImportacao ObterPorCodItem(string codInternoItem);
        RowImportacao ObterPorEanItem(string eanItem);
        void InativarRow(RowImportacao rowBD);
        void InserirRowAtualizada(RowImportacao rowPlanilha);
    }
}
