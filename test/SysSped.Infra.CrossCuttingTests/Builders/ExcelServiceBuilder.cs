using SysSped.Infra.CrossCutting.Excel;
using System.IO;

namespace SysSped.Infra.CrossCuttingTests.Builders
{
    public class ExcelServiceBuilder
    {
        private FileInfo Arquivo;

        private ExcelServiceBuilder()
        {
        }

        public ArquivoImportacaoService Build()
        {
            return new ArquivoImportacaoService();
        }

        public ExcelServiceBuilder ComExtensao(FileInfo arquivo)
        {
            Arquivo = arquivo;
            return this;
        }

        public static ExcelServiceBuilder Create()
        {
            var builder = new ExcelServiceBuilder();
            builder.Arquivo = new FileInfo("Arquivo.xlsx");

            return builder;
        }

        public ExcelServiceBuilder ComExtensao(string extensao)
        {
            this.Arquivo = new FileInfo($"Arquivo{extensao}");
            return this;
        }


    }
}
