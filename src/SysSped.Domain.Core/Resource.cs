namespace SysSped.Domain.Core
{
    public static class Resource
    {
        public static readonly string ARQUIVO_EXCEL_NULO = "Arquivo Excel nulo";
        public static readonly string EXTENSAO_INVALIDA = "Extensão inválida";
        public static readonly string ARQUIVO_NAO_ENCONTRADO = "Arquivo não encontrado";
        public static readonly string CAMPO_INVALIDO_NO_ARQUIVO = "Campo inválido no arquivo";
        public static readonly string ARQUIVO_ESTA_FALTANDO_CAMPO_PADRAO = "Arquivo está faltando campo padrão";
        public static readonly string OBJETO_NAO_TEM_PROPRIEDADE_COM_NOME_DA_COLUNA = "Objeto mapa não tem uma propriedade com mesmo nome da coluna do arquivo";
        public static readonly string NAO_INATIVOU_IMPORTACAO_ATUAL = "Houve um erro ao tentar inativar a importação atual";
        public static readonly string ARQUIVO_TXT_VAZIO_NULO = "Arquivo txt vazio";
        public static readonly string NAO_TEM_IMPORTACAO_ATIVA = "Não tem importação ativa";
        public static readonly string ARQUIVO_EXCEL_TEM_CODINTERNO_REPETIDO = "Arquivo Excel tem CodInterno repetido.";
        public static readonly string ARQUIVO_EXCEL_TEM_EAN_REPETIDO = "Arquivo Excel tem EAN repetido.";

        public static readonly string REGRA_CFOP = "CFOP";
        public static readonly string REGRA_PRODUTO_NAO_ENCONTRADO = "Produto Não Encontrado";
        public static readonly string REGRA_EAN_MAIS_CODINTERNO = "EAN + CodInterno";
        public static readonly string REGRA_EAN_MAIS_NCM = "EAN + NCM";
        public static readonly string REGRA_PRODUTO_EXCESSAO = "Regra de Exceção"; //Produto encontrado na planilha, mas nao caiu na regra de nenhum CST
    }
}
