using Dapper.Contrib.Extensions;

namespace SysSped.Domain.Entities.Importacao
{
    [Table("rowimportacao")]
    public class RowImportacao
    {
        [Key]
        public int id { get; set; }
        public string rowAdress { get; set; }
        public string codigointerno { get; set; }
        public string DescricaoCliente { get; set; }
        public string ean { get; set; }
        public string ncm { get; set; }
        public string ncm_ex { get; set; }
        public string cest { get; set; }
        public string NomedoCliente { get; set; }
        public string icms_cst_entrada { get; set; }
        public string icms_cst_saida { get; set; }
        public string icms_aliquota_interna { get; set; }
        public string icms_aliquota_interna_saida { get; set; }
        public string icms_aliquota_efetiva_entrada { get; set; }
        public string icms_aliquota_efetiva_saida { get; set; }
        public string icms_aliquota_interestadual { get; set; }
        public string icms_aliquota_interestadual_saida { get; set; }
        public string icms_reducao_base_calculo { get; set; }
        public string icms_reducao_base_calculo_saida { get; set; }
        public string cfop_dentro_estado_entrada { get; set; }
        public string cfop_dentro_estado_saida { get; set; }
        public string cfop_fora_estado_entrada { get; set; }
        public string cfop_fora_estado_saida { get; set; }
        public string mva_original_atacado { get; set; }
        public string mva_original_industria { get; set; }
        public string mva_original_recalculada { get; set; }
        public string mva_ajustada_interestadual_4 { get; set; }
        public string mva_ajustada_interestadual_12 { get; set; }
        public string mva_ajustada_interestadual_recalculada { get; set; }
        public string desc_icms { get; set; }
        public string codigo { get; set; }
        public string descricao { get; set; }
        public string dt_inicio { get; set; }
        public string dt_fim { get; set; }
        public string legislacao { get; set; }
        public string pis_cst_entrada { get; set; }
        public string pis_cst_saida { get; set; }
        public string pis_aliquota_entrada { get; set; }
        public string pis_aliquota_saida { get; set; }
        public string pis_natureza_receita { get; set; }
        public string cofins_cst_entrada { get; set; }
        public string cofins_cst_saida { get; set; }
        public string cofins_aliquota_entrada { get; set; }
        public string cofins_aliquota_saida { get; set; }
        public string cofins_natureza_receita { get; set; }
        public string ipi_cst_entrada { get; set; }
        public string ipi_cst_saida { get; set; }
        public string ipi_aliquota { get; set; }
        public string ativo { get; set; } = "1";

    }
}
