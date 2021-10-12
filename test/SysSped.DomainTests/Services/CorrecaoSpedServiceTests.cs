using Bogus;
using Moq;
using SysSped.Domain.Core;
using SysSped.Domain.Entities.CorrecaoSped;
using SysSped.Domain.Interfaces;
using SysSped.Domain.Services;
using SysSped.Infra.CrossCutting.Txt;
using System.Linq;
using Xunit;

namespace SysSped.DomainTests.Services
{
    public class CorrecaoSpedServiceTests
    {
        private readonly Mock<IImportacaoRepository> _mockRepoImportacao;
        private readonly Mock<ILogRepository> _mockLogRepository;
        private readonly ITxtService _txtService;

        public CorrecaoSpedServiceTests()
        {
            _mockRepoImportacao = new Mock<IImportacaoRepository>();
            _mockLogRepository = new Mock<ILogRepository>();
            _txtService = new TxtService();

        }


        [Theory]
        [InlineData("50", "73")]
        [InlineData("50", "70")]
        [InlineData("70", "50")]
        [InlineData("70", "73")]
        [InlineData("73", "50")]
        [InlineData("73", "70")]
        public void DeveInserirCST_PIS_TratadoConformeOriginal(string cstPisOriginal, string cstPisEsperado)
        {
            _mockRepoImportacao.Setup(mock => mock.ObterCST_PisProdutoPorCod_Item(It.IsAny<string>())).Returns(cstPisEsperado);

            var sped = new Sped();
            var c100 = new C100();
            var c170 = new C170();
            var c170_ = new C170();

            c170.CST_PIS = cstPisOriginal;
            c170_.CST_PIS = cstPisOriginal;
            c170.COD_ITEM = "18045";
            c170_.COD_ITEM = "18045";

            sped.BlocosC100.Add(c100);
            c100.BlocosC170.Add(c170);
            c100.BlocosC170.Add(c170_);

            var serv = new CorrecaoSpedService(_mockRepoImportacao.Object, _mockLogRepository.Object);
            serv.TratarSped(sped, new string[] { });

            var blocos170ComCST_PIS_Original = sped.BlocosC100.First().BlocosC170.Where(bloco => bloco.CST_PIS == cstPisOriginal);
            var trocouTodosPisParaEsperado = blocos170ComCST_PIS_Original.All(bloco => bloco.CST_PIS_TRATADO == cstPisEsperado);
            var todosC170CairamNaRegra = sped.BlocosC100.First().BlocosC170.All(c170 => c170.Tratado);

            Assert.NotEmpty(blocos170ComCST_PIS_Original);
            Assert.True(todosC170CairamNaRegra);
            Assert.True(trocouTodosPisParaEsperado);
        }

        [Theory]
        [InlineData("50", "73")]
        [InlineData("50", "70")]
        [InlineData("70", "50")]
        [InlineData("70", "73")]
        [InlineData("73", "50")]
        [InlineData("73", "70")]
        public void DeveInserirCST_Cofins_TratadoConformeOriginal(string cstCofinsOriginal, string cstCofinsEsperado)
        {
            _mockRepoImportacao.Setup(mock => mock.ObterCST_CofinsProdutoPorCod_Item(It.IsAny<string>())).Returns(cstCofinsEsperado);

            var sped = new Sped();
            var c100 = new C100();
            var c170 = new C170();
            var c170_ = new C170();

            c170.COD_ITEM = "18045";
            c170.CST_COFINS = cstCofinsOriginal;
            c170_.COD_ITEM = "18045";
            c170_.CST_COFINS = cstCofinsOriginal;

            sped.BlocosC100.Add(c100);
            c100.BlocosC170.Add(c170);
            c100.BlocosC170.Add(c170_);

            var serv = new CorrecaoSpedService(_mockRepoImportacao.Object, _mockLogRepository.Object);
            serv.TratarSped(sped, new string[] { });

            var blocos170ComCST_Original = sped.BlocosC100.First().BlocosC170.Where(bloco => bloco.CST_COFINS == cstCofinsOriginal);
            var trocouTodosCofnsParaEsperado = blocos170ComCST_Original.All(bloco => bloco.CST_COFINS_TRATADO == cstCofinsEsperado);
            var todosC170CairamNaRegra = sped.BlocosC100.First().BlocosC170.All(c170 => c170.Tratado);

            Assert.True(todosC170CairamNaRegra);
            Assert.NotEmpty(blocos170ComCST_Original);
            Assert.True(trocouTodosCofnsParaEsperado);
        }

        [Theory]
        [InlineData("50", "73")]
        [InlineData("50", "70")]
        [InlineData("70", "50")]
        [InlineData("70", "73")]
        [InlineData("73", "50")]
        [InlineData("73", "70")]
        public void DeveLogarAoCairEmQualquerRegraECorrigirArquivo(string cstOriginal, string cstEsperado)
        {
            var arquivoSpedOriginal = new string[] {
                $@"|C100|0|1|36968395000168|55|00|1|119|33200736968395000168550010000001191176609464|11072020|11072020|514,65|0|0|0|514,65|9|0|0|0|283,88|0|0|0|0|0|0|0|0|",
                $@"|C170|1|90118|CONSERTO|7|UND|3507,05|0|1|540|1403||3507,05|0|0|0|0|0|0|52|||||{cstOriginal}|3507,05|0|||0|{cstOriginal}|3507,05|0|||0|3.02.1.1.00002|",
                $@"|C170|2|90571|CONSERTO|11|UND|945,6|0|1|540|1403||945,6|0|0|0|0|0|0|52|||||{cstOriginal}|945,6|0|||0|{cstOriginal}|945,6|0|||0|3.02.1.1.00002|",
            };

            _mockRepoImportacao.Setup(mock => mock.ObterCST_PisProdutoPorCod_Item(It.IsAny<string>())).Returns(cstEsperado);
            _mockRepoImportacao.Setup(mock => mock.ObterCST_CofinsProdutoPorCod_Item(It.IsAny<string>())).Returns(cstEsperado);

            var sped = _txtService.ExecutaLeitura(arquivoSpedOriginal);

            var serv = new CorrecaoSpedService(_mockRepoImportacao.Object, _mockLogRepository.Object);
            serv.TratarSped(sped, arquivoSpedOriginal);

            var primeiroC170 = sped.BlocosC100[0].BlocosC170[0];
            var segundoC170 = sped.BlocosC100[0].BlocosC170[1];

            _mockLogRepository.Verify(mock => mock.RegistrarLog(sped.Bloco0000, EnumTipoSped.C170, 24, "CST_PIS_TRATADO", primeiroC170.IndiceArquivo, primeiroC170.COD_ITEM, primeiroC170.COD_NAT, cstOriginal, cstEsperado));
            _mockLogRepository.Verify(mock => mock.RegistrarLog(sped.Bloco0000, EnumTipoSped.C170, 30, "CST_COFINS_TRATADO", segundoC170.IndiceArquivo, segundoC170.COD_ITEM, primeiroC170.COD_NAT, cstOriginal, cstEsperado));

        }


        [Theory]
        [InlineData("1556")]
        [InlineData("2556")]
        [InlineData("1551")]
        [InlineData("2551")]
        [InlineData("1407")]
        [InlineData("2407")]
        [InlineData("1406")]
        [InlineData("1910")]
        [InlineData("2910")]
        [InlineData("1653")]
        public void DeveLogarAoCairNaRegraDoCFOP(string cfop)
        {
            string cstOriginal = new Faker().Random.Number(1, 97).ToString();
            string cstEsperado = "98";

            var arquivoSpedOriginal = new string[] {
                $@"|C100|0|1|36968395000168|55|00|1|119|33200736968395000168550010000001191176609464|11072020|11072020|514,65|0|0|0|514,65|9|0|0|0|283,88|0|0|0|0|0|0|0|0|",
                $@"|C170|1|90118|CONSERTO|7|UND|3507,05|0|1|540|{cfop}||3507,05|0|0|0|0|0|0|52|||||{cstOriginal}|3507,05|0|||0|{cstOriginal}|3507,05|0|||0|3.02.1.1.00002|",
                $@"|C170|2|90571|CONSERTO|11|UND|945,6|0|1|540|{cfop}||945,6|0|0|0|0|0|0|52|||||{cstOriginal}|945,6|0|||0|{cstOriginal}|945,6|0|||0|3.02.1.1.00002|",
            };

            _mockRepoImportacao.Setup(mock => mock.ObterCST_PisProdutoPorCod_Item(It.IsAny<string>())).Returns(cstEsperado);
            _mockRepoImportacao.Setup(mock => mock.ObterCST_CofinsProdutoPorCod_Item(It.IsAny<string>())).Returns(cstEsperado);

            var sped = _txtService.ExecutaLeitura(arquivoSpedOriginal);

            var serv = new CorrecaoSpedService(_mockRepoImportacao.Object, _mockLogRepository.Object);
            serv.TratarSped(sped, arquivoSpedOriginal);

            var primeiroC170 = sped.BlocosC100[0].BlocosC170[0];
            var segundoC170 = sped.BlocosC100[0].BlocosC170[1];

            _mockLogRepository.Verify(mock => mock.RegistrarLog(sped.Bloco0000, EnumTipoSped.C170, 24, "CST_PIS_TRATADO", primeiroC170.IndiceArquivo, primeiroC170.COD_ITEM, primeiroC170.COD_NAT, cstOriginal, cstEsperado));
            _mockLogRepository.Verify(mock => mock.RegistrarLog(sped.Bloco0000, EnumTipoSped.C170, 30, "CST_COFINS_TRATADO", segundoC170.IndiceArquivo, segundoC170.COD_ITEM, primeiroC170.COD_NAT, cstOriginal, cstEsperado));
        }



        [Fact]
        public void DeveRecalcularC100()
        {
            var arquivoSpedOriginal = new string[] {
                $@"|C100|0|1|36968395000168|55|00|1|119|33200736968395000168550010000001191176609464|11072020|11072020|514,65|0|0|0|514,65|9|0|0|0|283,88|0|0|0|0|12|13|0|0|",
                $@"|C170|1|95183|CONSERTO|1|UND|230,74|0|1|540|1556||0|0|0|0|0|0|0|52|||||73|230,74|0|||5|73|230,74|0|||0|3.02.1.1.00002|",
                $@"|C170|2|90571|CONSERTO|5|UND|283,88|0|1||1102||283,88|0|0|0|0|0|0|52|||||73|283,88|0|||0|73|283,88|0|||0|3.02.1.1.00002|"
            };

            var arquivoSpedTratado = new string[] {
                $@"|C100|0|1|36968395000168|55|00|1|119|33200736968395000168550010000001191176609464|11072020|11072020|514,65|0|0|0|514,65|9|0|0|0|283,88|0|0|0|0|12|13|0|0|",
                $@"|C170|1|95183|CONSERTO|1|UND|230,74|0|1|540|1556||0|0|0|0|0|0|0|52|||||73|230,74|0|||5|73|230,74|0|||0|3.02.1.1.00002|",
                $@"|C170|2|90571|CONSERTO|5|UND|283,88|0|1||1102||283,88|0|0|0|0|0|0|52|||||73|283,88|0|||0|73|283,88|0|||0|3.02.1.1.00002|"
            };

            var spedOriginal = _txtService.ExecutaLeitura(arquivoSpedOriginal);
            var spedTratado = _txtService.ExecutaLeitura(arquivoSpedTratado);

            var serv = new CorrecaoSpedService(_mockRepoImportacao.Object, _mockLogRepository.Object);
            serv.TratarSped(spedTratado, arquivoSpedTratado);

            var tratouSped = spedOriginal.BlocosC100.First().BlocosC170.FirstOrDefault().VL_PIS != spedTratado.BlocosC100.First().BlocosC170.FirstOrDefault().VL_PIS;

            var c100 = spedTratado.BlocosC100.FirstOrDefault();
            var c170s = spedTratado.BlocosC100.First().BlocosC170;

            var c100_VL_DOC = decimal.Parse(c100.VL_DOC);
            var c170_VL_ITEM = c170s.Sum(x => decimal.Parse(x.VL_ITEM));

            var c100_VL_PIS = decimal.Parse(c100.VL_PIS);
            var c170_VL_PIS = c170s.Sum(x => decimal.Parse(x.VL_PIS));

            var c100_VL_COFINS = decimal.Parse(c100.VL_COFINS);
            var c170_VL_COFINS = c170s.Sum(x => decimal.Parse(x.VL_COFINS));


            Assert.True(tratouSped);
            Assert.Equal(c100_VL_DOC, c170_VL_ITEM);
            Assert.Equal(c100_VL_PIS, c170_VL_PIS);
            Assert.Equal(c100_VL_COFINS, c170_VL_COFINS);
        }

        [Fact]
        public void NaoDeveCorrigirC100NaLinhaNoArquivoSpedSeNaoCairEmNenhumaRegra()
        {
            var arquivoSpedTratado = new string[] {
                $@"|C100|0|1|36968395000168|55|00|1|119|33200736968395000168550010000001191176609464|11072020|11072020|514,62|0|0|0|514,62|9|0|0|0|283,88|0|0|0|0|0|0|0|0|",
                $@"|C170|1|95183|CONSERTO|1|UND|230,74|0|1|540|1102||0|0|0|0|0|0|0|52|||||73|55,88|5|||0|73|55,88|5|||0|3.02.1.1.00002|",
                $@"|C170|2|90571|CONSERTO|5|UND|283,88|0|1||1102||283,88|0|0|0|0|0|0|52|||||73|283,88|0|||0|73|283,88|0|||0|3.02.1.1.00002|",
            };

            var arquivoSpedOriginal = new string[] {
                $@"|C100|0|1|36968395000168|55|00|1|119|33200736968395000168550010000001191176609464|11072020|11072020|514,62|0|0|0|514,62|9|0|0|0|283,88|0|0|0|0|0|0|0|0|",
                $@"|C170|1|95183|CONSERTO|1|UND|230,74|0|1|540|1102||0|0|0|0|0|0|0|52|||||73|55,88|5|||0|73|55,88|5|||0|3.02.1.1.00002|",
                $@"|C170|2|90571|CONSERTO|5|UND|283,88|0|1||1102||283,88|0|0|0|0|0|0|52|||||73|283,88|0|||0|73|283,88|0|||0|3.02.1.1.00002|",
            };

            var spedTratado = _txtService.ExecutaLeitura(arquivoSpedTratado);

            var serv = new CorrecaoSpedService(_mockRepoImportacao.Object, _mockLogRepository.Object);
            serv.TratarSped(spedTratado, arquivoSpedTratado);

            var temC100Tratado = spedTratado.BlocosC100.First().BlocosC170.Any(c170 => c170.Tratado);

            Assert.False(temC100Tratado);
            Assert.Equal(arquivoSpedTratado, arquivoSpedOriginal);
        }

        [Fact]
        public void DeveCorrigirC100NaLinhaNoArquivoSped()
        {
            var arquivoSpedTratado = new string[] {
                $@"|C100|0|1|36968395000168|55|00|1|119|33200736968395000168550010000001191176609464|11072020|11072020|514,62|0|0|0|514,62|9|0|0|0|283,88|0|0|0|0|12|13|0|0|",
                $@"|C170|1|95183|CONSERTO|1|UND|230,74|0|1|540|1556||0|0|0|0|0|0|0|52|||||73|230,74|0|||5|73|230,74|0|||0|3.02.1.1.00002|",
                $@"|C170|2|90571|CONSERTO|5|UND|283,88|0|1||1102||283,88|0|0|0|0|0|0|52|||||73|283,88|0|||0|73|283,88|0|||0|3.02.1.1.00002|"
            };

            var spedTratado = _txtService.ExecutaLeitura(arquivoSpedTratado);

            var serv = new CorrecaoSpedService(_mockRepoImportacao.Object, _mockLogRepository.Object);
            serv.TratarSped(spedTratado, arquivoSpedTratado);

            var arquivoSpedOriginal = new string[] {
                $@"|C100|0|1|36968395000168|55|00|1|119|33200736968395000168550010000001191176609464|11072020|11072020|514,62|0|0|0|514,62|9|0|0|0|283,88|0|0|0|0|0|0|0|0|",
                $@"|C170|1|95183|CONSERTO|1|UND|230,74|0|1|540|1556||0|0|0|0|0|0|0|52|||||73|230,74|0|||5|73|230,74|0|||0|3.02.1.1.00002|",
                $@"|C170|2|90571|CONSERTO|5|UND|283,88|0|1||1102||283,88|0|0|0|0|0|0|52|||||73|283,88|0|||0|73|283,88|0|||0|3.02.1.1.00002|"
            };

            var spedOriginal = _txtService.ExecutaLeitura(arquivoSpedOriginal);

            var tratouSped = spedOriginal.BlocosC100.First().BlocosC170.FirstOrDefault().VL_PIS != spedTratado.BlocosC100.First().BlocosC170.FirstOrDefault().VL_PIS;

            Assert.True(tratouSped);
            Assert.Equal(arquivoSpedTratado[0], arquivoSpedOriginal[0]);
        }

        [Fact]
        public void SeCSTFor50para73DeveManterBC()
        {
            var cstOriginal = "50";
            var cstEsperado = "73";
            var bcPisOriginal = "110";
            var bcCofinsOriginal = "150";

            _mockRepoImportacao.Setup(mock => mock.ObterCST_PisProdutoPorCod_Item(It.IsAny<string>())).Returns(cstEsperado);
            _mockRepoImportacao.Setup(mock => mock.ObterCST_CofinsProdutoPorCod_Item(It.IsAny<string>())).Returns(cstEsperado);

            var sped = new Sped();
            var c100 = new C100();
            var c170 = new C170();

            c170.CST_PIS = cstOriginal;
            c170.CST_COFINS = cstOriginal;
            c170.VL_BC_PIS = bcPisOriginal;
            c170.VL_BC_COFINS = bcCofinsOriginal;

            sped.BlocosC100.Add(c100);
            c100.BlocosC170.Add(c170);

            var serv = new CorrecaoSpedService(_mockRepoImportacao.Object, _mockLogRepository.Object);
            serv.TratarSped(sped, new string[] { });

            var mateveBC_Original = sped.BlocosC100.First().BlocosC170.All(bloco =>
                bloco.VL_BC_PIS == bcPisOriginal
                && bloco.VL_BC_COFINS == bcCofinsOriginal
            );

            Assert.True(mateveBC_Original);
        }

        [Fact]
        public void SeCSTFor50para73DeveZerarAliquota_e_VLPisCofins()
        {
            var cstOriginal = "50";
            var cstEsperado = "73";

            _mockRepoImportacao.Setup(mock => mock.ObterCST_PisProdutoPorCod_Item(It.IsAny<string>())).Returns(cstEsperado);
            _mockRepoImportacao.Setup(mock => mock.ObterCST_CofinsProdutoPorCod_Item(It.IsAny<string>())).Returns(cstEsperado);

            var sped = new Sped();
            var c100 = new C100();
            var c170 = new C170();

            c170.CST_PIS = cstOriginal;
            c170.CST_COFINS = cstOriginal;
            c170.VL_PIS = "110";
            c170.ALIQ_PIS = "130";
            c170.VL_COFINS = "130";
            c170.ALIQ_COFINS = "130";
            c170.COD_ITEM = "18045";

            sped.BlocosC100.Add(c100);
            c100.BlocosC170.Add(c170);

            var serv = new CorrecaoSpedService(_mockRepoImportacao.Object, _mockLogRepository.Object);
            serv.TratarSped(sped, new string[] { });

            var zerouAliquotaPercent = sped.BlocosC100.First().BlocosC170.All(bloco =>
                bloco.ALIQ_PIS == "0"
                && bloco.ALIQ_COFINS == "0"
            );

            var zerouValores = sped.BlocosC100.First().BlocosC170.All(bloco =>
                bloco.VL_PIS == "0"
                && bloco.VL_COFINS == "0"
            );

            Assert.True(zerouAliquotaPercent);
            Assert.True(zerouValores);
        }

        [Fact]
        public void SeCSTFor50para70DeveZerarBC()
        {
            var cstOriginal = "50";
            var cstEsperado = "70";
            var bcPisOriginal = "110";
            var bcCofinsOriginal = "150";

            _mockRepoImportacao.Setup(mock => mock.ObterCST_PisProdutoPorCod_Item(It.IsAny<string>())).Returns(cstEsperado);
            _mockRepoImportacao.Setup(mock => mock.ObterCST_CofinsProdutoPorCod_Item(It.IsAny<string>())).Returns(cstEsperado);

            var sped = new Sped();
            var c100 = new C100();
            var c170 = new C170();

            c170.CST_PIS = cstOriginal;
            c170.CST_COFINS = cstOriginal;
            c170.VL_BC_PIS = bcPisOriginal;
            c170.VL_BC_COFINS = bcCofinsOriginal;
            c170.COD_ITEM = "18045";

            c100.BlocosC170.Add(c170);
            sped.BlocosC100.Add(c100);

            var serv = new CorrecaoSpedService(_mockRepoImportacao.Object, _mockLogRepository.Object);
            serv.TratarSped(sped, new string[] { });

            var zerouBC = sped.BlocosC100.First().BlocosC170.All(bloco =>
                bloco.VL_BC_PIS == "0"
                && bloco.VL_BC_COFINS == "0"
            );

            Assert.True(zerouBC);
        }

        [Fact]
        public void SeCSTFor50para70DeveZerarAliquota_e_VLPisCofins()
        {
            var cstOriginal = "50";
            var cstEsperado = "70";

            _mockRepoImportacao.Setup(mock => mock.ObterCST_PisProdutoPorCod_Item(It.IsAny<string>())).Returns(cstEsperado);
            _mockRepoImportacao.Setup(mock => mock.ObterCST_CofinsProdutoPorCod_Item(It.IsAny<string>())).Returns(cstEsperado);

            var sped = new Sped();
            var c100 = new C100();
            var c170 = new C170();

            c170.CST_PIS = cstOriginal;
            c170.CST_COFINS = cstOriginal;
            c170.VL_PIS = "110";
            c170.ALIQ_PIS = "130";
            c170.VL_COFINS = "130";
            c170.ALIQ_COFINS = "130";
            c170.COD_ITEM = "18045";

            c100.BlocosC170.Add(c170);
            sped.BlocosC100.Add(c100);

            var serv = new CorrecaoSpedService(_mockRepoImportacao.Object, _mockLogRepository.Object);
            serv.TratarSped(sped, new string[] { });

            var zerouAliquotaPercent = sped.BlocosC100.First().BlocosC170.All(bloco =>
                bloco.ALIQ_PIS == "0"
                && bloco.ALIQ_COFINS == "0"
            );

            var zerouValores = sped.BlocosC100.First().BlocosC170.All(bloco =>
                bloco.VL_PIS == "0"
                && bloco.VL_COFINS == "0"
            );

            Assert.True(zerouAliquotaPercent);
            Assert.True(zerouValores);
        }

        [Fact]
        public void SeCSTFor73para70DeveZerarBC()
        {
            var cstOriginal = "73";
            var cstEsperado = "70";
            var bcPisOriginal = "110";
            var bcCofinsOriginal = "150";

            _mockRepoImportacao.Setup(mock => mock.ObterCST_PisProdutoPorCod_Item(It.IsAny<string>())).Returns(cstEsperado);
            _mockRepoImportacao.Setup(mock => mock.ObterCST_CofinsProdutoPorCod_Item(It.IsAny<string>())).Returns(cstEsperado);

            var sped = new Sped();
            var c100 = new C100();
            var c170 = new C170();

            c170.CST_PIS = cstOriginal;
            c170.CST_COFINS = cstOriginal;
            c170.VL_BC_PIS = bcPisOriginal;
            c170.VL_BC_COFINS = bcCofinsOriginal;
            c170.COD_ITEM = "18045";

            c100.BlocosC170.Add(c170);
            sped.BlocosC100.Add(c100);

            var serv = new CorrecaoSpedService(_mockRepoImportacao.Object, _mockLogRepository.Object);
            serv.TratarSped(sped, new string[] { });

            var zerouBC = sped.BlocosC100.First().BlocosC170.All(bloco =>
                bloco.VL_BC_PIS == "0"
                && bloco.VL_BC_COFINS == "0"
            );

            Assert.True(zerouBC);
        }

        [Fact]
        public void SeCSTFor73para70DeveZerarAliquota_e_VLPisCofins()
        {
            var cstOriginal = "73";
            var cstEsperado = "70";

            _mockRepoImportacao.Setup(mock => mock.ObterCST_PisProdutoPorCod_Item(It.IsAny<string>())).Returns(cstEsperado);
            _mockRepoImportacao.Setup(mock => mock.ObterCST_CofinsProdutoPorCod_Item(It.IsAny<string>())).Returns(cstEsperado);

            var sped = new Sped();
            var c100 = new C100();
            var c170 = new C170();

            c170.CST_PIS = cstOriginal;
            c170.CST_COFINS = cstOriginal;
            c170.VL_PIS = "110";
            c170.ALIQ_PIS = "130";
            c170.VL_COFINS = "130";
            c170.ALIQ_COFINS = "130";
            c170.COD_ITEM = "18045";

            c100.BlocosC170.Add(c170);
            sped.BlocosC100.Add(c100);

            var serv = new CorrecaoSpedService(_mockRepoImportacao.Object, _mockLogRepository.Object);
            serv.TratarSped(sped, new string[] { });

            var zerouAliquotaPercent = sped.BlocosC100.First().BlocosC170.All(bloco =>
                bloco.ALIQ_PIS == "0"
                && bloco.ALIQ_COFINS == "0"
            );

            var zerouValores = sped.BlocosC100.First().BlocosC170.All(bloco =>
                bloco.VL_PIS == "0"
                && bloco.VL_COFINS == "0"
            );

            Assert.True(zerouAliquotaPercent);
            Assert.True(zerouValores);
        }

        [Fact]
        public void SeCSTFor73para50DeveCopiarAliqDaPlanilhaParaSped()
        {
            var cstOriginal = "73";
            var cstEsperado = "50";
            var aliqPisEsperado = new Faker().Random.Double(1, 25);
            var aliqCofinsEsperado = new Faker().Random.Double(1, 25);

            _mockRepoImportacao.Setup(mock => mock.ObterCST_PisProdutoPorCod_Item(It.IsAny<string>())).Returns(cstEsperado);
            _mockRepoImportacao.Setup(mock => mock.ObterCST_CofinsProdutoPorCod_Item(It.IsAny<string>())).Returns(cstEsperado);
            _mockRepoImportacao.Setup(mock => mock.ObterAliquotaPisProdutoPorCod_Item(It.IsAny<string>())).Returns(aliqPisEsperado.ToString());
            _mockRepoImportacao.Setup(mock => mock.ObterAliquotaCofinsProdutoPorCod_Item(It.IsAny<string>())).Returns(aliqCofinsEsperado.ToString());

            var sped = new Sped();
            var c100 = new C100();
            var c170 = new C170();

            c170.CST_PIS = cstOriginal;
            c170.CST_COFINS = cstOriginal;
            c170.ALIQ_PIS = "3";
            c170.ALIQ_COFINS = "4";
            c170.COD_ITEM = "18045";

            c100.BlocosC170.Add(c170);
            sped.BlocosC100.Add(c100);

            var serv = new CorrecaoSpedService(_mockRepoImportacao.Object, _mockLogRepository.Object);
            serv.TratarSped(sped, new string[] { });

            var aliqPisTratado = sped.BlocosC100.First().BlocosC170.FirstOrDefault().ALIQ_PIS;
            var aliqCofinsTratado = sped.BlocosC100.First().BlocosC170.FirstOrDefault().ALIQ_COFINS;

            Assert.Equal(aliqPisEsperado.ToString().ToAliquotaDecimalDomain(), aliqPisTratado);
            Assert.Equal(aliqCofinsEsperado.ToString().ToAliquotaDecimalDomain(), aliqCofinsTratado);
        }

        [Fact]
        public void SeCSTFor70para50DeveCopiarAliqDaPlanilhaParaSped()
        {
            var cstOriginal = "70";
            var cstEsperado = "50";
            var aliqPisEsperado = new Faker().Random.Double(1, 25);
            var aliqCofinsEsperado = new Faker().Random.Double(1, 25);

            _mockRepoImportacao.Setup(mock => mock.ObterCST_PisProdutoPorCod_Item(It.IsAny<string>())).Returns(cstEsperado);
            _mockRepoImportacao.Setup(mock => mock.ObterCST_CofinsProdutoPorCod_Item(It.IsAny<string>())).Returns(cstEsperado);
            _mockRepoImportacao.Setup(mock => mock.ObterAliquotaPisProdutoPorCod_Item(It.IsAny<string>())).Returns(aliqPisEsperado.ToString());
            _mockRepoImportacao.Setup(mock => mock.ObterAliquotaCofinsProdutoPorCod_Item(It.IsAny<string>())).Returns(aliqCofinsEsperado.ToString());

            var sped = new Sped();
            var c100 = new C100();
            var c170 = new C170();

            c170.CST_PIS = cstOriginal;
            c170.CST_COFINS = cstOriginal;
            c170.ALIQ_PIS = "3";
            c170.ALIQ_COFINS = "4";
            c170.COD_ITEM = "18045";

            c100.BlocosC170.Add(c170);
            sped.BlocosC100.Add(c100);

            var serv = new CorrecaoSpedService(_mockRepoImportacao.Object, _mockLogRepository.Object);
            serv.TratarSped(sped, new string[] { });

            var aliqPisTratado = sped.BlocosC100.First().BlocosC170.FirstOrDefault().ALIQ_PIS;
            var aliqCofinsTratado = sped.BlocosC100.First().BlocosC170.FirstOrDefault().ALIQ_COFINS;

            Assert.Equal(aliqPisEsperado.ToString().ToAliquotaDecimalDomain(), aliqPisTratado);
            Assert.Equal(aliqCofinsEsperado.ToString().ToAliquotaDecimalDomain(), aliqCofinsTratado);
        }

        [Fact]
        public void SeCSTFor70para73DeveCopiarAliqDaPlanilhaParaSped()
        {
            var cstOriginal = "70";
            var cstEsperado = "73";
            var aliqPisEsperado = new Faker().Random.Double(1, 25);
            var aliqCofinsEsperado = new Faker().Random.Double(1, 25);

            _mockRepoImportacao.Setup(mock => mock.ObterCST_PisProdutoPorCod_Item(It.IsAny<string>())).Returns(cstEsperado);
            _mockRepoImportacao.Setup(mock => mock.ObterCST_CofinsProdutoPorCod_Item(It.IsAny<string>())).Returns(cstEsperado);
            _mockRepoImportacao.Setup(mock => mock.ObterAliquotaPisProdutoPorCod_Item(It.IsAny<string>())).Returns(aliqPisEsperado.ToString());
            _mockRepoImportacao.Setup(mock => mock.ObterAliquotaCofinsProdutoPorCod_Item(It.IsAny<string>())).Returns(aliqCofinsEsperado.ToString());

            var sped = new Sped();
            var c100 = new C100();
            var c170 = new C170();

            c170.CST_PIS = cstOriginal;
            c170.CST_COFINS = cstOriginal;
            c170.ALIQ_PIS = "3";
            c170.ALIQ_COFINS = "4";
            c170.COD_ITEM = "18045";

            c100.BlocosC170.Add(c170);
            sped.BlocosC100.Add(c100);

            var serv = new CorrecaoSpedService(_mockRepoImportacao.Object, _mockLogRepository.Object);
            serv.TratarSped(sped, new string[] { });

            var aliqPisTratado = sped.BlocosC100.First().BlocosC170.FirstOrDefault().ALIQ_PIS;
            var aliqCofinsTratado = sped.BlocosC100.First().BlocosC170.FirstOrDefault().ALIQ_COFINS;

            Assert.Equal(aliqPisEsperado.ToString().ToAliquotaDecimalDomain(), aliqPisTratado);
            Assert.Equal(aliqCofinsEsperado.ToString().ToAliquotaDecimalDomain(), aliqCofinsTratado);
        }


        [Fact]
        public void SeCSTFor73para50DeveCopiarValorTotalParaBC()
        {
            var cstOriginal = "73";
            var cstEsperado = "50";
            var qtd = new Faker().Random.Number(1, 100);
            var vl_item = new Faker().Random.Double(1, 100);

            _mockRepoImportacao.Setup(mock => mock.ObterCST_PisProdutoPorCod_Item(It.IsAny<string>())).Returns(cstEsperado);
            _mockRepoImportacao.Setup(mock => mock.ObterCST_CofinsProdutoPorCod_Item(It.IsAny<string>())).Returns(cstEsperado);

            var sped = new Sped();
            var c100 = new C100();
            var c170 = new C170();

            c170.CST_PIS = cstOriginal;
            c170.CST_COFINS = cstOriginal;
            c170.ALIQ_PIS = "3";
            c170.ALIQ_COFINS = "4";
            c170.VL_ITEM = vl_item.ToString();
            c170.QTD = qtd.ToString();
            c170.COD_ITEM = "18045";

            c100.BlocosC170.Add(c170);
            sped.BlocosC100.Add(c100);

            var serv = new CorrecaoSpedService(_mockRepoImportacao.Object, _mockLogRepository.Object);
            serv.TratarSped(sped, new string[] { });

            var VL_BC_PIS_Esperado = qtd * vl_item;
            var VL_BC_COFINS_Esperado = qtd * vl_item;

            var VL_BC_PIS_Tratado = sped.BlocosC100.First().BlocosC170.FirstOrDefault().VL_BC_PIS;
            var VL_BC_COFINS_Tratado = sped.BlocosC100.First().BlocosC170.FirstOrDefault().VL_BC_COFINS;

            Assert.Equal(VL_BC_PIS_Esperado.ToString(), VL_BC_PIS_Tratado);
            Assert.Equal(VL_BC_COFINS_Esperado.ToString(), VL_BC_COFINS_Tratado);
        }

        [Fact]
        public void SeCSTFor70para50DeveCopiarValorTotalParaBC()
        {
            var cstOriginal = "70";
            var cstEsperado = "50";
            var qtd = new Faker().Random.Number(1, 100);
            var vl_item = new Faker().Random.Double(1, 100); ;

            _mockRepoImportacao.Setup(mock => mock.ObterCST_PisProdutoPorCod_Item(It.IsAny<string>())).Returns(cstEsperado);
            _mockRepoImportacao.Setup(mock => mock.ObterCST_CofinsProdutoPorCod_Item(It.IsAny<string>())).Returns(cstEsperado);

            var sped = new Sped();
            var c100 = new C100();
            var c170 = new C170();

            c170.CST_PIS = cstOriginal;
            c170.CST_COFINS = cstOriginal;
            c170.ALIQ_PIS = "3";
            c170.ALIQ_COFINS = "4";
            c170.VL_ITEM = vl_item.ToString();
            c170.QTD = qtd.ToString();
            c170.COD_ITEM = "18045";

            c100.BlocosC170.Add(c170);
            sped.BlocosC100.Add(c100);

            var serv = new CorrecaoSpedService(_mockRepoImportacao.Object, _mockLogRepository.Object);
            serv.TratarSped(sped, new string[] { });

            var VL_BC_PIS_Esperado = qtd * vl_item;
            var VL_BC_COFINS_Esperado = qtd * vl_item;

            var VL_BC_PIS_Tratado = sped.BlocosC100.First().BlocosC170.FirstOrDefault().VL_BC_PIS;
            var VL_BC_COFINS_Tratado = sped.BlocosC100.First().BlocosC170.FirstOrDefault().VL_BC_COFINS;

            Assert.Equal(VL_BC_PIS_Esperado.ToString(), VL_BC_PIS_Tratado);
            Assert.Equal(VL_BC_COFINS_Esperado.ToString(), VL_BC_COFINS_Tratado);
        }

        [Fact]
        public void SeCSTFor70para50DeveCalcularVlPisCofinsComAliquotaDaPlanilha()
        {
            var cstOriginal = "70";
            var cstEsperado = "50";
            var aliqPisPlanilha = new Faker().Random.Double(1, 25);
            var aliqCofinsPlanilha = new Faker().Random.Double(1, 25);
            var qtd = new Faker().Random.Double(1, 100);
            var vl_item = new Faker().Random.Double(1, 100);

            _mockRepoImportacao.Setup(mock => mock.ObterCST_PisProdutoPorCod_Item(It.IsAny<string>())).Returns(cstEsperado);
            _mockRepoImportacao.Setup(mock => mock.ObterCST_CofinsProdutoPorCod_Item(It.IsAny<string>())).Returns(cstEsperado);
            _mockRepoImportacao.Setup(mock => mock.ObterAliquotaPisProdutoPorCod_Item(It.IsAny<string>())).Returns(aliqPisPlanilha.ToString());
            _mockRepoImportacao.Setup(mock => mock.ObterAliquotaCofinsProdutoPorCod_Item(It.IsAny<string>())).Returns(aliqCofinsPlanilha.ToString());

            var sped = new Sped();
            var c100 = new C100();
            var c170 = new C170();

            c170.CST_PIS = cstOriginal;
            c170.CST_COFINS = cstOriginal;
            c170.VL_ITEM = vl_item.ToString();
            c170.QTD = qtd.ToString();
            c170.COD_ITEM = "18045";

            c100.BlocosC170.Add(c170);
            sped.BlocosC100.Add(c100);

            var serv = new CorrecaoSpedService(_mockRepoImportacao.Object, _mockLogRepository.Object);
            serv.TratarSped(sped, new string[] { });

            var vl_bc_pis = double.Parse(sped.BlocosC100.First().BlocosC170.FirstOrDefault().VL_BC_PIS);
            var vl_bc_cofins = double.Parse(sped.BlocosC100.First().BlocosC170.FirstOrDefault().VL_BC_COFINS);

            var vl_pis_esperado = vl_bc_pis * aliqPisPlanilha.ToAliquotaDoubleDomain();
            var vl_cofins_esperado = vl_bc_cofins * aliqCofinsPlanilha.ToAliquotaDoubleDomain();

            var vl_pis_tratado = sped.BlocosC100.First().BlocosC170.FirstOrDefault().VL_PIS;
            var vl_cofins_tratado = sped.BlocosC100.First().BlocosC170.FirstOrDefault().VL_COFINS;

            Assert.Equal(vl_pis_esperado.ToString(), vl_pis_tratado);
            Assert.Equal(vl_cofins_esperado.ToString(), vl_cofins_tratado);
        }

        [Fact]
        public void SeCSTFor73para50DeveCalcularVlPisCofinsComAliquotaDaPlanilha()
        {
            var cstOriginal = "73";
            var cstEsperado = "50";
            var aliqPisPlanilha = new Faker().Random.Double(1, 25);
            var aliqCofinsPlanilha = new Faker().Random.Double(1, 25);
            var qtd = new Faker().Random.Double(1, 100);
            var vl_item = new Faker().Random.Double(1, 100);

            _mockRepoImportacao.Setup(mock => mock.ObterCST_PisProdutoPorCod_Item(It.IsAny<string>())).Returns(cstEsperado);
            _mockRepoImportacao.Setup(mock => mock.ObterCST_CofinsProdutoPorCod_Item(It.IsAny<string>())).Returns(cstEsperado);
            _mockRepoImportacao.Setup(mock => mock.ObterAliquotaPisProdutoPorCod_Item(It.IsAny<string>())).Returns(aliqPisPlanilha.ToString());
            _mockRepoImportacao.Setup(mock => mock.ObterAliquotaCofinsProdutoPorCod_Item(It.IsAny<string>())).Returns(aliqCofinsPlanilha.ToString());

            var sped = new Sped();
            var c100 = new C100();
            var c170 = new C170();

            c170.CST_PIS = cstOriginal;
            c170.CST_COFINS = cstOriginal;
            c170.VL_ITEM = vl_item.ToString();
            c170.QTD = qtd.ToString();
            c170.COD_ITEM = "18045";

            c100.BlocosC170.Add(c170);
            sped.BlocosC100.Add(c100);

            var serv = new CorrecaoSpedService(_mockRepoImportacao.Object, _mockLogRepository.Object);
            serv.TratarSped(sped, new string[] { });

            var vl_bc_pis = double.Parse(sped.BlocosC100.First().BlocosC170.FirstOrDefault().VL_BC_PIS);
            var vl_bc_cofins = double.Parse(sped.BlocosC100.First().BlocosC170.FirstOrDefault().VL_BC_COFINS);

            var vl_pis_esperado = vl_bc_pis * aliqPisPlanilha.ToAliquotaDoubleDomain();
            var vl_cofins_esperado = vl_bc_cofins * aliqCofinsPlanilha.ToAliquotaDoubleDomain();

            var vl_pis_tratado = sped.BlocosC100.First().BlocosC170.FirstOrDefault().VL_PIS;
            var vl_cofins_tratado = sped.BlocosC100.First().BlocosC170.FirstOrDefault().VL_COFINS;

            Assert.Equal(vl_pis_esperado.ToString(), vl_pis_tratado);
            Assert.Equal(vl_cofins_esperado.ToString(), vl_cofins_tratado);
        }

        [Fact]
        public void SeCSTFor70para73DeveCopiarValorTotalParaBC()
        {
            var cstOriginal = "70";
            var cstEsperado = "73";
            var qtd = new Faker().Random.Number(1, 100);
            var vl_item = new Faker().Random.Double(1, 100); ;

            _mockRepoImportacao.Setup(mock => mock.ObterCST_PisProdutoPorCod_Item(It.IsAny<string>())).Returns(cstEsperado);
            _mockRepoImportacao.Setup(mock => mock.ObterCST_CofinsProdutoPorCod_Item(It.IsAny<string>())).Returns(cstEsperado);

            var sped = new Sped();
            var c100 = new C100();
            var c170 = new C170();

            c170.CST_PIS = cstOriginal;
            c170.CST_COFINS = cstOriginal;
            c170.ALIQ_PIS = "3";
            c170.ALIQ_COFINS = "4";
            c170.VL_ITEM = vl_item.ToString();
            c170.QTD = qtd.ToString();
            c170.COD_ITEM = "18045";

            c100.BlocosC170.Add(c170);
            sped.BlocosC100.Add(c100);

            var serv = new CorrecaoSpedService(_mockRepoImportacao.Object, _mockLogRepository.Object);
            serv.TratarSped(sped, new string[] { });

            var VL_BC_PIS_Esperado = qtd * vl_item;
            var VL_BC_COFINS_Esperado = qtd * vl_item;

            var VL_BC_PIS_Tratado = sped.BlocosC100.First().BlocosC170.FirstOrDefault().VL_BC_PIS;
            var VL_BC_COFINS_Tratado = sped.BlocosC100.First().BlocosC170.FirstOrDefault().VL_BC_COFINS;

            Assert.Equal(VL_BC_PIS_Esperado.ToString(), VL_BC_PIS_Tratado);
            Assert.Equal(VL_BC_COFINS_Esperado.ToString(), VL_BC_COFINS_Tratado);
        }

        [Fact]
        public void SeCSTFor70para73DeveCalcularVlPisCofinsComAliquotaDaPlanilha()
        {
            var cstOriginal = "70";
            var cstEsperado = "73";
            var aliqPisPlanilha = new Faker().Random.Double(1, 25);
            var aliqCofinsPlanilha = new Faker().Random.Double(1, 25);
            var qtd = new Faker().Random.Double(1, 100);
            var vl_item = new Faker().Random.Double(1, 100);

            _mockRepoImportacao.Setup(mock => mock.ObterCST_PisProdutoPorCod_Item(It.IsAny<string>())).Returns(cstEsperado);
            _mockRepoImportacao.Setup(mock => mock.ObterCST_CofinsProdutoPorCod_Item(It.IsAny<string>())).Returns(cstEsperado);
            _mockRepoImportacao.Setup(mock => mock.ObterAliquotaPisProdutoPorCod_Item(It.IsAny<string>())).Returns(aliqPisPlanilha.ToString());
            _mockRepoImportacao.Setup(mock => mock.ObterAliquotaCofinsProdutoPorCod_Item(It.IsAny<string>())).Returns(aliqCofinsPlanilha.ToString());

            var sped = new Sped();
            var c100 = new C100();
            var c170 = new C170();

            c170.CST_PIS = cstOriginal;
            c170.CST_COFINS = cstOriginal;
            c170.VL_ITEM = vl_item.ToString();
            c170.QTD = qtd.ToString();
            c170.COD_ITEM = "18045";

            c100.BlocosC170.Add(c170);
            sped.BlocosC100.Add(c100);

            var serv = new CorrecaoSpedService(_mockRepoImportacao.Object, _mockLogRepository.Object);
            serv.TratarSped(sped, new string[] { });

            var vl_bc_pis = double.Parse(sped.BlocosC100.First().BlocosC170.FirstOrDefault().VL_BC_PIS);
            var vl_bc_cofins = double.Parse(sped.BlocosC100.First().BlocosC170.FirstOrDefault().VL_BC_COFINS);

            var vl_pis_esperado = vl_bc_pis * aliqPisPlanilha.ToAliquotaDoubleDomain();
            var vl_cofins_esperado = vl_bc_cofins * aliqCofinsPlanilha.ToAliquotaDoubleDomain();

            var vl_pis_tratado = sped.BlocosC100.First().BlocosC170.FirstOrDefault().VL_PIS;
            var vl_cofins_tratado = sped.BlocosC100.First().BlocosC170.FirstOrDefault().VL_COFINS;

            Assert.Equal(vl_pis_esperado.ToString(), vl_pis_tratado);
            Assert.Equal(vl_cofins_esperado.ToString(), vl_cofins_tratado);
        }

        [Theory]
        [InlineData("50", "73")]
        [InlineData("50", "70")]
        [InlineData("70", "50")]
        [InlineData("70", "73")]
        [InlineData("73", "50")]
        [InlineData("73", "70")]
        public void DeveCorrigirCstNaLinhaNoArquivoSped(string cstOriginal, string cstEsperado)
        {
            _mockRepoImportacao.Setup(mock => mock.ObterCST_PisProdutoPorCod_Item(It.IsAny<string>())).Returns(cstEsperado);
            _mockRepoImportacao.Setup(mock => mock.ObterCST_CofinsProdutoPorCod_Item(It.IsAny<string>())).Returns(cstEsperado);

            var arquivoSpedOriginal = new string[] {
                $@"|C100|0|1|36968395000168|55|00|1|119|33200736968395000168550010000001191176609464|11072020|11072020|514,65|0|0|0|514,65|9|0|0|0|283,88|0|0|0|0|0|0|0|0|",
                $@"|C170|1|90118|CONSERTO|7|UND|3507,05|0|1|540|1403||3507,05|0|0|0|0|0|0|52|||||{cstOriginal}|3507,05|0|||0|{cstOriginal}|3507,05|0|||0|3.02.1.1.00002|",
                $@"|C170|2|90571|CONSERTO|11|UND|945,6|0|1|540|1403||945,6|0|0|0|0|0|0|52|||||{cstOriginal}|945,6|0|||0|{cstOriginal}|945,6|0|||0|3.02.1.1.00002|",
            };

            var arquivoSpedEsperado = new string[] {
                $@"|C100|0|1|36968395000168|55|00|1|119|33200736968395000168550010000001191176609464|11072020|11072020|514,65|0|0|0|514,65|9|0|0|0|283,88|0|0|0|0|0|0|0|0|",
                $@"|C170|1|90118|CONSERTO|7|UND|3507,05|0|1|540|1403||3507,05|0|0|0|0|0|0|52|||||{cstEsperado}|3507,05|0|||0|{cstEsperado}|3507,05|0|||0|3.02.1.1.00002|",
                $@"|C170|2|90571|CONSERTO|11|UND|945,6|0|1|540|1403||945,6|0|0|0|0|0|0|52|||||{cstEsperado}|945,6|0|||0|{cstEsperado}|945,6|0|||0|3.02.1.1.00002|",
            };

            var sped = _txtService.ExecutaLeitura(arquivoSpedOriginal);

            var serv = new CorrecaoSpedService(_mockRepoImportacao.Object, _mockLogRepository.Object);
            serv.TratarSped(sped, arquivoSpedOriginal);

            var cstsPis = arquivoSpedOriginal.Skip(1).Select(row => row.Split('|')[25]);
            var cstsCofins = arquivoSpedOriginal.Skip(1).Select(row => row.Split('|')[31]);

            Assert.True(cstsPis.All(cst => cst == cstEsperado));
            Assert.True(cstsCofins.All(cst => cst == cstEsperado));
        }

        [Theory]
        [InlineData("50", "73")]
        [InlineData("50", "70")]
        [InlineData("70", "50")]
        [InlineData("70", "73")]
        [InlineData("73", "50")]
        [InlineData("73", "70")]
        public void DeveCorrigirAliqPisCofinsNaLinhaNoArquivoSped(string cstOriginal, string cstEsperado)
        {
            _mockRepoImportacao.Setup(mock => mock.ObterCST_PisProdutoPorCod_Item(It.IsAny<string>())).Returns(cstEsperado);
            _mockRepoImportacao.Setup(mock => mock.ObterCST_CofinsProdutoPorCod_Item(It.IsAny<string>())).Returns(cstEsperado);

            var arquivoSpedOriginal = new string[] {
                $@"|C100|0|1|36968395000168|55|00|1|119|33200736968395000168550010000001191176609464|11072020|11072020|514,65|0|0|0|514,65|9|0|0|0|283,88|0|0|0|0|0|0|0|0|",
                $@"|C170|1|90118|CONSERTO|7|UND|3507,05|0|1|540|1403||3507,05|0|0|0|0|0|0|52|||||{cstOriginal}|3507,05|0|||0|{cstOriginal}|3507,05|0|||0|3.02.1.1.00002|",
                $@"|C170|2|90571|CONSERTO|11|UND|945,6|0|1|540|1403||945,6|0|0|0|0|0|0|52|||||{cstOriginal}|945,6|0|||0|{cstOriginal}|945,6|0|||0|3.02.1.1.00002|",
            };

            var arquivoSpedEsperado = new string[] {
                $@"|C100|0|1|36968395000168|55|00|1|119|33200736968395000168550010000001191176609464|11072020|11072020|514,65|0|0|0|514,65|9|0|0|0|283,88|0|0|0|0|0|0|0|0|",
                $@"|C170|1|90118|CONSERTO|7|UND|3507,05|0|1|540|1403||3507,05|0|0|0|0|0|0|52|||||{cstEsperado}|3507,05|0|||0|{cstEsperado}|3507,05|0|||0|3.02.1.1.00002|",
                $@"|C170|2|90571|CONSERTO|11|UND|945,6|0|1|540|1403||945,6|0|0|0|0|0|0|52|||||{cstEsperado}|945,6|0|||0|{cstEsperado}|945,6|0|||0|3.02.1.1.00002|",
            };

            var sped = _txtService.ExecutaLeitura(arquivoSpedOriginal);

            var serv = new CorrecaoSpedService(_mockRepoImportacao.Object, _mockLogRepository.Object);
            serv.TratarSped(sped, arquivoSpedOriginal);

            var cstsPis = arquivoSpedOriginal.Skip(1).Select(row => row.Split('|')[25]);
            var cstsCofins = arquivoSpedOriginal.Skip(1).Select(row => row.Split('|')[31]);

            Assert.True(true);

            Assert.True(true);
        }

        [Theory]
        [InlineData("1556")]
        [InlineData("2556")]
        [InlineData("1551")]
        [InlineData("2551")]
        [InlineData("1407")]
        [InlineData("2407")]
        [InlineData("1406")]
        [InlineData("1910")]
        [InlineData("2910")]
        [InlineData("1653")]
        public void SeTiverCfopDaRegraDeveTrocarCstPISPara98(string cfop)
        {
            string cstPisOriginal = new Faker().Random.Number(1, 99).ToString();
            string cstPisEsperado = "98";

            _mockRepoImportacao.Setup(mock => mock.ObterCST_PisProdutoPorCod_Item(It.IsAny<string>())).Returns(cstPisEsperado);

            var sped = new Sped();
            var c100 = new C100();
            var c170 = new C170();
            var c170_ = new C170();

            c170.CST_PIS = cstPisOriginal;
            c170_.CST_PIS = cstPisOriginal;
            c170.CFOP = cfop;
            c170_.CFOP = cfop;

            c100.BlocosC170.Add(c170);
            c100.BlocosC170.Add(c170_);
            sped.BlocosC100.Add(c100);

            var serv = new CorrecaoSpedService(_mockRepoImportacao.Object, _mockLogRepository.Object);
            serv.TratarSped(sped, new string[] { });

            var blocos170ComCST_PIS_Original = sped.BlocosC100.First().BlocosC170.Where(bloco => bloco.CST_PIS == cstPisOriginal);
            var trocouTodosPisParaEsperado = blocos170ComCST_PIS_Original.All(bloco => bloco.CST_PIS_TRATADO == cstPisEsperado);

            Assert.NotEmpty(blocos170ComCST_PIS_Original);
            Assert.True(trocouTodosPisParaEsperado);
        }

        [Theory]
        [InlineData("1556")]
        [InlineData("2556")]
        [InlineData("1551")]
        [InlineData("2551")]
        [InlineData("1407")]
        [InlineData("2407")]
        [InlineData("1406")]
        [InlineData("1910")]
        [InlineData("2910")]
        [InlineData("1653")]
        public void SeTiverCfopDaRegraDeveTrocarCstCofinsPara98(string cfop)
        {
            string cstCofinsOriginal = new Faker().Random.Number(1, 99).ToString();
            string cstCofinsEsperado = "98";

            _mockRepoImportacao.Setup(mock => mock.ObterCST_CofinsProdutoPorCod_Item(It.IsAny<string>())).Returns(cstCofinsEsperado);

            var sped = new Sped();
            var c100 = new C100();
            var c170 = new C170();
            var c170_ = new C170();

            c170.CST_COFINS = cstCofinsOriginal;
            c170_.CST_COFINS = cstCofinsOriginal;
            c170.CFOP = cfop;
            c170_.CFOP = cfop;

            c100.BlocosC170.Add(c170);
            c100.BlocosC170.Add(c170_);
            sped.BlocosC100.Add(c100);

            var serv = new CorrecaoSpedService(_mockRepoImportacao.Object, _mockLogRepository.Object);
            serv.TratarSped(sped, new string[] { });

            var blocos170ComCST_Cofins_Original = sped.BlocosC100.First().BlocosC170.Where(bloco => bloco.CST_COFINS == cstCofinsOriginal);
            var trocouTodosCofinsParaEsperado = blocos170ComCST_Cofins_Original.All(bloco => bloco.CST_COFINS_TRATADO == cstCofinsEsperado);
            var todosC170CairamNaRegra = sped.BlocosC100.First().BlocosC170.All(c170 => c170.Tratado);

            Assert.NotEmpty(blocos170ComCST_Cofins_Original);
            Assert.True(todosC170CairamNaRegra);
            Assert.True(trocouTodosCofinsParaEsperado);
        }

        [Theory]
        [InlineData("1556")]
        [InlineData("2556")]
        [InlineData("1551")]
        [InlineData("2551")]
        [InlineData("1407")]
        [InlineData("2407")]
        [InlineData("1406")]
        [InlineData("1910")]
        [InlineData("2910")]
        [InlineData("1653")]
        public void SeTiverCfopDaRegraDeveZerarBC_Aliquotas_e_ValoresDePisCofins(string cfop)
        {
            var sped = new Sped();
            var c100 = new C100();
            var c170 = new C170();

            c170.CFOP = cfop;
            c170.VL_BC_PIS = new Faker().Random.Double(1, 100).ToString();
            c170.VL_BC_COFINS = new Faker().Random.Double(1, 100).ToString();
            c170.ALIQ_PIS = new Faker().Random.Double(1, 100).ToString();
            c170.ALIQ_COFINS = new Faker().Random.Double(1, 100).ToString();
            c170.VL_PIS = new Faker().Random.Double(1, 100).ToString();
            c170.VL_COFINS = new Faker().Random.Double(1, 100).ToString();

            c100.BlocosC170.Add(c170);
            sped.BlocosC100.Add(c100);

            var serv = new CorrecaoSpedService(_mockRepoImportacao.Object, _mockLogRepository.Object);
            serv.TratarSped(sped, new string[] { });

            var zerouAliquotaPercent = sped.BlocosC100.First().BlocosC170.All(bloco =>
                bloco.ALIQ_PIS == "0"
                && bloco.ALIQ_COFINS == "0"
            );

            var zerouValores = sped.BlocosC100.First().BlocosC170.All(bloco =>
                bloco.VL_PIS == "0"
                && bloco.VL_COFINS == "0"
            );

            var zerouBC = sped.BlocosC100.First().BlocosC170.All(bloco =>
                bloco.VL_BC_PIS == "0"
                && bloco.VL_BC_COFINS == "0"
            );

            Assert.True(zerouAliquotaPercent);
            Assert.True(zerouValores);
            Assert.True(zerouBC);
        }

        [Theory]
        [InlineData("0", "0,00")]
        [InlineData("1.7", "1,70")]
        [InlineData("1.7666", "1,76")]
        [InlineData("1.555,7666", "1555,76")]
        [InlineData("1,555.7666", "1555,76")]
        public void DeveTratarAliquotaParaVirComVirgulaESemPonto(string valorOriginal, string valorEsperado)
        {
            var valorTratado = valorOriginal.ToAliquotaDecimalDomain();

            Assert.Equal(valorTratado, valorEsperado);
        }
    }
}
