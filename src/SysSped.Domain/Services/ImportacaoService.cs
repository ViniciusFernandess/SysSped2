using OfficeOpenXml;
using SysSped.Domain.Core;
using SysSped.Domain.Interfaces;

namespace SysSped.Domain.Services
{

    public class ImportacaoService : Validation
    {
        private readonly IImportacaoRepository _importacaoRepo;
        private readonly IExcelService _excelService;

        public ImportacaoService(IImportacaoRepository importacaoRepo, IExcelService excelService)
        {
            _importacaoRepo = importacaoRepo;
            _excelService = excelService;
        }

        public void RenovarBase(ExcelPackage pkg)
        {
            var model = _excelService.ExecutaLeitura(pkg);

            var valido = _excelService.IsValid();
            if (!valido)
            {
                this.AddErro(_excelService.Erros);
                return;
            }


            _importacaoRepo.InativarImportacaoAtual();

            var inativouImportacaoAtual = _importacaoRepo.VerificarSeTemImportacaoAtiva() == false;

            if (!inativouImportacaoAtual)
            {
                this.AddErro(new RequestResult(Resource.NAO_INATIVOU_IMPORTACAO_ATUAL));
                return;
            }

            _importacaoRepo.Importar(model);
        }

        public void AtualizarBase(ExcelPackage pkg)
        {
            var arquivoImportacao = _excelService.ExecutaLeitura(pkg);

            var valido = _excelService.IsValid();
            if (!valido)
            {
                this.AddErro(_excelService.Erros);
                return;
            }

            var temImportacaoAtiva = _importacaoRepo.VerificarSeTemImportacaoAtiva();

            if (!temImportacaoAtiva)
            {
                this.AddErro(new RequestResult(Resource.NAO_TEM_IMPORTACAO_ATIVA));
                return;
            }

            foreach (var rowPlanilha in arquivoImportacao.Rows)
            {
                var rowBD = _importacaoRepo.ObterPorCodItem(rowPlanilha.codigointerno);

                var jaTemRowNoBanco = rowBD != null;

                if (jaTemRowNoBanco)
                    _importacaoRepo.InativarRow(rowBD);

                _importacaoRepo.InserirRowAtualizada(rowPlanilha);
            }

            _importacaoRepo.AtualizarDics();
        }
    }
}
