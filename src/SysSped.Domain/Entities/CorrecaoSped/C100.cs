using System.Collections.Generic;

namespace SysSped.Domain.Entities.CorrecaoSped
{
    public class C100 : LinhaSpedBase<C170>
    {
        public string REG { get; set; }
        public string IND_OPER { get; set; }
        public string IND_EMIT { get; set; }
        public string COD_PART { get; set; }
        public string COD_MOD { get; set; }
        public string COD_SIT { get; set; }
        public string SER { get; set; }
        public string NUM_DOC { get; set; }
        public string CHV_NFE { get; set; }
        public string DT_DOC { get; set; }
        public string DT_E_S { get; set; }
        public string VL_DOC { get; set; }
        public string IND_PGTO { get; set; }
        public string VL_DESC { get; set; }
        public string VL_ABAT_NT { get; set; }
        public string VL_MERC { get; set; }
        public string IND_FRT { get; set; }
        public string VL_FRT { get; set; }
        public string VL_SEG { get; set; }
        public string VL_OUT_DA { get; set; }
        public string VL_BC_ICMS { get; set; }
        public string VL_ICMS { get; set; }
        public string VL_BC_ICMS_ST { get; set; }
        public string VL_ICMS_ST { get; set; }
        public string VL_IPI { get; set; }
        public string VL_PIS { get; set; }
        public string VL_COFINS { get; set; }
        public string VL_PIS_ST { get; set; }
        public string VL_COFINS_ST { get; set; }

        public List<C170> BlocosC170 = new List<C170>();
    }
}
