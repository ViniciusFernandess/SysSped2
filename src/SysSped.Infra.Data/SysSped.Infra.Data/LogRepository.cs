using Dapper;
using Dapper.Contrib.Extensions;
using SysSped.Domain.Core;
using SysSped.Domain.Entities.CorrecaoSped;
using SysSped.Domain.Entities.Relatorio;
using SysSped.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SysSped.Infra.Data
{
    public class LogRepository : BaseRepository, ILogRepository
    {
        public LogRepository() : base()
        {

        }

        public void RegistrarLog(Bloco0000 bloco0000, EnumTipoSped tipoBloco, int indiceCampo, string nomeCampo, int indiceLinha, string codigoInterno, string ean, string campoAntigo, string campoNovo, string regra = "")
        {
            var sql = $@"
                        INSERT INTO 
                        `sysspeddb`.`logalteracaosped` 
                        (idbloco0000, nrolinha, indiceCampo, nomeCampo, codigoInterno, ean, tiposped, valorantigo, valoratual, datacadastro, ativo, regra) 
                        VALUES 
                        ({bloco0000.Id}, {indiceLinha + 1}, {indiceCampo}, '{nomeCampo}', '{codigoInterno}', '{ean}', {(int)tipoBloco}, '{campoAntigo}', '{campoNovo}', NOW(), 1, '{regra}')";

            _conn.Query(sql);
        }

        public int RegistrarLogBloco0000(Bloco0000 bloco0000)
        {
            bloco0000.Ativo = true;
            bloco0000.DataCadastro = DateTime.Now;
            return (int)_conn.Insert(bloco0000);
        }

        public List<AlteracoesSped> ObterAlteracoesPorIdBloco0000(IEnumerable<int> ids)
        {
            var sql = $@"
                        SELECT 
	                        logg.Id, 
                            logg.IdBloco0000,
                            Logg.NroLinha, 
                            tipo.Titulo as Bloco,
                            logg.CodigoInterno,
                            logg.Ean,
                            logg.IndiceCampo,
                            logg.NomeCampo,
                            logg.ValorAntigo, 
                            logg.ValorAtual, 
                            logg.DataCadastro, 
                            logg.Ativo, 
                            logg.Regra
                        FROM 
	                        `sysspeddb`.`logalteracaosped` AS logg
	                        INNER JOIN `sysspeddb`.`tiposped` AS tipo ON tipo.Id = logg.tipoSped
                        WHERE 
                            IdBloco0000 IN ({string.Join(",", ids)})
                        ORDER BY logg.Id
                        ";

            var retorno = _conn.Query<AlteracoesSped>(sql).ToList();

            return retorno;
        }

        public IEnumerable<Bloco0000> ObterBloco0000Ativos()
        {
            var sql = "SELECT * FROM sysspeddb.bloco0000 WHERE ativo = 1;";

            var retorno = _conn.Query<Bloco0000>(sql).ToList();

            return retorno;
        }

        public IEnumerable<Bloco0000> ObterBloco0000Ativos(int idBloco0000)
        {
            var sql = $@"SELECT * FROM sysspeddb.bloco0000 WHERE ativo = 1 AND Id = {idBloco0000};";

            var retorno = _conn.Query<Bloco0000>(sql).ToList();

            return retorno;
        }

        public IEnumerable<Bloco0000> ObterBloco0000Ativos(IEnumerable<int> idBsloco0000)
        {
            var sql = $@"SELECT * FROM sysspeddb.bloco0000 WHERE ativo = 1 AND Id in ({String.Join(",", idBsloco0000)});";

            var retorno = _conn.Query<Bloco0000>(sql).ToList();

            return retorno;
        }
    }
}
