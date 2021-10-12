using OfficeOpenXml;
using SysSped.Domain.Core;
using SysSped.Infra.CrossCuttingTests.Builders;
using System.Linq;
using Xunit;

namespace SysSped.Infra.CrossCuttingTests.Excel
{
    public class ExcelServiceTests
    {
        #region Validacao Basica Da Classe

        [Fact]
        public void NaoDeveTerListaErrosNula()
        {
            var serv = ExcelServiceBuilder.Create().Build();

            Assert.NotNull(serv.Erros);
        }

        [Fact]
        public void NaoDeveTerListaCamposPadroesVaziaOuNula()
        {
            var serv = ExcelServiceBuilder.Create().Build();

            Assert.NotNull(serv.CamposPadroes);
            Assert.NotEmpty(serv.CamposPadroes);
        }

        [Fact]
        public void DeveTerErroSeTiverAlgumErro()
        {
            var serv = ExcelServiceBuilder.Create().Build();

            serv.Erros.Add(new RequestResult(Resource.ARQUIVO_NAO_ENCONTRADO));

            Assert.False(serv.IsValid());
        }

        #endregion

        #region Validacao Basica Do Arquivo

        [Fact]
        public void NaoDeveTerArquivoExcelNulo()
        {
            var serv = ExcelServiceBuilder.Create().Build();


            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            var package = new ExcelPackage();
            var workbook = package.Workbook;


            serv.ExecutaLeitura(package);
            var temDeErroArquivoExcelNulo = serv.Erros.Any(x => x.Mensagem == Resource.ARQUIVO_EXCEL_NULO);

            Assert.True(temDeErroArquivoExcelNulo);
        }

        #endregion

        #region Validacao de Layout

        [Theory]
        [InlineData("codigointerno")]
        [InlineData("Descrição Cliente")]
        [InlineData("ean")]
        [InlineData("ncm")]
        [InlineData("ncm_ex")]
        [InlineData("cest")]
        [InlineData("Nome do Cliente")]
        [InlineData("icms_cst_entrada")]
        [InlineData("icms_cst_saida")]
        [InlineData("icms_aliquota_interna")]
        [InlineData("icms_aliquota_interna_saida")]
        [InlineData("icms_aliquota_efetiva_entrada")]
        [InlineData("icms_aliquota_efetiva_saida")]
        [InlineData("icms_aliquota_interestadual")]
        [InlineData("icms_aliquota_interestadual_saida")]
        [InlineData("icms_reducao_base_calculo")]
        [InlineData("icms_reducao_base_calculo_saida")]
        [InlineData("cfop_dentro_estado_entrada")]
        [InlineData("cfop_dentro_estado_saida")]
        [InlineData("cfop_fora_estado_entrada")]
        [InlineData("cfop_fora_estado_saida")]
        [InlineData("mva_original_atacado")]
        [InlineData("mva_original_industria")]
        [InlineData("mva_original_recalculada")]
        [InlineData("mva_ajustada_interestadual_4")]
        [InlineData("mva_ajustada_interestadual_12")]
        [InlineData("mva_ajustada_interestadual_recalculada")]
        [InlineData("desc_icms")]
        [InlineData("codigo")]
        [InlineData("descricao")]
        [InlineData("dt_inicio")]
        [InlineData("dt_fim")]
        [InlineData("legislacao")]
        [InlineData("pis_cst_entrada")]
        [InlineData("pis_cst_saida")]
        [InlineData("pis_aliquota_entrada")]
        [InlineData("pis_aliquota_saida")]
        [InlineData("pis_natureza_receita")]
        [InlineData("cofins_cst_entrada")]
        [InlineData("cofins_cst_saida")]
        [InlineData("cofins_aliquota_entrada")]
        [InlineData("cofins_aliquota_saida")]
        [InlineData("cofins_natureza_receita")]
        [InlineData("ipi_cst_entrada")]
        [InlineData("ipi_cst_saida")]
        [InlineData("ipi_aliquota")]
        public void DeveAceitarCampoPadraoNoArquivo(string nomeCampo)
        {
            var serv = ExcelServiceBuilder.Create().Build();
            var package = ArquivoExcelBuilder.Create().ComCabecalho(serv.CamposPadroes).Build();

            package.Workbook.Worksheets[0].Cells["A1"].Value = nomeCampo;

            serv.ExecutaLeitura(package);
            var ehCampoValido = !serv.Erros.Any(x => x.Mensagem == Resource.CAMPO_INVALIDO_NO_ARQUIVO);

            Assert.True(ehCampoValido, @$"Invalid Args: ""{nomeCampo}""");
        }

        [Theory]
        [InlineData("ipi_aliquota2")]
        public void NaoDeveSerValidoArquivoComCampoInvalidoNoCabecalho(string nomeCampo)
        {
            var serv = ExcelServiceBuilder.Create().Build();
            var package = ArquivoExcelBuilder.Create().ComCabecalho(serv.CamposPadroes).Build();

            package.Workbook.Worksheets[0].Cells["A1"].Value = nomeCampo;

            serv.ExecutaLeitura(package);
            var ehCampoInValido = serv.Erros.Any(x => x.Mensagem == Resource.CAMPO_INVALIDO_NO_ARQUIVO);

            Assert.True(ehCampoInValido);
        }

        #endregion

        #region Validacao Preenchimento Layout

        [Fact]
        public void DeveRetornarLayoutImportacaoPreenchido()
        {
            var serv = ExcelServiceBuilder.Create().Build();
            var package = ArquivoExcelBuilder.Create().ComCabecalho(serv.CamposPadroes).ComLinhas().Build();

            var arquivoRetorno = serv.ExecutaLeitura(package);

            Assert.NotNull(arquivoRetorno);
        }

        [Fact]
        public void DeveRemoverLinhasRepetidas()
        {
            var serv = ExcelServiceBuilder.Create().Build();
            var package = ArquivoExcelBuilder.Create().ComCabecalho(serv.CamposPadroes).ComLinhas().Build();

            var arquivoRetorno = serv.ExecutaLeitura(package);

            var temLinhasRepetidas = package.Workbook.Worksheets[0].Cells["A3"].Value == package.Workbook.Worksheets[0].Cells["A4"].Value;
            var removeuLinhasRepetidas = !arquivoRetorno.Rows.GroupBy(x => x.codigointerno).Any(x => x.Count() > 1);

            Assert.True(temLinhasRepetidas);
            Assert.True(removeuLinhasRepetidas);
        }

        [Fact]
        public void DeveTerErroSeArquivoRetornadoTemPropriedadeNaoPreenchida()
        {
            var serv = ExcelServiceBuilder.Create().Build();
            var package = ArquivoExcelBuilder.Create().ComCabecalho(serv.CamposPadroes).ComLinhas().Build();

            var arquivoRetorno = serv.ExecutaLeitura(package);
            bool temPropriedadeNula = serv.Erros.Any(x => x.Mensagem == Resource.OBJETO_NAO_TEM_PROPRIEDADE_COM_NOME_DA_COLUNA);

            Assert.False(temPropriedadeNula);
        }

        #endregion

        [Fact]
        public void DeveSerValidoSeTemTodosOsCamposPadroes()
        {
            var serv = ExcelServiceBuilder.Create().Build();
            var package = ArquivoExcelBuilder.Create().ComCabecalho(serv.CamposPadroes).ComLinhas().Build();

            serv.ExecutaLeitura(package);
            bool temTodosCamposPadroes = !serv.Erros.Any(x => x.Mensagem == Resource.ARQUIVO_ESTA_FALTANDO_CAMPO_PADRAO);

            Assert.True(temTodosCamposPadroes);
        }

        [Fact]
        public void DeveTerErroSeNaoTemTodosOsCamposPadroes()
        {
            var serv = ExcelServiceBuilder.Create().Build();
            var package = ArquivoExcelBuilder.Create().ComCabecalho(serv.CamposPadroes).Build();
            var sheet = package.Workbook.Worksheets[0];

            package.Workbook.Worksheets[0].Cells["A1"].Value = sheet.Cells["A2"].Value;

            serv.ExecutaLeitura(package);
            bool naotemTodosCamposPadroes = serv.Erros.Any(x => x.Mensagem == Resource.ARQUIVO_ESTA_FALTANDO_CAMPO_PADRAO);

            Assert.True(naotemTodosCamposPadroes);
        }

        [Fact]
        public void DeveTerErroSerSeNaoTemTodosOsCamposPadroes()
        {
            var serv = ExcelServiceBuilder.Create().Build();
            var package = ArquivoExcelBuilder.Create().ComCabecalho(serv.CamposPadroes).Build();

            package.Workbook.Worksheets[0].Cells["A1"].Value = "";

            serv.ExecutaLeitura(package);
            bool naoTemTodosCamposPadroes = serv.Erros.Any(x => x.Mensagem == Resource.ARQUIVO_ESTA_FALTANDO_CAMPO_PADRAO);

            Assert.True(naoTemTodosCamposPadroes);
        }
    }
}
