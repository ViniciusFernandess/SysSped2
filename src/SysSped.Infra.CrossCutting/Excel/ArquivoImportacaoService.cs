using OfficeOpenXml;
using SysSped.Domain.Core;
using SysSped.Domain.Entities.Importacao;
using SysSped.Domain.Interfaces;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace SysSped.Infra.CrossCutting.Excel
{
    public class ArquivoImportacaoService : Validation, IExcelService
    {
        #region PROPRIEDADES
        public readonly IReadOnlyCollection<string> CamposPadroes = new List<string> {
            "C.digo interno",
            "Descri..o Cliente",
            "ean",
            "ncm",
            "ncm_ex",
            "cest",
            "Nome do Cliente",
            "icms_cst_entrada",
            "icms_cst_sa.da",
            "icms_al.quota_interna",
            "icms_al.quota_interna_sa.da",
            "icms_al.quota_efetiva_entrada",
            "icms_al.quota_efetiva_sa.da",
            "icms_al.quota_interestadual",
            "icms_al.quota_interestadual_sa.da",
            "icms_redu..o_base_c.lculo",
            "icms_redu..o_base_c.lculo_sa.da",
            "cfop_dentro_estado_entrada",
            "cfop_dentro_estado_sa.da",
            "cfop_fora_estado_entrada",
            "cfop_fora_estado_sa.da",
            "mva_original_atacado",
            "mva_original_ind.stria",
            "mva_original_recalculada",
            "mva_ajustada_interestadual_4",
            "mva_ajustada_interestadual_12",
            "mva_ajustada_interestadual_recalculada",
            "desc_icms",
            "C.digo",
            "descri..o",
            "dt_inicio",
            "dt_fim",
            "legisla..o",
            "pis_cst_entrada",
            "pis_cst_sa.da",
            "pis_al.quota_entrada",
            "pis_al.quota_sa.da",
            "pis_natureza_receita",
            "cofins_cst_entrada",
            "cofins_cst_sa.da",
            "cofins_al.quota_entrada",
            "cofins_al.quota_sa.da",
            "cofins_natureza_receita",
            "ipi_cst_entrada",
            "ipi_cst_sa.da",
            "ipi_al.quota",

        };
        public FileInfo Arquivo { get; private set; }

        #endregion

        public ArquivoImportacaoService()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        public ArquivoImportacao ExecutaLeitura(ExcelPackage pkg)
        {
            Erros = new List<RequestResult>();

            var sheet = ObterArquivoExcel(pkg);
            if (!IsValid()) return null;

            var camposArquivo = ObterTodosCamposCabecalho(sheet);

            foreach (var campo in camposArquivo)
                VerificaSeEhCampoPadrao(campo);
            if (!IsValid()) return null;

            VerificaSeTemTodosOsCamposPadroes(camposArquivo);
            if (!IsValid()) return null;

            var arquivoMapeado = ExecutaMapeamentoCampos(sheet);

            //if (arquivoMapeado.Rows.Count != arquivoMapeado.Rows.Select(x => x.codigointerno.Trim().ToLower()).Distinct().Count())
            //    AddErro(new RequestResult(Resource.ARQUIVO_EXCEL_TEM_CODINTERNO_REPETIDO));

            //if (arquivoMapeado.Rows.Count(x => !string.IsNullOrEmpty(x.ean)) != arquivoMapeado.Rows.Where(x => !string.IsNullOrEmpty(x.ean)).Select(x => x.ean.Trim().ToLower()).Distinct().Count())
            //    AddErro(new RequestResult(Resource.ARQUIVO_EXCEL_TEM_EAN_REPETIDO));

            arquivoMapeado.Rows = arquivoMapeado.Rows.DistinctBy(x => x.codigointerno).ToList();

            return arquivoMapeado;
        }

        private ArquivoImportacao ExecutaMapeamentoCampos(ExcelWorksheet sheet)
        {
            var arquivoImportacao = new ArquivoImportacao();

            var start = sheet.Dimension.Start;
            var end = sheet.Dimension.End;

            for (int row = start.Row + 1; row <= end.Row; row++)
            { // Row by row...
                var rowImportacao = new RowImportacao();

                PrencheRow(sheet, row, start, end, rowImportacao);
                var linhaNaoEstaEmBranco = !string.IsNullOrEmpty(rowImportacao.codigointerno.Trim());

                if (linhaNaoEstaEmBranco)
                    arquivoImportacao.Rows.Add(rowImportacao);
            }

            return arquivoImportacao;
        }

        private void PrencheRow(ExcelWorksheet sheet, int row, ExcelCellAddress start, ExcelCellAddress end, RowImportacao rowImportacao)
        {
            rowImportacao.rowAdress = row.ToString();

            for (int col = start.Column; col <= end.Column; col++)
            { // ... Cell by cell...
                PreenchePropriedade(sheet, row, col, rowImportacao);
            }
        }

        private void PreenchePropriedade(ExcelWorksheet sheet, int row, int col, RowImportacao rowObject)
        {
            var nomeColuna = sheet.Cells[1, col].Value?.ToString() ?? "";
            var valorColuna = sheet.Cells[row, col].Value?.ToString() ?? "";

            if (Regex.IsMatch(nomeColuna.ToLower().Replace(" ", ""), "c.digo interno".ToLower().Replace(" ", "")))
                rowObject.codigointerno = valorColuna;

            else if (Regex.IsMatch(nomeColuna.ToLower().Replace(" ", ""), "Descri..o Cliente".ToLower().Replace(" ", "")))
                rowObject.DescricaoCliente = valorColuna;

            else if (Regex.IsMatch(nomeColuna.ToLower().Replace(" ", ""), "ean".ToLower().Replace(" ", "")))
                rowObject.ean = valorColuna;

            else if (Regex.IsMatch("ncm".ToLower().Replace(" ", ""), nomeColuna.ToLower().Replace(" ", "")))
                rowObject.ncm = valorColuna;

            else if (Regex.IsMatch(nomeColuna.ToLower().Replace(" ", ""), "ncm_ex".ToLower().Replace(" ", "")))
                rowObject.ncm_ex = valorColuna;

            else if (Regex.IsMatch(nomeColuna.ToLower().Replace(" ", ""), "cest".ToLower().Replace(" ", "")))
                rowObject.cest = valorColuna;

            else if (Regex.IsMatch(nomeColuna.ToLower().Replace(" ", ""), "Nome do Cliente".ToLower().Replace(" ", "")))
                rowObject.NomedoCliente = valorColuna;

            else if (Regex.IsMatch(nomeColuna.ToLower().Replace(" ", ""), "icms_cst_entrada".ToLower().Replace(" ", "")))
                rowObject.icms_cst_entrada = valorColuna;

            else if (Regex.IsMatch(nomeColuna.ToLower().Replace(" ", ""), "icms_cst_sa.da".ToLower().Replace(" ", "")))
                rowObject.icms_cst_saida = valorColuna;

            else if (Regex.IsMatch(nomeColuna.ToLower().Replace(" ", ""), "icms_al.quota_interna".ToLower().Replace(" ", "")))
                rowObject.icms_aliquota_interna = valorColuna;

            else if (Regex.IsMatch(nomeColuna.ToLower().Replace(" ", ""), "icms_al.quota_interna_sa.da".ToLower().Replace(" ", "")))
                rowObject.icms_aliquota_interna_saida = valorColuna;

            else if (Regex.IsMatch(nomeColuna.ToLower().Replace(" ", ""), "icms_al.quota_efetiva_entrada".ToLower().Replace(" ", "")))
                rowObject.icms_aliquota_efetiva_entrada = valorColuna;

            else if (Regex.IsMatch(nomeColuna.ToLower().Replace(" ", ""), "icms_al.quota_efetiva_sa.da".ToLower().Replace(" ", "")))
                rowObject.icms_aliquota_efetiva_saida = valorColuna;

            else if (Regex.IsMatch(nomeColuna.ToLower().Replace(" ", ""), "icms_al.quota_interestadual".ToLower().Replace(" ", "")))
                rowObject.icms_aliquota_interestadual = valorColuna;

            else if (Regex.IsMatch(nomeColuna.ToLower().Replace(" ", ""), "icms_al.quota_interestadual_sa.da".ToLower().Replace(" ", "")))
                rowObject.icms_aliquota_interestadual_saida = valorColuna;

            else if (Regex.IsMatch(nomeColuna.ToLower().Replace(" ", ""), "icms_redu..o_base_c.lculo".ToLower().Replace(" ", "")))
                rowObject.icms_reducao_base_calculo = valorColuna;

            else if (Regex.IsMatch(nomeColuna.ToLower().Replace(" ", ""), "icms_redu..o_base_c.lculo_saida".ToLower().Replace(" ", "")))
                rowObject.icms_reducao_base_calculo_saida = valorColuna;

            else if (Regex.IsMatch(nomeColuna.ToLower().Replace(" ", ""), "cfop_dentro_estado_entrada".ToLower().Replace(" ", "")))
                rowObject.cfop_dentro_estado_entrada = valorColuna;

            else if (Regex.IsMatch(nomeColuna.ToLower().Replace(" ", ""), "cfop_dentro_estado_sa.da".ToLower().Replace(" ", "")))
                rowObject.cfop_dentro_estado_saida = valorColuna;

            else if (Regex.IsMatch(nomeColuna.ToLower().Replace(" ", ""), "cfop_fora_estado_entrada".ToLower().Replace(" ", "")))
                rowObject.cfop_fora_estado_entrada = valorColuna;

            else if (Regex.IsMatch(nomeColuna.ToLower().Replace(" ", ""), "cfop_fora_estado_sa.da".ToLower().Replace(" ", "")))
                rowObject.cfop_fora_estado_saida = valorColuna;

            else if (Regex.IsMatch(nomeColuna.ToLower().Replace(" ", ""), "mva_original_atacado".ToLower().Replace(" ", "")))
                rowObject.mva_original_atacado = valorColuna;

            else if (Regex.IsMatch(nomeColuna.ToLower().Replace(" ", ""), "mva_original_ind.stria".ToLower().Replace(" ", "")))
                rowObject.mva_original_industria = valorColuna;

            else if (Regex.IsMatch(nomeColuna.ToLower().Replace(" ", ""), "mva_original_recalculada".ToLower().Replace(" ", "")))
                rowObject.mva_original_recalculada = valorColuna;

            else if (Regex.IsMatch(nomeColuna.ToLower().Replace(" ", ""), "mva_ajustada_interestadual_4".ToLower().Replace(" ", "")))
                rowObject.mva_ajustada_interestadual_4 = valorColuna;

            else if (Regex.IsMatch(nomeColuna.ToLower().Replace(" ", ""), "mva_ajustada_interestadual_12".ToLower().Replace(" ", "")))
                rowObject.mva_ajustada_interestadual_12 = valorColuna;

            else if (Regex.IsMatch(nomeColuna.ToLower().Replace(" ", ""), "mva_ajustada_interestadual_recalculada".ToLower().Replace(" ", "")))
                rowObject.mva_ajustada_interestadual_recalculada = valorColuna;

            else if (Regex.IsMatch(nomeColuna.ToLower().Replace(" ", ""), "desc_icms".ToLower().Replace(" ", "")))
                rowObject.desc_icms = valorColuna;

            else if (Regex.IsMatch(nomeColuna.ToLower().Replace(" ", ""), "c.digo".ToLower().Replace(" ", "")))
                rowObject.codigo = valorColuna;

            else if (Regex.IsMatch(nomeColuna.ToLower().Replace(" ", ""), "descri..o".ToLower().Replace(" ", "")))
                rowObject.descricao = valorColuna;

            else if (Regex.IsMatch(nomeColuna.ToLower().Replace(" ", ""), "dt_inicio".ToLower().Replace(" ", "")))
                rowObject.dt_inicio = valorColuna;

            else if (Regex.IsMatch(nomeColuna.ToLower().Replace(" ", ""), "dt_fim".ToLower().Replace(" ", "")))
                rowObject.dt_fim = valorColuna;

            else if (Regex.IsMatch(nomeColuna.ToLower().Replace(" ", ""), "legisla..o".ToLower().Replace(" ", "")))
                rowObject.legislacao = valorColuna;

            else if (Regex.IsMatch(nomeColuna.ToLower().Replace(" ", ""), "pis_cst_entrada".ToLower().Replace(" ", "")))
                rowObject.pis_cst_entrada = valorColuna;

            else if (Regex.IsMatch(nomeColuna.ToLower().Replace(" ", ""), "pis_cst_sa.da".ToLower().Replace(" ", "")))
                rowObject.pis_cst_saida = valorColuna;

            else if (Regex.IsMatch(nomeColuna.ToLower().Replace(" ", ""), "pis_al.quota_entrada".ToLower().Replace(" ", "")))
                rowObject.pis_aliquota_entrada = valorColuna;

            else if (Regex.IsMatch(nomeColuna.ToLower().Replace(" ", ""), "pis_al.quota_sa.da".ToLower().Replace(" ", "")))
                rowObject.pis_aliquota_saida = valorColuna;

            else if (Regex.IsMatch(nomeColuna.ToLower().Replace(" ", ""), "pis_natureza_receita".ToLower().Replace(" ", "")))
                rowObject.pis_natureza_receita = valorColuna;

            else if (Regex.IsMatch(nomeColuna.ToLower().Replace(" ", ""), "cofins_cst_entrada".ToLower().Replace(" ", "")))
                rowObject.cofins_cst_entrada = valorColuna;

            else if (Regex.IsMatch(nomeColuna.ToLower().Replace(" ", ""), "cofins_cst_sa.da".ToLower().Replace(" ", "")))
                rowObject.cofins_cst_saida = valorColuna;

            else if (Regex.IsMatch(nomeColuna.ToLower().Replace(" ", ""), "cofins_al.quota_entrada".ToLower().Replace(" ", "")))
                rowObject.cofins_aliquota_entrada = valorColuna;

            else if (Regex.IsMatch(nomeColuna.ToLower().Replace(" ", ""), "cofins_al.quota_sa.da".ToLower().Replace(" ", "")))
                rowObject.cofins_aliquota_saida = valorColuna;

            else if (Regex.IsMatch(nomeColuna.ToLower().Replace(" ", ""), "cofins_natureza_receita".ToLower().Replace(" ", "")))
                rowObject.cofins_natureza_receita = valorColuna;

            else if (Regex.IsMatch(nomeColuna.ToLower().Replace(" ", ""), "ipi_cst_entrada".ToLower().Replace(" ", "")))
                rowObject.ipi_cst_entrada = valorColuna;

            else if (Regex.IsMatch(nomeColuna.ToLower().Replace(" ", ""), "ipi_cst_sa.da".ToLower().Replace(" ", "")))
                rowObject.ipi_cst_saida = valorColuna;

            else if (Regex.IsMatch(nomeColuna.ToLower().Replace(" ", ""), "ipi_al.quota".ToLower().Replace(" ", "")))
                rowObject.ipi_aliquota = valorColuna;
            else
                Erros.Add(new RequestResult(Resource.OBJETO_NAO_TEM_PROPRIEDADE_COM_NOME_DA_COLUNA));

        }

        private ExcelWorksheet ObterArquivoExcel(ExcelPackage package)
        {
            //ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            //var package = new ExcelPackage(Arquivo);
            var workbook = package.Workbook;

            var naoTemSheet = workbook.Worksheets.Count == 0;
            if (naoTemSheet)
            {
                Erros.Add(new RequestResult(Resource.ARQUIVO_EXCEL_NULO));
                return null;
            }

            var sheet = workbook.Worksheets[0];
            return sheet;
        }

        private bool VerificaSeEhCampoPadrao(string nomeCampo)
        {
            var ehCampoValido = CamposPadroes.Any(regex => Regex.IsMatch(nomeCampo.ToLower().Replace(" ", ""), regex.ToLower().Replace(" ", "")));

            if (!ehCampoValido)
                Erros.Add(new RequestResult(Resource.CAMPO_INVALIDO_NO_ARQUIVO + ": " + nomeCampo));

            return CamposPadroes.Contains(nomeCampo);
        }

        private List<string> ObterTodosCamposCabecalho(ExcelWorksheet sheet)
        {
            var start = sheet.Dimension.Start;
            var end = sheet.Dimension.End;

            var camposCabecalho = new List<string>();

            for (int col = start.Column; col <= end.Column; col++)
            { // ... Cell by cell...
                var cellValue = sheet.Cells[1, col].Text; // This got me the actual value I needed.

                var temNaLista = camposCabecalho.Any(c => c.ToLower() == cellValue.ToLower());

                if (!string.IsNullOrEmpty(cellValue) && !temNaLista)
                    camposCabecalho.Add(cellValue);
            }

            return camposCabecalho;
        }

        private bool VerificaSeTemTodosOsCamposPadroes(List<string> camposArquivo)
        {
            var arquivoTemTodosCamposPadrao = CamposPadroes.All(x => camposArquivo.Any(regex => Regex.IsMatch(regex.ToLower().Replace(" ", ""), x.ToLower().Replace(" ", ""))));

            var camposFaltantes = CamposPadroes.Where(x => !camposArquivo.Any(regex => Regex.IsMatch(regex.ToLower().Replace(" ", ""), x.ToLower().Replace(" ", ""))));

            if (!arquivoTemTodosCamposPadrao)
                Erros.Add(new RequestResult(Resource.ARQUIVO_ESTA_FALTANDO_CAMPO_PADRAO + ": " + string.Join(", ", camposFaltantes)));

            return arquivoTemTodosCamposPadrao;
        }

    }
}
