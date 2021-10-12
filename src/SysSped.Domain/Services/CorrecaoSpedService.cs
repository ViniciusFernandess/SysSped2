using SysSped.Domain.Core;
using SysSped.Domain.Entities.CorrecaoSped;
using SysSped.Domain.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace SysSped.Domain.Services
{
    public class CorrecaoSpedService
    {
        private readonly IImportacaoRepository _importacaoRepo;
        private readonly ILogRepository _logRepository;
        private Bloco0000 bloco0000;
        private EnumRegraPesquisaProd _regraPesquisaProduto;

        public CorrecaoSpedService(IImportacaoRepository importacaoRepo, ILogRepository logService, EnumRegraPesquisaProd regraPesquisaProduto)
        {
            this._importacaoRepo = importacaoRepo;
            this._logRepository = logService;
            this._regraPesquisaProduto = regraPesquisaProduto;
        }

        public bool TratarSped(Sped sped, string[] linhasArquivoOriginal)
        {
            bloco0000 = sped.Bloco0000;

            var baseImportacao = _importacaoRepo.ObterImportacaoAtiva();
            var lstCodItem = baseImportacao.Select(x => x.codigointerno.Trim()).Distinct();
            var lstEan = baseImportacao.Select(x => x.ean.Trim()).Distinct();
            var lstNcm = baseImportacao.Select(x => x.ncm.Trim()).Distinct();

            var dicCodItemBase = lstCodItem.ToDictionary(x => x);
            var dicEanBase = lstEan.ToDictionary(x => x);
            var dicNcmBase = lstNcm.ToDictionary(x => x);

            foreach (var bloco100 in sped.BlocosC100)
            {
                if (bloco100.COD_MOD.Trim() != "55")
                    continue;

                var tipoNota = bloco100.IND_OPER.Trim() == "0" ? EnumTipoNota.Entrada : EnumTipoNota.Saida;
                foreach (var bloco170 in bloco100.BlocosC170)
                {
                    if (bloco170.COD_ITEM == "1815")
                    {

                    }

                    bool ehCfopDaRegra = VerificaSeCfopEhDaRegra(bloco170.CFOP);
                    if (ehCfopDaRegra && (bloco170.CST_PIS != "98" || bloco170.CST_COFINS != "98"))
                    {
                        AplicaRegraDosCfop(bloco170);
                    }
                    else
                    {
                        var temEan = !string.IsNullOrEmpty(bloco170.Bloco0200?.COD_BARRA ?? "");
                        var temNcm = !string.IsNullOrEmpty(bloco170.Bloco0200?.COD_NCM ?? "");
                        var temCod_item = !string.IsNullOrEmpty(bloco170.COD_ITEM);
                        var prodEncontrado = false;

                        if (_regraPesquisaProduto == EnumRegraPesquisaProd.EAN_MAIS_CODINTERNO)
                        {
                            if (!temEan && !temCod_item)
                                continue;

                            prodEncontrado = dicEanBase.ContainsKey(bloco170.Bloco0200?.COD_BARRA ?? "") || dicCodItemBase.ContainsKey(bloco170.COD_ITEM);
                        }
                        else if (_regraPesquisaProduto == EnumRegraPesquisaProd.EAN_MAIS_NCM)
                        {
                            if (!temEan && !temNcm)
                                continue;

                            prodEncontrado = dicEanBase.ContainsKey(bloco170.Bloco0200?.COD_BARRA ?? "") || dicNcmBase.ContainsKey(bloco170.Bloco0200?.COD_NCM ?? "");
                        }

                        if (prodEncontrado)
                        {
                            AplicaCSTPisTratado(bloco170);
                            AplicaCSTCofinsTratado(bloco170);

                            var prodNaoCaiuNaRegraDeCST = !bloco170.Tratado;
                            if (prodNaoCaiuNaRegraDeCST)
                                AplicaRegraProdNaoEncontrado(bloco170, tipoNota, Resource.REGRA_PRODUTO_EXCESSAO);
                        }
                        else
                        {
                            AplicaRegraProdNaoEncontrado(bloco170, tipoNota, Resource.REGRA_PRODUTO_NAO_ENCONTRADO);
                        }
                    }
                }

                RecalcularBlocoC100(bloco100);
            }

            var foiTratado = CorrigirArquivoSped(sped, linhasArquivoOriginal);

            return foiTratado;
        }

        private void AplicaRegraProdNaoEncontrado(C170 bloco170Arquivo, EnumTipoNota tipoNota, string regra)
        {

            var vlPisEhDiferenteZero = !string.IsNullOrEmpty(bloco170Arquivo.VL_PIS) && bloco170Arquivo.VL_PIS.Trim() != "0" && decimal.Parse(bloco170Arquivo.VL_PIS) > 0;
            var vlCofinsEhDiferenteZero = !string.IsNullOrEmpty(bloco170Arquivo.VL_COFINS) && bloco170Arquivo.VL_COFINS.Trim() != "0" && decimal.Parse(bloco170Arquivo.VL_COFINS) > 0;
            var vlPisOuCofinsEhDiferenteZero = vlPisEhDiferenteZero || vlCofinsEhDiferenteZero;

            // Regra 3
            if (!bloco170Arquivo.Tratado && tipoNota == EnumTipoNota.Entrada /*&& && vlPisOuCofinsEhDiferenteZero regra removida em 11/10/2021 - matheus combinou com a menina*/)
            {
                bloco170Arquivo.Tratado = true;
                bloco170Arquivo.Regra = regra;

                // Aplica CST PIS e COFINS
                bloco170Arquivo.CST_PIS_TRATADO = "50";
                bloco170Arquivo.CST_COFINS_TRATADO = "50";
            }
            else if (!bloco170Arquivo.Tratado && tipoNota == EnumTipoNota.Saida /*&& vlPisOuCofinsEhDiferenteZero*/)
            {
                bloco170Arquivo.Tratado = true;
                bloco170Arquivo.Regra = regra;

                // Aplica CST PIS e COFINS
                bloco170Arquivo.CST_PIS_TRATADO = "01";
                bloco170Arquivo.CST_COFINS_TRATADO = "01";
            }

            // Regra 4
            var ehCST_01 = bloco170Arquivo.CST_PIS == "01" || bloco170Arquivo.CST_PIS_TRATADO == "01";
            var ehCST_50 = bloco170Arquivo.CST_PIS == "50" || bloco170Arquivo.CST_PIS_TRATADO == "50";

            if (ehCST_01 && tipoNota == EnumTipoNota.Saida)
            {
                bloco170Arquivo.Tratado = true;

                // Aplica Aliq Pis e COFINS
                bloco170Arquivo.ALIQ_PIS = "1,65";
                bloco170Arquivo.ALIQ_COFINS = "7,60";

                // calcula BC PIS e COFINS
                bloco170Arquivo.VL_BC_PIS = CalcularBC(bloco170Arquivo.QTD, bloco170Arquivo.VL_ITEM);
                bloco170Arquivo.VL_BC_COFINS = CalcularBC(bloco170Arquivo.QTD, bloco170Arquivo.VL_ITEM);

                // calcule VL PIS e COFINS
                bloco170Arquivo.VL_PIS = CalcularVL_PIS_COFINS(bloco170Arquivo.VL_BC_PIS, bloco170Arquivo.ALIQ_PIS);
                bloco170Arquivo.VL_COFINS = CalcularVL_PIS_COFINS(bloco170Arquivo.VL_BC_COFINS, bloco170Arquivo.ALIQ_COFINS);

            }
            else if (ehCST_50 && tipoNota == EnumTipoNota.Entrada)
            {
                bloco170Arquivo.Tratado = true;

                // Aplica Aliq Pis e COFINS
                bloco170Arquivo.ALIQ_PIS = "1,65";
                bloco170Arquivo.ALIQ_COFINS = "7,60";

                // calcula BC PIS e COFINS
                bloco170Arquivo.VL_BC_PIS = CalcularBC(bloco170Arquivo.QTD, bloco170Arquivo.VL_ITEM);
                bloco170Arquivo.VL_BC_COFINS = CalcularBC(bloco170Arquivo.QTD, bloco170Arquivo.VL_ITEM);

                // calcule VL PIS e COFINS
                bloco170Arquivo.VL_PIS = CalcularVL_PIS_COFINS(bloco170Arquivo.VL_BC_PIS, bloco170Arquivo.ALIQ_PIS);
                bloco170Arquivo.VL_COFINS = CalcularVL_PIS_COFINS(bloco170Arquivo.VL_BC_COFINS, bloco170Arquivo.ALIQ_COFINS);
            }
        }

        private void CorrigirFormatacaoC170Sped(Sped sped, string[] linhasArquivoOriginal)
        {
            var c170s = sped.BlocosC100.SelectMany(x => x.BlocosC170);

            foreach (var c170 in c170s)
            {
                var totalZeroBaseArquivoC170 = linhasArquivoOriginal.Length;
                if (totalZeroBaseArquivoC170 < c170.IndiceArquivo)
                    continue;

                var camposC170 = ObterCampoDaLinha(c170.IndiceArquivo, linhasArquivoOriginal);
                var camposCorrigidosC170 = CorrigirFormatacaoCampos(camposC170);

                linhasArquivoOriginal[c170.IndiceArquivo] = "|" + string.Join("|", camposCorrigidosC170) + "|";
            }
        }

        private void RecalcularBlocoC100(C100 bloco100)
        {
            if (bloco100.VL_DOC != null)
            {
                if (bloco100.VL_DOC == "")
                    bloco100.VL_DOC = "0";

                var c100_VL_DOC = decimal.Parse(bloco100.VL_DOC);
                var c170_VL_ITEM = bloco100.BlocosC170.Sum(x => decimal.Parse(x.VL_ITEM));
                bloco100.VL_DOC = c170_VL_ITEM.ToString().ToAliquotaDecimalDomain();
            }

            if (bloco100.VL_PIS != null)
            {
                if (bloco100.VL_PIS == "")
                    bloco100.VL_PIS = "0";

                var c100_VL_PIS = decimal.Parse(bloco100.VL_PIS);
                var c170_VL_PIS = bloco100.BlocosC170.Sum(x => decimal.Parse(x.VL_PIS));
                bloco100.VL_PIS = c170_VL_PIS.ToString().ToAliquotaDecimalDomain();
            }

            if (bloco100.VL_COFINS != null)
            {
                if (bloco100.VL_COFINS == "")
                    bloco100.VL_COFINS = "0";

                var c100_VL_COFINS = decimal.Parse(bloco100.VL_COFINS);
                var c170_VL_COFINS = bloco100.BlocosC170.Sum(x => decimal.Parse(x.VL_COFINS));
                bloco100.VL_COFINS = c170_VL_COFINS.ToString().ToAliquotaDecimalDomain();
            }
        }

        private void AplicaRegraDosCfop(C170 bloco170)
        {
            bloco170.CST_COFINS_TRATADO = "98";
            bloco170.VL_BC_COFINS = "0";
            bloco170.ALIQ_COFINS = "0";
            bloco170.VL_COFINS = "0";

            bloco170.CST_PIS_TRATADO = "98";
            bloco170.VL_BC_PIS = "0";
            bloco170.ALIQ_PIS = "0";
            bloco170.VL_PIS = "0";

            bloco170.Regra = Resource.REGRA_CFOP;
            bloco170.Tratado = true;
        }

        private bool VerificaSeCfopEhDaRegra(string cfop)
        {
            var cfopsDaRegra = new List<string> {
            "1556",
            "2556",
            "1551",
            "2551",
            "1407",
            "2407",
            "1406",
            "1910",
            "2910",
            "1653"
            };

            var ehCfopDaRegra = cfopsDaRegra.Contains(cfop);

            return ehCfopDaRegra;
        }

        private bool CorrigirArquivoSped(Sped sped, string[] linhasArquivoOriginal)
        {
            var c100Tratados = sped.BlocosC100.Where(C100 => C100.BlocosC170.Any(c170 => c170.Tratado));
            var teveAlteracao = c100Tratados.Any();

            if (!teveAlteracao)
                return teveAlteracao;

            bloco0000.Id = _logRepository.RegistrarLogBloco0000(sped.Bloco0000);

            foreach (var c100 in sped.BlocosC100)
            {
                //var totalZeroBaseArquivoC100 = linhasArquivoOriginal.Length - 1;
                if (linhasArquivoOriginal.Length < c100.IndiceArquivo)
                    continue;

                var camposC100 = ObterCampoDaLinha(c100.IndiceArquivo, linhasArquivoOriginal);
                var camposCorrigidosC100 = CorrigirCampos(c100, camposC100);

                linhasArquivoOriginal[c100.IndiceArquivo] = "|" + string.Join("|", camposCorrigidosC100) + "|";

                foreach (var c170 in c100.BlocosC170)
                {
                    if (!c170.Tratado)
                        continue;

                    var totalZeroBaseArquivoC170 = linhasArquivoOriginal.Length;
                    if (totalZeroBaseArquivoC170 < c170.IndiceArquivo)
                        continue;

                    var camposC170 = ObterCampoDaLinha(c170.IndiceArquivo, linhasArquivoOriginal);
                    var camposCorrigidosC170 = CorrigirCampos(c170, camposC170);

                    linhasArquivoOriginal[c170.IndiceArquivo] = "|" + string.Join("|", camposCorrigidosC170) + "|";
                }
            }

            return teveAlteracao;
        }

        private List<string> CorrigirCampos(C170 c170, List<string> campos)
        {
            CorrigirCampoRegistrarLog(bloco0000, EnumTipoSped.C170, campos, c170.IndiceArquivo, 24, c170.COD_ITEM, c170.COD_NAT, c170.CST_PIS_TRATADO, "CST_PIS_TRATADO", c170.Regra);
            CorrigirCampoRegistrarLog(bloco0000, EnumTipoSped.C170, campos, c170.IndiceArquivo, 25, c170.COD_ITEM, c170.COD_NAT, c170.VL_BC_PIS, "VL_BC_PIS", c170.Regra);
            CorrigirCampoRegistrarLog(bloco0000, EnumTipoSped.C170, campos, c170.IndiceArquivo, 26, c170.COD_ITEM, c170.COD_NAT, c170.ALIQ_PIS, "ALIQ_PIS", c170.Regra);
            CorrigirCampoRegistrarLog(bloco0000, EnumTipoSped.C170, campos, c170.IndiceArquivo, 29, c170.COD_ITEM, c170.COD_NAT, c170.VL_PIS, "VL_PIS", c170.Regra);

            CorrigirCampoRegistrarLog(bloco0000, EnumTipoSped.C170, campos, c170.IndiceArquivo, 30, c170.COD_ITEM, c170.COD_NAT, c170.CST_COFINS_TRATADO, "CST_COFINS_TRATADO", c170.Regra);
            CorrigirCampoRegistrarLog(bloco0000, EnumTipoSped.C170, campos, c170.IndiceArquivo, 31, c170.COD_ITEM, c170.COD_NAT, c170.VL_BC_COFINS, "VL_BC_COFINS", c170.Regra);
            CorrigirCampoRegistrarLog(bloco0000, EnumTipoSped.C170, campos, c170.IndiceArquivo, 32, c170.COD_ITEM, c170.COD_NAT, c170.ALIQ_COFINS, "ALIQ_COFINS", c170.Regra);
            CorrigirCampoRegistrarLog(bloco0000, EnumTipoSped.C170, campos, c170.IndiceArquivo, 35, c170.COD_ITEM, c170.COD_NAT, c170.VL_COFINS, "VL_COFINS", c170.Regra);

            return CorrigirFormatacaoCampos(campos);
        }

        private List<string> CorrigirFormatacaoCampos(List<string> campos)
        {
            campos[4] = campos[4].To5DecimalPlace();
            campos[6] = campos[6].To2DecimalPlace();
            campos[7] = campos[7].To2DecimalPlace();
            campos[12] = campos[12].To2DecimalPlace();
            campos[13] = campos[13].To2DecimalPlace();
            campos[14] = campos[14].To2DecimalPlace();
            campos[15] = campos[15].To2DecimalPlace();
            campos[16] = campos[16].To2DecimalPlace();
            campos[17] = campos[17].To2DecimalPlace();
            campos[21] = campos[21].To2DecimalPlace();
            campos[22] = campos[22].To2DecimalPlace();
            campos[23] = campos[23].To2DecimalPlace();

            return campos;
        }

        private List<string> CorrigirCampos(C100 c100, List<string> campos)
        {
            CorrigirCampoRegistrarLog(bloco0000, EnumTipoSped.C100, campos, c100.IndiceArquivo, 11, "", "", c100.VL_DOC, "VL_DOC");
            CorrigirCampoRegistrarLog(bloco0000, EnumTipoSped.C100, campos, c100.IndiceArquivo, 25, "", "", c100.VL_PIS, "VL_PIS");
            CorrigirCampoRegistrarLog(bloco0000, EnumTipoSped.C100, campos, c100.IndiceArquivo, 26, "", "", c100.VL_COFINS, "VL_COFINS");

            return campos;
        }

        private void CorrigirCampoRegistrarLog(Bloco0000 bloco0000, EnumTipoSped tipoBloco, List<string> campos, int indiceArquivo, int indiceCampoOriginal, string codigoInterno, string ean, string campoTratado, string nomeCampo, string regra = "")
        {
            var campoOriginal = campos[indiceCampoOriginal];

            if (campoOriginal == campoTratado)
                return;

            _logRepository.RegistrarLog(bloco0000, tipoBloco, indiceCampoOriginal, nomeCampo, indiceArquivo, codigoInterno, ean, campoOriginal, campoTratado, regra);
            campos[indiceCampoOriginal] = campoTratado;
        }

        private List<string> ObterCampoDaLinha(int indiceArquivo, string[] linhasArquivoOriginal)
        {
            var campos = linhasArquivoOriginal[indiceArquivo].Split('|').ToList();
            var primeiro = 0;
            var ultimo = campos.Count() - 1;

            campos.RemoveAt(ultimo);
            campos.RemoveAt(primeiro);

            return campos;
        }

        private void AplicaCSTCofinsTratado(C170 bloco170Arquivo)
        {
            var cstCofinsTratadoProduto = ObtercstCofinsTratadoProduto(bloco170Arquivo);

            if (bloco170Arquivo.CST_COFINS == cstCofinsTratadoProduto)
            {
                var AliquotaCofinsProdutoPlanilha = ObterAliquotaCofinsProdutoPlanilha(bloco170Arquivo);

                bloco170Arquivo.Tratado = true;
                bloco170Arquivo.CST_COFINS_TRATADO = cstCofinsTratadoProduto;
                bloco170Arquivo.ALIQ_COFINS = AliquotaCofinsProdutoPlanilha;
                bloco170Arquivo.VL_BC_COFINS = CalcularBC(bloco170Arquivo.QTD, bloco170Arquivo.VL_ITEM);
                bloco170Arquivo.VL_COFINS = CalcularVL_PIS_COFINS(bloco170Arquivo.VL_BC_COFINS, AliquotaCofinsProdutoPlanilha);
            }

            else if (bloco170Arquivo.CST_COFINS == "50" && cstCofinsTratadoProduto == "73")
            {
                bloco170Arquivo.Tratado = true;
                bloco170Arquivo.CST_COFINS_TRATADO = cstCofinsTratadoProduto;
                bloco170Arquivo.ALIQ_COFINS = "0";
                bloco170Arquivo.VL_COFINS = "0";
            }

            else if (bloco170Arquivo.CST_COFINS == "50" && cstCofinsTratadoProduto == "70")
            {
                bloco170Arquivo.Tratado = true;
                bloco170Arquivo.CST_COFINS_TRATADO = cstCofinsTratadoProduto;
                bloco170Arquivo.VL_BC_COFINS = "0";
                bloco170Arquivo.ALIQ_COFINS = "0";
                bloco170Arquivo.VL_COFINS = "0";
            }

            else if (bloco170Arquivo.CST_COFINS == "70" && cstCofinsTratadoProduto == "50")
            {
                var AliquotaCofinsProdutoPlanilha = ObterAliquotaCofinsProdutoPlanilha(bloco170Arquivo);

                bloco170Arquivo.Tratado = true;
                bloco170Arquivo.CST_COFINS_TRATADO = cstCofinsTratadoProduto;
                bloco170Arquivo.ALIQ_COFINS = AliquotaCofinsProdutoPlanilha;
                bloco170Arquivo.VL_BC_COFINS = CalcularBC(bloco170Arquivo.QTD, bloco170Arquivo.VL_ITEM);
                bloco170Arquivo.VL_COFINS = CalcularVL_PIS_COFINS(bloco170Arquivo.VL_BC_COFINS, AliquotaCofinsProdutoPlanilha);
            }

            else if (bloco170Arquivo.CST_COFINS == "70" && cstCofinsTratadoProduto == "73")
            {
                var AliquotaCofinsProdutoPlanilha = ObterAliquotaCofinsProdutoPlanilha(bloco170Arquivo);

                bloco170Arquivo.Tratado = true;
                bloco170Arquivo.CST_COFINS_TRATADO = cstCofinsTratadoProduto;
                bloco170Arquivo.ALIQ_COFINS = AliquotaCofinsProdutoPlanilha;
                bloco170Arquivo.VL_BC_COFINS = CalcularBC(bloco170Arquivo.QTD, bloco170Arquivo.VL_ITEM);
                bloco170Arquivo.VL_COFINS = CalcularVL_PIS_COFINS(bloco170Arquivo.VL_BC_COFINS, AliquotaCofinsProdutoPlanilha);
            }

            else if (bloco170Arquivo.CST_COFINS == "73" && cstCofinsTratadoProduto == "50")
            {
                var AliquotaCofinsProdutoPlanilha = ObterAliquotaCofinsProdutoPlanilha(bloco170Arquivo);

                bloco170Arquivo.Tratado = true;
                bloco170Arquivo.CST_COFINS_TRATADO = cstCofinsTratadoProduto;
                bloco170Arquivo.ALIQ_COFINS = AliquotaCofinsProdutoPlanilha;
                bloco170Arquivo.VL_BC_COFINS = CalcularBC(bloco170Arquivo.QTD, bloco170Arquivo.VL_ITEM);
                bloco170Arquivo.VL_COFINS = CalcularVL_PIS_COFINS(bloco170Arquivo.VL_BC_COFINS, AliquotaCofinsProdutoPlanilha);
            }

            else if (bloco170Arquivo.CST_COFINS == "73" && cstCofinsTratadoProduto == "70")
            {
                bloco170Arquivo.Tratado = true;
                bloco170Arquivo.CST_COFINS_TRATADO = cstCofinsTratadoProduto;
                bloco170Arquivo.VL_BC_COFINS = "0";
                bloco170Arquivo.ALIQ_COFINS = "0";
                bloco170Arquivo.VL_COFINS = "0";
            }

            if (bloco170Arquivo.Tratado)
                bloco170Arquivo.Regra = _regraPesquisaProduto == EnumRegraPesquisaProd.EAN_MAIS_CODINTERNO ? Resource.REGRA_EAN_MAIS_CODINTERNO : Resource.REGRA_EAN_MAIS_NCM;
        }

        private string ObtercstCofinsTratadoProduto(C170 bloco170Arquivo)
        {
            var temEan = !string.IsNullOrEmpty(bloco170Arquivo.Bloco0200?.COD_BARRA ?? "");
            var temCod_item = !string.IsNullOrEmpty(bloco170Arquivo.COD_ITEM);
            var cstCofinsTratadoProduto = "";


            if (_regraPesquisaProduto == EnumRegraPesquisaProd.EAN_MAIS_CODINTERNO)
            {
                if (temEan)
                    cstCofinsTratadoProduto = _importacaoRepo.ObterCST_CofinsProdutoPorEan(bloco170Arquivo.Bloco0200?.COD_BARRA ?? "");

                var encontrouCofinsPlanilha = !string.IsNullOrEmpty(cstCofinsTratadoProduto);

                if (temCod_item && !encontrouCofinsPlanilha)
                    cstCofinsTratadoProduto = _importacaoRepo.ObterCST_CofinsProdutoPorCod_Item(bloco170Arquivo.COD_ITEM);
            }
            else if (_regraPesquisaProduto == EnumRegraPesquisaProd.EAN_MAIS_NCM)
            {
                if (temEan)
                    cstCofinsTratadoProduto = _importacaoRepo.ObterCST_CofinsProdutoPorEan(bloco170Arquivo.Bloco0200?.COD_BARRA ?? "");

                var encontrouCofinsPlanilha = !string.IsNullOrEmpty(cstCofinsTratadoProduto);

                if (temCod_item && !encontrouCofinsPlanilha)
                    cstCofinsTratadoProduto = _importacaoRepo.ObterCST_CofinsProdutoPorNcm(bloco170Arquivo.Bloco0200?.COD_NCM ?? "");

            }

            return cstCofinsTratadoProduto;
        }

        private string ObterAliquotaCofinsProdutoPlanilha(C170 bloco170Arquivo)
        {
            var temEan = !string.IsNullOrEmpty(bloco170Arquivo.Bloco0200?.COD_BARRA ?? "");
            var temCod_item = !string.IsNullOrEmpty(bloco170Arquivo.COD_ITEM);
            var AliquotaCofinsProdutoPlanilha = "";


            if (_regraPesquisaProduto == EnumRegraPesquisaProd.EAN_MAIS_CODINTERNO)
            {
                if (temEan)
                    AliquotaCofinsProdutoPlanilha = _importacaoRepo.ObterAliquotaCofinsProdutoPorEan(bloco170Arquivo.Bloco0200?.COD_BARRA ?? "");

                var encontrouCofinsPlanilha = !string.IsNullOrEmpty(AliquotaCofinsProdutoPlanilha);

                if (temCod_item && !encontrouCofinsPlanilha)
                    AliquotaCofinsProdutoPlanilha = _importacaoRepo.ObterAliquotaCofinsProdutoPorCod_Item(bloco170Arquivo.COD_ITEM);
            }
            else if (_regraPesquisaProduto == EnumRegraPesquisaProd.EAN_MAIS_NCM)
            {
                if (temEan)
                    AliquotaCofinsProdutoPlanilha = _importacaoRepo.ObterAliquotaCofinsProdutoPorEan(bloco170Arquivo.Bloco0200?.COD_BARRA ?? "");

                var encontrouCofinsPlanilha = !string.IsNullOrEmpty(AliquotaCofinsProdutoPlanilha);

                if (temCod_item && !encontrouCofinsPlanilha)
                    AliquotaCofinsProdutoPlanilha = _importacaoRepo.ObterAliquotaCofinsProdutoPorNcm(bloco170Arquivo.Bloco0200?.COD_NCM ?? "");

            }

            if (!string.IsNullOrEmpty(AliquotaCofinsProdutoPlanilha) && decimal.TryParse(AliquotaCofinsProdutoPlanilha.ToAliquotaDecimalDomain(), out var valor))
                AliquotaCofinsProdutoPlanilha = AliquotaCofinsProdutoPlanilha.ToAliquotaDecimalDomain();

            return AliquotaCofinsProdutoPlanilha;
        }

        private void AplicaCSTPisTratado(C170 bloco170Arquivo)
        {
            var cstPisTratadoProduto = ObtercstPisTratadoProduto(bloco170Arquivo);

            if (bloco170Arquivo.CST_PIS == cstPisTratadoProduto)
            {
                var AliquotaPisProdutoPlanilha = ObterAliquotaPisProdutoPlanilha(bloco170Arquivo);

                bloco170Arquivo.Tratado = true;
                bloco170Arquivo.CST_PIS_TRATADO = cstPisTratadoProduto;
                bloco170Arquivo.ALIQ_PIS = AliquotaPisProdutoPlanilha;
                bloco170Arquivo.VL_BC_PIS = CalcularBC(bloco170Arquivo.QTD, bloco170Arquivo.VL_ITEM);
                bloco170Arquivo.VL_PIS = CalcularVL_PIS_COFINS(bloco170Arquivo.VL_BC_PIS, AliquotaPisProdutoPlanilha);

            }

            else if (bloco170Arquivo.CST_PIS == "50" && cstPisTratadoProduto == "73")
            {
                bloco170Arquivo.Tratado = true;
                bloco170Arquivo.CST_PIS_TRATADO = cstPisTratadoProduto;
                bloco170Arquivo.ALIQ_PIS = "0";
                bloco170Arquivo.VL_PIS = "0";

            }

            else if (bloco170Arquivo.CST_PIS == "50" && cstPisTratadoProduto == "70")
            {
                bloco170Arquivo.Tratado = true;
                bloco170Arquivo.CST_PIS_TRATADO = cstPisTratadoProduto;
                bloco170Arquivo.VL_BC_PIS = "0";
                bloco170Arquivo.ALIQ_PIS = "0";
                bloco170Arquivo.VL_PIS = "0";

            }

            else if (bloco170Arquivo.CST_PIS == "70" && cstPisTratadoProduto == "50")
            {
                var AliquotaPisProdutoPlanilha = ObterAliquotaPisProdutoPlanilha(bloco170Arquivo);

                bloco170Arquivo.Tratado = true;
                bloco170Arquivo.CST_PIS_TRATADO = cstPisTratadoProduto;
                bloco170Arquivo.ALIQ_PIS = AliquotaPisProdutoPlanilha;
                bloco170Arquivo.VL_BC_PIS = CalcularBC(bloco170Arquivo.QTD, bloco170Arquivo.VL_ITEM);
                bloco170Arquivo.VL_PIS = CalcularVL_PIS_COFINS(bloco170Arquivo.VL_BC_PIS, AliquotaPisProdutoPlanilha);

            }

            else if (bloco170Arquivo.CST_PIS == "70" && cstPisTratadoProduto == "73")
            {
                var AliquotaPisProdutoPlanilha = ObterAliquotaPisProdutoPlanilha(bloco170Arquivo);

                bloco170Arquivo.Tratado = true;
                bloco170Arquivo.CST_PIS_TRATADO = cstPisTratadoProduto;
                bloco170Arquivo.ALIQ_PIS = AliquotaPisProdutoPlanilha;
                bloco170Arquivo.VL_BC_PIS = CalcularBC(bloco170Arquivo.QTD, bloco170Arquivo.VL_ITEM);
                bloco170Arquivo.VL_PIS = CalcularVL_PIS_COFINS(bloco170Arquivo.VL_BC_PIS, AliquotaPisProdutoPlanilha);

            }

            else if (bloco170Arquivo.CST_PIS == "73" && cstPisTratadoProduto == "50")
            {
                var AliquotaPisProdutoPlanilha = ObterAliquotaPisProdutoPlanilha(bloco170Arquivo);

                bloco170Arquivo.Tratado = true;
                bloco170Arquivo.CST_PIS_TRATADO = cstPisTratadoProduto;
                bloco170Arquivo.ALIQ_PIS = AliquotaPisProdutoPlanilha;
                bloco170Arquivo.VL_BC_PIS = CalcularBC(bloco170Arquivo.QTD, bloco170Arquivo.VL_ITEM);
                bloco170Arquivo.VL_PIS = CalcularVL_PIS_COFINS(bloco170Arquivo.VL_BC_PIS, AliquotaPisProdutoPlanilha);
            }

            else if (bloco170Arquivo.CST_PIS == "73" && cstPisTratadoProduto == "70")
            {
                bloco170Arquivo.Tratado = true;
                bloco170Arquivo.CST_PIS_TRATADO = cstPisTratadoProduto;
                bloco170Arquivo.VL_BC_PIS = "0";
                bloco170Arquivo.ALIQ_PIS = "0";
                bloco170Arquivo.VL_PIS = "0";
            }

            if (bloco170Arquivo.Tratado)
                bloco170Arquivo.Regra = _regraPesquisaProduto == EnumRegraPesquisaProd.EAN_MAIS_CODINTERNO ? Resource.REGRA_EAN_MAIS_CODINTERNO : Resource.REGRA_EAN_MAIS_NCM;
        }

        private string ObterAliquotaPisProdutoPlanilha(C170 bloco170Arquivo)
        {
            var temEan = !string.IsNullOrEmpty(bloco170Arquivo.Bloco0200?.COD_BARRA ?? "");
            var temCod_item = !string.IsNullOrEmpty(bloco170Arquivo.COD_ITEM);
            var AliquotaPisProdutoPlanilha = "";

            if (_regraPesquisaProduto == EnumRegraPesquisaProd.EAN_MAIS_CODINTERNO)
            {
                if (temEan)
                    AliquotaPisProdutoPlanilha = _importacaoRepo.ObterAliquotaPisProdutoPorEan(bloco170Arquivo.Bloco0200?.COD_BARRA ?? "");

                var encontrouPisPlanilha = !string.IsNullOrEmpty(AliquotaPisProdutoPlanilha);

                if (temCod_item && !encontrouPisPlanilha)
                    AliquotaPisProdutoPlanilha = _importacaoRepo.ObterAliquotaPisProdutoPorCod_Item(bloco170Arquivo.COD_ITEM);

            }
            else if (_regraPesquisaProduto == EnumRegraPesquisaProd.EAN_MAIS_NCM)
            {
                if (temEan)
                    AliquotaPisProdutoPlanilha = _importacaoRepo.ObterAliquotaPisProdutoPorEan(bloco170Arquivo.Bloco0200?.COD_BARRA ?? "");

                var encontrouPisPlanilha = !string.IsNullOrEmpty(AliquotaPisProdutoPlanilha);

                if (temCod_item && !encontrouPisPlanilha)
                    AliquotaPisProdutoPlanilha = _importacaoRepo.ObterAliquotaPisProdutoPorNcm(bloco170Arquivo.Bloco0200?.COD_NCM ?? "");

            }

            if (!string.IsNullOrEmpty(AliquotaPisProdutoPlanilha) && decimal.TryParse(AliquotaPisProdutoPlanilha.ToAliquotaDecimalDomain(), out var valor))
                AliquotaPisProdutoPlanilha = AliquotaPisProdutoPlanilha.ToAliquotaDecimalDomain();

            return AliquotaPisProdutoPlanilha;
        }

        private string ObtercstPisTratadoProduto(C170 bloco170Arquivo)
        {
            var temEan = !string.IsNullOrEmpty(bloco170Arquivo.Bloco0200?.COD_BARRA ?? "");
            var cstPisTratadoProduto = "";

            if (_regraPesquisaProduto == EnumRegraPesquisaProd.EAN_MAIS_CODINTERNO)
            {
                if (temEan)
                    cstPisTratadoProduto = _importacaoRepo.ObterCST_PisProdutoPorEan(bloco170Arquivo.Bloco0200?.COD_BARRA ?? "0");

                var temCod_item = !string.IsNullOrEmpty(bloco170Arquivo.COD_ITEM);
                var encontrouPisPlanilha = !string.IsNullOrEmpty(cstPisTratadoProduto);

                if (temCod_item && !encontrouPisPlanilha)
                    cstPisTratadoProduto = _importacaoRepo.ObterCST_PisProdutoPorCod_Item(bloco170Arquivo.COD_ITEM);

            }
            else if (_regraPesquisaProduto == EnumRegraPesquisaProd.EAN_MAIS_NCM)
            {
                if (temEan)
                    cstPisTratadoProduto = _importacaoRepo.ObterCST_PisProdutoPorEan(bloco170Arquivo.Bloco0200.COD_BARRA);

                var encontrouPisPlanilha = !string.IsNullOrEmpty(cstPisTratadoProduto);

                //if (temEan && !encontrouPisPlanilha)
                if (!encontrouPisPlanilha)
                    cstPisTratadoProduto = _importacaoRepo.ObterCST_PisProdutoPorNcm(bloco170Arquivo?.Bloco0200.COD_NCM ?? "0");

            }


            return cstPisTratadoProduto;
        }

        private string CalcularVL_PIS_COFINS(string _VL_BC_PISCofins, string _AliquotaPisCofinsProdutoPlanilha)
        {
            double VL_BC_PISCofins = 0;
            double AliquotaPisCofinsProdutoPlanilha = 0;

            double.TryParse(_VL_BC_PISCofins, out VL_BC_PISCofins);
            double.TryParse(_AliquotaPisCofinsProdutoPlanilha, out AliquotaPisCofinsProdutoPlanilha);

            return (VL_BC_PISCofins * (AliquotaPisCofinsProdutoPlanilha / 100)).ToString().ToAliquotaDecimalDomain();
        }

        private string CalcularBC(string _QTD, string _VL_ITEM)
        {
            return _VL_ITEM;

            //Passado que nao deve calcular, somente deve retornar o valor do item
            double QTD = 0;
            double VL_ITEM = 0;

            double.TryParse(_QTD, out QTD);
            double.TryParse(_VL_ITEM, out VL_ITEM);

            return (QTD * VL_ITEM).ToString().ToAliquotaDecimalDomain();
        }
    }
}
