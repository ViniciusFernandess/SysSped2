using OfficeOpenXml;
using System.Collections.Generic;

namespace SysSped.Infra.CrossCuttingTests.Builders
{
    public class ArquivoExcelBuilder
    {
        private IEnumerable<string> _camposPadroes;
        private bool _comLinhas;

        private ArquivoExcelBuilder()
        {
        }

        public static ArquivoExcelBuilder Create()
        {
            var builder = new ArquivoExcelBuilder();
            return builder;
        }

        public ArquivoExcelBuilder ComCabecalho(IEnumerable<string> camposPadroes)
        {
            this._camposPadroes = camposPadroes;
            return this;
        }

        public ArquivoExcelBuilder ComLinhas()
        {
            this._comLinhas = true;
            return this;
        }

        public ExcelPackage Build()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            var package = new ExcelPackage();
            var workbook = package.Workbook;
            var sheet = workbook.Worksheets.Add("Plan1");

            if (_camposPadroes != null)
            {
                int col = 1;

                //Popula cabeçalho do arquivo excel com campos padoes 
                foreach (var campo in _camposPadroes)
                {
                    sheet.Cells[1, col].Value = campo;
                    col++;
                }
            }

            if (_comLinhas)
            {
                sheet.Cells["A2"].Value = "95183";
                sheet.Cells["B2"].Value = "REDUTOR 1101 900ML TEMPO";
                sheet.Cells["C2"].Value = "7894195110124";
                sheet.Cells["D2"].Value = "58063200";
                sheet.Cells["E2"].Value = "";
                sheet.Cells["F2"].Value = "";
                sheet.Cells["G2"].Value = "CONSTRULIDER DE MANGARATIBA MATERIAL";
                sheet.Cells["H2"].Value = "0";
                sheet.Cells["I2"].Value = "0";
                sheet.Cells["J2"].Value = "20.0000";
                sheet.Cells["K2"].Value = "20.0000";
                sheet.Cells["L2"].Value = "";
                sheet.Cells["M2"].Value = "";
                sheet.Cells["N2"].Value = "12.0000";
                sheet.Cells["O2"].Value = "12.0000";
                sheet.Cells["P2"].Value = "";
                sheet.Cells["Q2"].Value = "";
                sheet.Cells["R2"].Value = "1102";
                sheet.Cells["S2"].Value = "5102";
                sheet.Cells["T2"].Value = "2102";
                sheet.Cells["U2"].Value = "6102";
                sheet.Cells["V2"].Value = "";
                sheet.Cells["W2"].Value = "";
                sheet.Cells["X2"].Value = "";
                sheet.Cells["Y2"].Value = "";
                sheet.Cells["Z2"].Value = "";
                sheet.Cells["AA2"].Value = "";
                sheet.Cells["AB2"].Value = "";
                sheet.Cells["AC2"].Value = "";
                sheet.Cells["AD2"].Value = "";
                sheet.Cells["AE2"].Value = "";
                sheet.Cells["AF2"].Value = "";
                sheet.Cells["AG2"].Value = "";
                sheet.Cells["AH2"].Value = "50";
                sheet.Cells["AI2"].Value = "1";
                sheet.Cells["AJ2"].Value = "1.6500";
                sheet.Cells["AK2"].Value = "1.6500";
                sheet.Cells["AL2"].Value = "";
                sheet.Cells["AM2"].Value = "50";
                sheet.Cells["AN2"].Value = "1";
                sheet.Cells["AO2"].Value = "7.6000";
                sheet.Cells["AP2"].Value = "7.6000";
                sheet.Cells["AQ2"].Value = "";
                sheet.Cells["AR2"].Value = "3";
                sheet.Cells["AS2"].Value = "53";
                sheet.Cells["AT2"].Value = "0.0000";

                sheet.Cells["A3"].Value = "95235";
                sheet.Cells["B3"].Value = "ABRACADEIRA COPO 1";
                sheet.Cells["C3"].Value = "7898492970214";
                sheet.Cells["D3"].Value = "73269090";
                sheet.Cells["E3"].Value = "";
                sheet.Cells["F3"].Value = "1006200";
                sheet.Cells["G3"].Value = "CONSTRULIDER DE MANGARATIBA MATERIAL";
                sheet.Cells["H3"].Value = "60";
                sheet.Cells["I3"].Value = "60";
                sheet.Cells["J3"].Value = "20.0000";
                sheet.Cells["K3"].Value = "20.0000";
                sheet.Cells["L3"].Value = "0.0000";
                sheet.Cells["M3"].Value = "0.0000";
                sheet.Cells["N3"].Value = "12.0000";
                sheet.Cells["O3"].Value = "12.0000";
                sheet.Cells["P3"].Value = "";
                sheet.Cells["Q3"].Value = "";
                sheet.Cells["R3"].Value = "1403";
                sheet.Cells["S3"].Value = "5405";
                sheet.Cells["T3"].Value = "2403";
                sheet.Cells["U3"].Value = "6404";
                sheet.Cells["V3"].Value = "";
                sheet.Cells["W3"].Value = "80.0000";
                sheet.Cells["X3"].Value = "";
                sheet.Cells["Y3"].Value = "116.0000";
                sheet.Cells["Z3"].Value = "98.0000";
                sheet.Cells["AA3"].Value = "";
                sheet.Cells["AB3"].Value = "";
                sheet.Cells["AC3"].Value = "";
                sheet.Cells["AD3"].Value = "";
                sheet.Cells["AE3"].Value = "";
                sheet.Cells["AF3"].Value = "";
                sheet.Cells["AG3"].Value = "";
                sheet.Cells["AH3"].Value = "50";
                sheet.Cells["AI3"].Value = "1";
                sheet.Cells["AJ3"].Value = "1.6500";
                sheet.Cells["AK3"].Value = "1.6500";
                sheet.Cells["AL3"].Value = "";
                sheet.Cells["AM3"].Value = "50";
                sheet.Cells["AN3"].Value = "1";
                sheet.Cells["AO3"].Value = "7.6000";
                sheet.Cells["AP3"].Value = "7.6000";
                sheet.Cells["AQ3"].Value = "";
                sheet.Cells["AR3"].Value = "3";
                sheet.Cells["AS3"].Value = "53";
                sheet.Cells["AT3"].Value = "5.0000";

                sheet.Cells["A4"].Value = "95235";
                sheet.Cells["B4"].Value = "ABRACADEIRA COPO 1";
                sheet.Cells["C4"].Value = "7898492970214";
                sheet.Cells["D4"].Value = "73269090";
                sheet.Cells["E4"].Value = "";
                sheet.Cells["F4"].Value = "1006200";
                sheet.Cells["G4"].Value = "CONSTRULIDER DE MANGARATIBA MATERIAL";
                sheet.Cells["H4"].Value = "60";
                sheet.Cells["I4"].Value = "60";
                sheet.Cells["J4"].Value = "20.0000";
                sheet.Cells["K4"].Value = "20.0000";
                sheet.Cells["L4"].Value = "0.0000";
                sheet.Cells["M4"].Value = "0.0000";
                sheet.Cells["N4"].Value = "12.0000";
                sheet.Cells["O4"].Value = "12.0000";
                sheet.Cells["P4"].Value = "";
                sheet.Cells["Q4"].Value = "";
                sheet.Cells["R4"].Value = "1403";
                sheet.Cells["S4"].Value = "5405";
                sheet.Cells["T4"].Value = "2403";
                sheet.Cells["U4"].Value = "6404";
                sheet.Cells["V4"].Value = "";
                sheet.Cells["W4"].Value = "80.0000";
                sheet.Cells["X4"].Value = "";
                sheet.Cells["Y4"].Value = "116.0000";
                sheet.Cells["Z4"].Value = "98.0000";
                sheet.Cells["AA4"].Value = "";
                sheet.Cells["AB4"].Value = "";
                sheet.Cells["AC4"].Value = "";
                sheet.Cells["AD4"].Value = "";
                sheet.Cells["AE4"].Value = "";
                sheet.Cells["AF4"].Value = "";
                sheet.Cells["AG4"].Value = "";
                sheet.Cells["AH4"].Value = "50";
                sheet.Cells["AI4"].Value = "1";
                sheet.Cells["AJ4"].Value = "1.6500";
                sheet.Cells["AK4"].Value = "1.6500";
                sheet.Cells["AL4"].Value = "";
                sheet.Cells["AM4"].Value = "50";
                sheet.Cells["AN4"].Value = "1";
                sheet.Cells["AO4"].Value = "7.6000";
                sheet.Cells["AP4"].Value = "7.6000";
                sheet.Cells["AQ4"].Value = "";
                sheet.Cells["AR4"].Value = "3";
                sheet.Cells["AS4"].Value = "53";
                sheet.Cells["AT4"].Value = "5.0000";

            }

            return package;
        }
    }
}
