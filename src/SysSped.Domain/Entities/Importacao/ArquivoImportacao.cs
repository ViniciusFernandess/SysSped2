using System.Collections.Generic;

namespace SysSped.Domain.Entities.Importacao
{
    public class ArquivoImportacao
    {
        public List<RowImportacao> Rows { get; set; } = new List<RowImportacao>();
    }
}
