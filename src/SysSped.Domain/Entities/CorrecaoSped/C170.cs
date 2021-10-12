namespace SysSped.Domain.Entities.CorrecaoSped
{
    public class C170 : LinhaSpedBase<C170>
    {
        public string REG { get; set; }
        public string NUM_ITEM { get; set; }
        public string COD_ITEM { get; set; }
        public string DESCR_COMPL { get; set; }
        public string QTD { get; set; }
        public string UNID { get; set; }
        public string VL_ITEM { get; set; }
        public string VL_DESC { get; set; }
        public string IND_MOV { get; set; }
        public string CST_ICMS { get; set; }
        public string CFOP { get; set; }
        public string COD_NAT { get; set; }
        public string VL_BC_ICMS { get; set; }
        public string ALIQ_ICMS { get; set; }
        public string VL_ICMS { get; set; }
        public string VL_BC_ICMS_ST { get; set; }
        public string ALIQ_ST { get; set; }
        public string VL_ICMS_ST { get; set; }
        public string IND_APUR { get; set; }
        public string CST_IPI { get; set; }
        public string COD_ENQ { get; set; }
        public string VL_BC_IPI { get; set; }
        public string ALIQ_IPI { get; set; }
        public string VL_IPI { get; set; }
        public string CST_PIS { get; set; }
        private string _CST_PIS_TRATADO;
        public string CST_PIS_TRATADO
        {
            get { return string.IsNullOrEmpty(_CST_PIS_TRATADO) ? CST_PIS : _CST_PIS_TRATADO; }
            set { _CST_PIS_TRATADO = value; }
        }
        public string VL_BC_PIS { get; set; }
        public string ALIQ_PIS { get; set; }
        public string QUANT_BC_PIS { get; set; }
        public string ALIQ_PIS_QUANT { get; set; }
        public string VL_PIS { get; set; }
        public string CST_COFINS { get; set; }
        private string _CST_COFINS_TRATADO;

        public string CST_COFINS_TRATADO
        {
            get { return string.IsNullOrEmpty(_CST_COFINS_TRATADO) ? CST_COFINS : _CST_COFINS_TRATADO; }
            set { _CST_COFINS_TRATADO = value; }
        }

        public string VL_BC_COFINS { get; set; }
        public string ALIQ_COFINS { get; set; }
        public string QUANT_BC_COFINS { get; set; }
        public string ALIQ_COFINS_QUANT { get; set; }
        public string VL_COFINS { get; set; }
        public string COD_CTA { get; set; }

        public Bloco0200 Bloco0200 { get; set; }
    }
}
