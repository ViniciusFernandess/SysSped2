using Moq;
using OfficeOpenXml;
using SysSped.Domain.Core;
using SysSped.Domain.Entities.Importacao;
using SysSped.Domain.Interfaces;
using SysSped.Domain.Services;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace SysSped.DomainTests.Services
{
    public class ImportacaoServiceTests
    {
        private readonly Mock<IExcelService> _mockExcelService;
        private readonly Mock<IImportacaoRepository> _mockRepoImportacao;

        public ImportacaoServiceTests()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            _mockRepoImportacao = new Mock<IImportacaoRepository>();
            _mockExcelService = new Mock<IExcelService>();
        }

        [Fact]
        public void DeveRenovarBaseNoBanco()
        {
            _mockExcelService.Setup(mock => mock.ExecutaLeitura(It.IsAny<ExcelPackage>())).Returns(new ArquivoImportacao());
            _mockExcelService.Setup(mock => mock.IsValid()).Returns(true);
            _mockRepoImportacao.Setup(mock => mock.VerificarSeTemImportacaoAtiva()).Returns(false);

            var servImportacao = new ImportacaoService(_mockRepoImportacao.Object, _mockExcelService.Object);
            servImportacao.RenovarBase(It.IsAny<ExcelPackage>());

            _mockRepoImportacao.Verify(mock => mock.Importar(It.IsAny<ArquivoImportacao>()), times: Times.Once());
        }

        [Fact]
        public void DeveInativarLinhaNoBancoSeEanDoSpedJaConstaNoBanco()
        {
            var arquivoImportado = new ArquivoImportacao { Rows = new List<RowImportacao> { new RowImportacao() } };

            _mockExcelService.Setup(mock => mock.ExecutaLeitura(It.IsAny<ExcelPackage>())).Returns(arquivoImportado);
            _mockExcelService.Setup(mock => mock.IsValid()).Returns(true);
            _mockRepoImportacao.Setup(mock => mock.VerificarSeTemImportacaoAtiva()).Returns(true);
            _mockRepoImportacao.Setup(mock => mock.ObterPorCodItem(It.IsAny<string>())).Returns(arquivoImportado.Rows.First());

            var servImportacao = new ImportacaoService(_mockRepoImportacao.Object, _mockExcelService.Object);
            servImportacao.AtualizarBase(It.IsAny<ExcelPackage>());

            _mockRepoImportacao.Verify(mock => mock.InativarRow(It.IsAny<RowImportacao>()), times: Times.Once());
        }

        [Fact]
        public void NaoDeveRenovarBaseSeTiverErroDoServicoDeLeitura()
        {
            _mockExcelService.Setup(mock => mock.ExecutaLeitura(It.IsAny<ExcelPackage>())).Returns(new ArquivoImportacao());
            _mockExcelService.Setup(mock => mock.IsValid()).Returns(false);
            _mockExcelService.Setup(Mock => Mock.Erros).Returns(new List<RequestResult> { new RequestResult(Resource.ARQUIVO_EXCEL_NULO) });
            _mockRepoImportacao.Setup(mock => mock.VerificarSeTemImportacaoAtiva()).Returns(false);

            var servImportacao = new ImportacaoService(_mockRepoImportacao.Object, _mockExcelService.Object);
            servImportacao.RenovarBase(It.IsAny<ExcelPackage>());

            _mockRepoImportacao.Verify(mock => mock.Importar(It.IsAny<ArquivoImportacao>()), times: Times.Never());
            Assert.NotEmpty(servImportacao.Erros);
            Assert.False(servImportacao.IsValid());
        }

        [Fact]
        public void NaoDeveRenovaBaseSeNaoInativouImportacaoAtual()
        {
            _mockExcelService.Setup(mock => mock.ExecutaLeitura(It.IsAny<ExcelPackage>())).Returns(new ArquivoImportacao());
            _mockExcelService.Setup(mock => mock.IsValid()).Returns(true);
            _mockRepoImportacao.Setup(Mock => Mock.VerificarSeTemImportacaoAtiva()).Returns(true);

            var servImportacao = new ImportacaoService(_mockRepoImportacao.Object, _mockExcelService.Object);
            servImportacao.RenovarBase(It.IsAny<ExcelPackage>());

            _mockRepoImportacao.Verify(mock => mock.Importar(It.IsAny<ArquivoImportacao>()), times: Times.Never());
            Assert.Contains(servImportacao.Erros, (erro) => erro.Mensagem == Resource.NAO_INATIVOU_IMPORTACAO_ATUAL);
        }

        [Fact]
        public void DeveAtualizarBaseNoBanco()
        {
            var arquivoImportado = new ArquivoImportacao { Rows = new List<RowImportacao> { new RowImportacao() } };

            _mockExcelService.Setup(mock => mock.ExecutaLeitura(It.IsAny<ExcelPackage>())).Returns(arquivoImportado);
            _mockExcelService.Setup(mock => mock.IsValid()).Returns(true);
            _mockRepoImportacao.Setup(mock => mock.VerificarSeTemImportacaoAtiva()).Returns(true);

            var servImportacao = new ImportacaoService(_mockRepoImportacao.Object, _mockExcelService.Object);
            servImportacao.AtualizarBase(It.IsAny<ExcelPackage>());

            _mockRepoImportacao.Verify(mock => mock.InserirRowAtualizada(It.IsAny<RowImportacao>()), times: Times.Once());
        }

        [Fact]
        public void NaoDeveAtualizarBaseNoBancoSeTiverErroDoServicoDeLeitura()
        {
            _mockExcelService.Setup(mock => mock.ExecutaLeitura(It.IsAny<ExcelPackage>())).Returns(new ArquivoImportacao());
            _mockExcelService.Setup(mock => mock.IsValid()).Returns(false);
            _mockRepoImportacao.Setup(mock => mock.VerificarSeTemImportacaoAtiva()).Returns(true);
            _mockExcelService.Setup(Mock => Mock.Erros).Returns(new List<RequestResult> { new RequestResult(Resource.ARQUIVO_EXCEL_NULO) });

            var servImportacao = new ImportacaoService(_mockRepoImportacao.Object, _mockExcelService.Object);
            servImportacao.AtualizarBase(It.IsAny<ExcelPackage>());

            _mockRepoImportacao.Verify(mock => mock.InserirRowAtualizada(It.IsAny<RowImportacao>()), times: Times.Never());
            Assert.Contains(servImportacao.Erros, (erro) => erro.Mensagem == Resource.ARQUIVO_EXCEL_NULO);
            Assert.False(servImportacao.IsValid());
        }

        [Fact]
        public void NaoDeveAtualizarBaseNoBancoSeNaoTiverImportacaoAtiva()
        {
            _mockExcelService.Setup(mock => mock.ExecutaLeitura(It.IsAny<ExcelPackage>())).Returns(new ArquivoImportacao());
            _mockExcelService.Setup(mock => mock.IsValid()).Returns(true);
            _mockRepoImportacao.Setup(mock => mock.VerificarSeTemImportacaoAtiva()).Returns(false);

            var servImportacao = new ImportacaoService(_mockRepoImportacao.Object, _mockExcelService.Object);
            servImportacao.AtualizarBase(It.IsAny<ExcelPackage>());

            _mockRepoImportacao.Verify(mock => mock.InserirRowAtualizada(It.IsAny<RowImportacao>()), times: Times.Never());
            Assert.Contains(servImportacao.Erros, (erro) => erro.Mensagem == Resource.NAO_TEM_IMPORTACAO_ATIVA);
        }

    }
}
