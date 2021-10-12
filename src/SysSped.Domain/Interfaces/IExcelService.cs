using OfficeOpenXml;
using SysSped.Domain.Core.Interfaces;
using SysSped.Domain.Entities.Importacao;

namespace SysSped.Domain.Interfaces
{
    public interface IExcelService : IValidation
    {
        ArquivoImportacao ExecutaLeitura(ExcelPackage pkg);
    }
}
