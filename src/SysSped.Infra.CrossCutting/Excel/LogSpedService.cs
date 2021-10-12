using OfficeOpenXml;
using SysSped.Domain.Core;
using SysSped.Domain.Entities.CorrecaoSped;
using SysSped.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SysSped.Infra.CrossCutting.Excel
{
    public class LogSpedService : ILogSpedService
    {
        private readonly ILogRepository _repoLog;

        public LogSpedService(ILogRepository repoLog)
        {
            _repoLog = repoLog;

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        public string ExtrairRelatorioAlteracoesSped(IEnumerable<Bloco0000> blocos0000, string caminhoPasta)
        {
            var dicBloco0000 = blocos0000.ToDictionary(x => x.Id);
            var ids = blocos0000.Select(x => x.Id);

            var rel = _repoLog.ObterAlteracoesPorIdBloco0000(ids);

            var file = new FileInfo($@"{caminhoPasta}\Log Alterações SPED {DateTime.Now.ToString("dd-MM-yyyy HHmmss")}.xlsx");
            var pkg = new ExcelPackage(file);
            var sheet = pkg.Workbook.Worksheets.Add("Alterações SPED");

            sheet.Cells["A1"].Value = "Data Alteração";
            sheet.Cells["B1"].Value = "NOME";
            sheet.Cells["C1"].Value = "CNPJ";
            sheet.Cells["D1"].Value = "DT_INI";
            sheet.Cells["E1"].Value = "DT_FIN";

            sheet.Cells["F1"].Value = "Nro Linha";
            sheet.Cells["G1"].Value = "Bloco";
            sheet.Cells["H1"].Value = "Codigo Interno";
            sheet.Cells["I1"].Value = "Ean";
            sheet.Cells["J1"].Value = "Indice Campo";
            sheet.Cells["K1"].Value = "Nome Campo";
            sheet.Cells["L1"].Value = "Valor Antigo";
            sheet.Cells["M1"].Value = "Valor Atual";
            sheet.Cells["N1"].Value = "Regra";

            for (int i = 1; i < rel.Count; i++)
            {
                var item = rel[i - 1];
                var bloco0000 = dicBloco0000[item.IdBloco0000];

                var row = i + 1;

                sheet.Cells[row, 1].Value = bloco0000.DataCadastro.ToString();
                sheet.Cells[row, 2].Value = bloco0000.NOME;
                sheet.Cells[row, 3].Value = bloco0000.CNPJ;
                sheet.Cells[row, 4].Value = bloco0000.DT_INI.ToDateTime().ToShortDateString();
                sheet.Cells[row, 5].Value = bloco0000.DT_FIN.ToDateTime().ToShortDateString();

                sheet.Cells[row, 6].Value = item.NroLinha;
                sheet.Cells[row, 7].Value = item.Bloco;
                sheet.Cells[row, 8].Value = item.CodigoInterno;
                sheet.Cells[row, 9].Value = item.Ean;
                sheet.Cells[row, 10].Value = item.IndiceCampo;
                sheet.Cells[row, 11].Value = item.NomeCampo;
                sheet.Cells[row, 12].Value = item.ValorAntigo;
                sheet.Cells[row, 13].Value = item.ValorAtual;
                sheet.Cells[row, 14].Value = item.Regra;
            }

            var range = sheet.Cells[1, 1, rel.Count, 14];
            sheet.Tables.Add(range, "Log");
            sheet.Cells.AutoFitColumns();

            pkg.Save();
            pkg.Dispose();

            return file.FullName;
        }

        public string ExtrairRelatorioC100NaoTratado(Sped sped, string caminhoPasta)
        {
            var dic0200 = sped.Blocos0200.ToDictionary(x => x.COD_ITEM);
            var c170s = sped.BlocosC100.SelectMany(x => x.BlocosC170).Where(c170 => !c170.Tratado).DistinctBy(c170 => c170.COD_ITEM).ToList();

            var file = new FileInfo($@"{caminhoPasta}\Produtos não alterados {DateTime.Now.ToString("dd-MM-yyyy HHmmss")}.xlsx");
            var pkg = new ExcelPackage(file);
            var sheet = pkg.Workbook.Worksheets.Add("Produtos");

            sheet.Cells["A1"].Value = "Codigo Interno";
            sheet.Cells["B1"].Value = "Descrição";
            sheet.Cells["C1"].Value = "CST Pis";
            sheet.Cells["D1"].Value = "CST Cofins";
            

            for (int i = 1; i < c170s.Count; i++)
            {
                var item = c170s[i - 1];
                var row = i + 1;

                sheet.Cells[row, 1].Value = item.COD_ITEM;
                sheet.Cells[row, 2].Value = dic0200.ContainsKey(item.COD_ITEM) ? dic0200[item.COD_ITEM].DESCR_ITEM : "";
                sheet.Cells[row, 3].Value = item.CST_PIS;
                sheet.Cells[row, 4].Value = item.CST_COFINS;
            }

            var range = sheet.Cells[1, 1, c170s.Count, 4];
            sheet.Tables.Add(range, "Log");
            sheet.Cells.AutoFitColumns();

            pkg.Save();
            pkg.Dispose();

            return file.FullName;
        }
    }
}
