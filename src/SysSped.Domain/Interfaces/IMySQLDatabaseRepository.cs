namespace SysSped.Domain.Interfaces
{
    public interface IMySQLDatabaseRepository
    {
        bool VerificaBDExiste();
        bool VerificarTableExiste(string tableName);

        void CriaBD();

        void CriarTableC170();
        void CriarTableBloco0000();
        void CriarTableRowImportacao();
        void CriarTableLogAlteracaoSped();
        void CriarTableTipoSped();
    }
}
