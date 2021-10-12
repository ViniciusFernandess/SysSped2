using Dapper;
using SysSped.Domain.Interfaces;
using System.Linq;

namespace SysSped.Infra.Data
{
    public class MySQLDatabaseRepository : BaseRepository, IMySQLDatabaseRepository
    {
        private string bdName = "sysspeddb";

        public MySQLDatabaseRepository() : base()
        {

        }

        public MySQLDatabaseRepository(bool coisa) : base(coisa)
        {

        }



        public bool VerificaUserExiste()
        {
            var sql = $@"SELECT EXISTS (SELECT DISTINCT user FROM mysql.user WHERE user = 'admin') as is_user ;";

            var retorno = _conn.Query<int>(sql).FirstOrDefault() > 0;
            return retorno;
        }
        public void CriaUser()
        {
            var sql = $@"CREATE USER IF NOT EXISTS 'admin'@'localhost' IDENTIFIED BY 'abc123';";
            _conn.Query(sql);
        }

        public void CriaBD()
        {
            var sql = $@"CREATE DATABASE IF NOT EXISTS {bdName};";
            _conn.Query(sql);
        }

        public bool VerificaBDExiste()
        {
            var sql = $@"
                        SELECT SCHEMA_NAME
                        FROM INFORMATION_SCHEMA.SCHEMATA
                        WHERE SCHEMA_NAME = '{bdName}'";

            var retorno = _conn.Query(sql).Any();
            return retorno;
        }

        public bool VerificarTableExiste(string tableName)
        {
            var sql = $@"
                        USE {bdName};
                        SHOW TABLES LIKE '{tableName}';";

            var retorno = _conn.Query(sql).Any();
            return retorno;
        }

        public void CriarTableTipoSped()
        {
            var sql = $@"
                      USE {bdName};

                      CREATE TABLE `tiposped` (
                      `Id` int NOT NULL,
                      `Titulo` varchar(45) NOT NULL,
                      PRIMARY KEY (`Id`)
                      ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

                      INSERT INTO sysspeddb.tiposped (Id, titulo) VALUES (1, 'Bloco0000'), (2, 'C100'), (3, 'C170');";
            _conn.Query(sql);
        }

        public void CriarTableLogAlteracaoSped()
        {
            var sql = $@"
                      USE {bdName};

                       CREATE TABLE `logalteracaosped` (
                        `Id` int NOT NULL AUTO_INCREMENT,
                        `IdBloco0000` int NOT NULL,
                        `NroLinha` int NOT NULL,
                        `IndiceCampo` int NOT NULL,
                        `NomeCampo` varchar(45) NOT NULL,
                        `CodigoInterno` varchar(45) DEFAULT NULL,
                        `Ean` varchar(45) DEFAULT NULL,
                        `TipoSped` int NOT NULL,
                        `ValorAntigo` varchar(45) NOT NULL,
                        `ValorAtual` varchar(45) NOT NULL,
                        `DataCadastro` datetime NOT NULL,
                        `Ativo` bit(1) NOT NULL,
                        `regra` varchar(100),
                        PRIMARY KEY (`Id`),
                        KEY `LogTipoSped_idx` (`TipoSped`),
                        KEY `LogIdBloco0000_idx` (`IdBloco0000`),
                        CONSTRAINT `LogIdBloco0000` FOREIGN KEY (`IdBloco0000`) REFERENCES `bloco0000` (`Id`),
                        CONSTRAINT `LogTipoSped` FOREIGN KEY (`TipoSped`) REFERENCES `tiposped` (`Id`)
                    ) ENGINE=InnoDB AUTO_INCREMENT=44069 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;";
            _conn.Query(sql);
        }

        public void CriarTableRowImportacao()
        {
            var sql = $@"
                      USE {bdName};

                    CREATE TABLE `rowimportacao` (
                    `Id` int NOT NULL AUTO_INCREMENT,
                    `rowAdress` varchar(45) DEFAULT NULL,
                    `codigointerno` varchar(45) DEFAULT NULL,
                    `DescricaoCliente` varchar(245) DEFAULT NULL,
                    `ean` varchar(45) DEFAULT NULL,
                    `ncm` varchar(45) DEFAULT NULL,
                    `ncm_ex` varchar(45) DEFAULT NULL,
                    `cest` varchar(45) DEFAULT NULL,
                    `NomedoCliente` varchar(45) DEFAULT NULL,
                    `icms_cst_entrada` varchar(45) DEFAULT NULL,
                    `icms_cst_saida` varchar(45) DEFAULT NULL,
                    `icms_aliquota_interna` varchar(45) DEFAULT NULL,
                    `icms_aliquota_interna_saida` varchar(45) DEFAULT NULL,
                    `icms_aliquota_efetiva_entrada` varchar(45) DEFAULT NULL,
                    `icms_aliquota_efetiva_saida` varchar(45) DEFAULT NULL,
                    `icms_aliquota_interestadual` varchar(45) DEFAULT NULL,
                    `icms_aliquota_interestadual_saida` varchar(45) DEFAULT NULL,
                    `icms_reducao_base_calculo` varchar(45) DEFAULT NULL,
                    `icms_reducao_base_calculo_saida` varchar(45) DEFAULT NULL,
                    `cfop_dentro_estado_entrada` varchar(45) DEFAULT NULL,
                    `cfop_dentro_estado_saida` varchar(45) DEFAULT NULL,
                    `cfop_fora_estado_entrada` varchar(45) DEFAULT NULL,
                    `cfop_fora_estado_saida` varchar(45) DEFAULT NULL,
                    `mva_original_atacado` varchar(45) DEFAULT NULL,
                    `mva_original_industria` varchar(45) DEFAULT NULL,
                    `mva_original_recalculada` varchar(45) DEFAULT NULL,
                    `mva_ajustada_interestadual_4` varchar(45) DEFAULT NULL,
                    `mva_ajustada_interestadual_12` varchar(45) DEFAULT NULL,
                    `mva_ajustada_interestadual_recalculada` varchar(45) DEFAULT NULL,
                    `desc_icms` varchar(45) DEFAULT NULL,
                    `codigo` varchar(45) DEFAULT NULL,
                    `descricao` varchar(245) DEFAULT NULL,
                    `dt_inicio` varchar(45) DEFAULT NULL,
                    `dt_fim` varchar(45) DEFAULT NULL,
                    `legislacao` varchar(245) DEFAULT NULL,
                    `pis_cst_entrada` varchar(45) DEFAULT NULL,
                    `pis_cst_saida` varchar(45) DEFAULT NULL,
                    `pis_aliquota_entrada` varchar(45) DEFAULT NULL,
                    `pis_aliquota_saida` varchar(45) DEFAULT NULL,
                    `pis_natureza_receita` varchar(45) DEFAULT NULL,
                    `cofins_cst_entrada` varchar(45) DEFAULT NULL,
                    `cofins_cst_saida` varchar(45) DEFAULT NULL,
                    `cofins_aliquota_entrada` varchar(45) DEFAULT NULL,
                    `cofins_aliquota_saida` varchar(45) DEFAULT NULL,
                    `cofins_natureza_receita` varchar(45) DEFAULT NULL,
                    `ipi_cst_entrada` varchar(45) DEFAULT NULL,
                    `ipi_cst_saida` varchar(45) DEFAULT NULL,
                    `ipi_aliquota` varchar(45) DEFAULT NULL,
                    `ativo` tinyint NOT NULL DEFAULT '1',
                    PRIMARY KEY (`Id`)
                ) ENGINE=InnoDB AUTO_INCREMENT=16655 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;";
            _conn.Query(sql);
        }

        public void CriarTableC170()
        {
            var sql = $@"
                        USE {bdName};

                          CREATE TABLE `c170` (
                          `Id` int NOT NULL,
                          `REG` varchar(45) DEFAULT NULL,
                          `NUM_ITEM` varchar(45) DEFAULT NULL,
                          `COD_ITEM` varchar(45) DEFAULT NULL,
                          `DESCR_COMPL` varchar(45) DEFAULT NULL,
                          `QTD` varchar(45) DEFAULT NULL,
                          `UNID` varchar(45) DEFAULT NULL,
                          `VL_ITEM` varchar(45) DEFAULT NULL,
                          `VL_DESC` varchar(45) DEFAULT NULL,
                          `IND_MOV` varchar(45) DEFAULT NULL,
                          `CST_ICMS` varchar(45) DEFAULT NULL,
                          `CFOP` varchar(45) DEFAULT NULL,
                          `COD_NAT` varchar(45) DEFAULT NULL,
                          `VL_BC_ICMS` varchar(45) DEFAULT NULL,
                          `ALIQ_ICMS` varchar(45) DEFAULT NULL,
                          `VL_ICMS` varchar(45) DEFAULT NULL,
                          `VL_BC_ICMS_ST` varchar(45) DEFAULT NULL,
                          `ALIQ_ST` varchar(45) DEFAULT NULL,
                          `VL_ICMS_ST` varchar(45) DEFAULT NULL,
                          `IND_APUR` varchar(45) DEFAULT NULL,
                          `CST_IPI` varchar(45) DEFAULT NULL,
                          `COD_ENQ` varchar(45) DEFAULT NULL,
                          `VL_BC_IPI` varchar(45) DEFAULT NULL,
                          `ALIQ_IPI` varchar(45) DEFAULT NULL,
                          `VL_IPI` varchar(45) DEFAULT NULL,
                          `CST_PIS` varchar(45) DEFAULT NULL,
                          `CST_PIS_TRATADO` varchar(45) DEFAULT NULL,
                          `VL_BC_PIS` varchar(45) DEFAULT NULL,
                          `ALIQ_PIS` varchar(45) DEFAULT NULL,
                          `QUANT_BC_PIS` varchar(45) DEFAULT NULL,
                          `ALIQ_PIS_QUANT` varchar(45) DEFAULT NULL,
                          `VL_PIS` varchar(45) DEFAULT NULL,
                          `CST_COFINS` varchar(45) DEFAULT NULL,
                          `CST_COFINS_TRATADO` varchar(45) DEFAULT NULL,
                          `VL_BC_COFINS` varchar(45) DEFAULT NULL,
                          `ALIQ_COFINS` varchar(45) DEFAULT NULL,
                          `QUANT_BC_COFINS` varchar(45) DEFAULT NULL,
                          `ALIQ_COFINS_QUANT` varchar(45) DEFAULT NULL,
                          `VL_COFINS` varchar(45) DEFAULT NULL,
                          `COD_CTA` varchar(45) DEFAULT NULL,
                          PRIMARY KEY (`Id`)
                        ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;";
            _conn.Query(sql);
        }

        public void CriarTableBloco0000()
        {
            var sql = $@"
                        USE {bdName};

                          CREATE TABLE `bloco0000` (
                          `Id` int NOT NULL AUTO_INCREMENT,
                          `REG` varchar(45) DEFAULT NULL,
                          `COD_VER` varchar(45) DEFAULT NULL,
                          `TIPO_ESCRIT` varchar(45) DEFAULT NULL,
                          `IND_SIT_ESP` varchar(45) DEFAULT NULL,
                          `NUM_REC_ANTERIOR` varchar(45) DEFAULT NULL,
                          `DT_INI` varchar(45) DEFAULT NULL,
                          `DT_FIN` varchar(45) DEFAULT NULL,
                          `NOME` varchar(250) DEFAULT NULL,
                          `CNPJ` varchar(45) DEFAULT NULL,
                          `UF` varchar(45) DEFAULT NULL,
                          `COD_MUN` varchar(45) DEFAULT NULL,
                          `SUFRAMA` varchar(45) DEFAULT NULL,
                          `IND_NAT_PJ` varchar(45) DEFAULT NULL,
                          `IND_ATIV` varchar(45) DEFAULT NULL,
                          `DataCadastro` datetime NOT NULL,
                          `Ativo` bit(1) NOT NULL,
                          PRIMARY KEY (`Id`)
                        ) ENGINE=InnoDB AUTO_INCREMENT=29 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;";
            _conn.Query(sql);
        }
    }
}
