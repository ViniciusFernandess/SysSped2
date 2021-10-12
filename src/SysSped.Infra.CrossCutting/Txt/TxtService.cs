using SysSped.Domain.Core;
using SysSped.Domain.Entities.CorrecaoSped;
using SysSped.Domain.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace SysSped.Infra.CrossCutting.Txt
{
    public class TxtService : Validation, ITxtService
    {
        public Sped sped { get; private set; }

        public Sped ExecutaLeitura(string[] txtArquivo)
        {
            sped = new Sped();

            VerificaSeEstaVazioOuNulo(txtArquivo);
            if (!IsValid())
                return null;

            ObterCampos(txtArquivo);
            if (!IsValid())
                return null;

            ObterEanDosC170();

            return sped;
        }

        private void ObterEanDosC170()
        {
            foreach (var c100 in sped.BlocosC100)
            {
                foreach (var c170 in c100.BlocosC170)
                {
                    if (c170.COD_ITEM == "1267")
                    {

                    }

                    var bloco0200Correspondente = sped.Blocos0200.FirstOrDefault(b => b.COD_ITEM == c170.COD_ITEM);

                    if (bloco0200Correspondente != null)
                    {
                        c170.Bloco0200 = bloco0200Correspondente;
                        //c170.COD_NAT = bloco0200Correspondente.COD_BARRA;

                    }
                }
            }
        }

        private void ObterCampos(string[] rows)
        {
            var ultimoC100 = new C100();
            for (int i = 0; i < rows.Length; i++)
            {
                var row = rows[i];
                var indiceLinha = i;

                var ehBloco0000 = row.StartsWith("|0000|");
                if (ehBloco0000)
                {
                    var campos = row.Split('|').ToList();
                    var primeiro = 0;
                    var ultimo = campos.Count() - 1;

                    campos.RemoveAt(ultimo);
                    campos.RemoveAt(primeiro);

                    var bloco0000 = new Bloco0000
                    {
                        REG = campos[0],
                        COD_VER = campos[1],
                        TIPO_ESCRIT = campos[2],
                        IND_SIT_ESP = campos[3],
                        NUM_REC_ANTERIOR = campos[4],
                        DT_INI = campos[5],
                        DT_FIN = campos[6],
                        NOME = campos[7],
                        CNPJ = campos[8],
                        UF = campos[9],
                        COD_MUN = campos[10],
                        SUFRAMA = campos[11],
                        IND_NAT_PJ = campos[12],
                        IND_ATIV = campos[13],
                        Ativo = true
                    };

                    sped.Bloco0000 = bloco0000;
                }

                var ehBlocoC100 = row.StartsWith("|C100|");
                if (ehBlocoC100)
                {
                    var campos = row.Split('|').ToList();
                    var primeiro = 0;
                    var ultimo = campos.Count() - 1;

                    campos.RemoveAt(ultimo);
                    campos.RemoveAt(primeiro);

                    ultimoC100 = new C100
                    {
                        IndiceArquivo = indiceLinha,
                        REG = campos[0],
                        IND_OPER = campos[1],
                        IND_EMIT = campos[2],
                        COD_PART = campos[3],
                        COD_MOD = campos[4],
                        COD_SIT = campos[5],
                        SER = campos[6],
                        NUM_DOC = campos[7],
                        CHV_NFE = campos[8],
                        DT_DOC = campos[9],
                        DT_E_S = campos[10],
                        VL_DOC = campos[11],
                        IND_PGTO = campos[12],
                        VL_DESC = campos[13],
                        VL_ABAT_NT = campos[14],
                        VL_MERC = campos[15],
                        IND_FRT = campos[16],
                        VL_FRT = campos[17],
                        VL_SEG = campos[18],
                        VL_OUT_DA = campos[19],
                        VL_BC_ICMS = campos[20],
                        VL_ICMS = campos[21],
                        VL_BC_ICMS_ST = campos[22],
                        VL_ICMS_ST = campos[23],
                        VL_IPI = campos[24],
                        VL_PIS = campos[25],
                        VL_COFINS = campos[26],
                        VL_PIS_ST = campos[27],
                        VL_COFINS_ST = campos[28],
                        BlocosC170 = new List<C170>()
                    };

                    sped.BlocosC100.Add(ultimoC100);
                }

                var ehBlocoC170 = row.StartsWith("|C170|");
                if (ehBlocoC170)
                {
                    var campos = row.Split('|').ToList();
                    var primeiro = 0;
                    var ultimo = campos.Count() - 1;

                    campos.RemoveAt(ultimo);
                    campos.RemoveAt(primeiro);

                    var c170 = new C170
                    {
                        IndiceArquivo = indiceLinha,
                        REG = campos[0],
                        NUM_ITEM = campos[1],
                        COD_ITEM = campos[2],
                        DESCR_COMPL = campos[3],
                        QTD = campos[4],
                        UNID = campos[5],
                        VL_ITEM = campos[6],
                        VL_DESC = campos[7],
                        IND_MOV = campos[8],
                        CST_ICMS = campos[9],
                        CFOP = campos[10],
                        COD_NAT = campos[11],
                        VL_BC_ICMS = campos[12],
                        ALIQ_ICMS = campos[13],
                        VL_ICMS = campos[14],
                        VL_BC_ICMS_ST = campos[15],
                        ALIQ_ST = campos[16],
                        VL_ICMS_ST = campos[17],
                        IND_APUR = campos[18],
                        CST_IPI = campos[19],
                        COD_ENQ = campos[20],
                        VL_BC_IPI = campos[21],
                        ALIQ_IPI = campos[22],
                        VL_IPI = campos[23],
                        CST_PIS = campos[24],
                        VL_BC_PIS = campos[25],
                        ALIQ_PIS = campos[26],
                        QUANT_BC_PIS = campos[27],
                        ALIQ_PIS_QUANT = campos[28],
                        VL_PIS = campos[29],
                        CST_COFINS = campos[30],
                        VL_BC_COFINS = campos[31],
                        ALIQ_COFINS = campos[32],
                        QUANT_BC_COFINS = campos[33],
                        ALIQ_COFINS_QUANT = campos[34],
                        VL_COFINS = campos[35],
                        COD_CTA = campos[36],
                    };

                    ultimoC100.BlocosC170.Add(c170);
                }

                var ehBlocoC200 = row.StartsWith("|0200|");
                if (ehBlocoC200)
                {
                    var campos = row.Split('|').ToList();
                    var primeiro = 0;
                    var ultimo = campos.Count() - 1;

                    campos.RemoveAt(ultimo);
                    campos.RemoveAt(primeiro);

                    var bloco0200 = new Bloco0200
                    {
                        IndiceArquivo = indiceLinha,
                        REG = campos[0],
                        COD_ITEM = campos[1],
                        DESCR_ITEM = campos[2],
                        COD_BARRA = campos[3],
                        COD_ANT_ITEM = campos[4],
                        UNID_INV = campos[5],
                        TIPO_ITEM = campos[6],
                        COD_NCM = campos[7],
                        EX_IPI = campos[8],
                        COD_GEN = campos[9],
                        COD_LST = campos[10],
                        ALIQ_ICMS = campos[11],
                    };

                    sped.Blocos0200.Add(bloco0200);
                }
            }
        }

        private void VerificaSeEstaVazioOuNulo(string[] linhasArquivo)
        {
            if (linhasArquivo.Length == 0)
                AddErro(Resource.ARQUIVO_TXT_VAZIO_NULO);
        }
    }
}
