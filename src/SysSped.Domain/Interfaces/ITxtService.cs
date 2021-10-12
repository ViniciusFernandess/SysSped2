using SysSped.Domain.Entities.CorrecaoSped;

namespace SysSped.Domain.Interfaces
{
    public interface ITxtService
    {
        Sped ExecutaLeitura(string[] txtArquivo);
    }
}
