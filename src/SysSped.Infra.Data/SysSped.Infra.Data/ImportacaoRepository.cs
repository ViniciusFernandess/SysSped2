using Dapper;
using Dapper.Contrib.Extensions;
using SysSped.Domain.Entities.Importacao;
using SysSped.Domain.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace SysSped.Infra.Data
{
    public class ImportacaoRepository : BaseRepository, IImportacaoRepository
    {
        private List<RowImportacao> _importacaoAtiva;
        private Dictionary<string, RowImportacao> _dicImportacaoAtivaPorCod_interno;
        private Dictionary<string, RowImportacao> _dicImportacaoAtivaPorEan;
        public bool temCodItemRepetidoNaBase = false;
        public bool temEanRepetidoNaBase = false;

        public ImportacaoRepository() : base()
        {
            AtualizarDics();
        }

        public void Importar(ArquivoImportacao model)
        {
            foreach (var row in model.Rows)
            {
                _conn.Insert(row);
            }

            AtualizarDics();
        }

        public void AtualizarDics()
        {
            //Atualiza dicionario após atualizar base
            _importacaoAtiva = ObterImportacaoAtiva();

            try { _dicImportacaoAtivaPorCod_interno = _importacaoAtiva.ToDictionary(x => x.codigointerno); } catch (System.Exception) { temCodItemRepetidoNaBase = true; }

            try { _dicImportacaoAtivaPorEan = _importacaoAtiva.ToDictionary(x => x.ean); } catch (System.Exception) { temEanRepetidoNaBase = true; }

        }

        public void InativarImportacaoAtual()
        {
            var sql = @"UPDATE `sysspeddb`.`rowimportacao` SET ativo = 0 WHERE ativo = 1";

            var retorno = _conn.Query(sql).ToList();
        }

        public string ObterAliquotaCofinsProdutoPorCod_Item(string cod_item)
        {
            _dicImportacaoAtivaPorCod_interno.TryGetValue(cod_item, out var retorno);

            return retorno?.cofins_aliquota_entrada ?? "";

            //var sql = $@"SELECT cofins_aliquota_entrada FROM `sysspeddb`.`rowimportacao` WHERE codigoInterno = {cod_item}";

            //var retorno = _conn.Query<string>(sql).FirstOrDefault();

            //return retorno;
        }

        public string ObterAliquotaPisProdutoPorCod_Item(string cod_item)
        {
            _dicImportacaoAtivaPorCod_interno.TryGetValue(cod_item, out var retorno);

            return retorno?.pis_aliquota_entrada ?? "";

            //var sql = $@"SELECT pis_aliquota_entrada FROM `sysspeddb`.`rowimportacao` WHERE codigoInterno = {cod_item}";

            //var retorno = _conn.Query<string>(sql).FirstOrDefault();

            //return retorno;
        }

        public string ObterCST_CofinsProdutoPorCod_Item(string cod_item)
        {
            _dicImportacaoAtivaPorCod_interno.TryGetValue(cod_item, out var retorno);

            return retorno?.cofins_cst_entrada ?? "";

            //var sql = $@"SELECT cofins_cst_entrada FROM `sysspeddb`.`rowimportacao` WHERE codigoInterno = {cod_item} AND Ativo = 1";

            //var retorno = _conn.Query<string>(sql).FirstOrDefault();

            //return retorno;
        }

        public string ObterCST_PisProdutoPorCod_Item(string cod_item)
        {
            _dicImportacaoAtivaPorCod_interno.TryGetValue(cod_item, out var retorno);

            return retorno?.pis_cst_entrada ?? "";

            //var sql = $@"SELECT pis_cst_entrada FROM `sysspeddb`.`rowimportacao` WHERE codigoInterno = {cod_item} AND Ativo = 1";

            //var retorno = _conn.Query<string>(sql).FirstOrDefault();

            //return retorno;
        }

        public bool VerificarSeTemImportacaoAtiva()
        {
            var sql = @"SELECT * FROM `sysspeddb`.`rowimportacao` WHERE ativo = 1";

            var retorno = _conn.Query<RowImportacao>(sql).ToList();

            return retorno.Any();
        }

        public List<RowImportacao> ObterImportacaoAtiva()
        {
            // obtem os eans repetidos na base para remover, mantendo só o mais recente inserido na base
            var sql = @"with cte as (
	                        select id, row_number() Over(partition by ean order by id desc) as r from `sysspeddb`.`rowimportacao` where ativo = 1
                        ) select id from cte where r > 1";

            var ids = _conn.Query<int>(sql).ToList();

            if (ids.Count > 0)
            {
                sql = $@"update `sysspeddb`.`rowimportacao` set ativo = 0 where id in ({string.Join(",", ids)})";
                _conn.Execute(sql);
            }

            sql = @"SELECT * FROM `sysspeddb`.`rowimportacao` WHERE ativo = 1";

            var retorno = _conn.Query<RowImportacao>(sql).ToList();

            return retorno;
        }

        public RowImportacao ObterPorCodItem(string codInternoItem)
        {
            _dicImportacaoAtivaPorCod_interno.TryGetValue(codInternoItem, out var retorno);

            return retorno;

            //var sql = $@"SELECT * FROM `sysspeddb`.`rowimportacao` WHERE ativo = 1 AND codigointerno = {codInternoItem}";

            //var retorno = _conn.Query<RowImportacao>(sql).FirstOrDefault();

            //return retorno;
        }

        public RowImportacao ObterPorEanItem(string eanItem)
        {
            _dicImportacaoAtivaPorEan.TryGetValue(eanItem, out var retorno);

            return retorno;

            //var sql = $@"SELECT * FROM `sysspeddb`.`rowimportacao` WHERE ean = {eanItem} AND Ativo = 1";

            //var retorno = _conn.Query<RowImportacao>(sql).FirstOrDefault();

            //return retorno;
        }

        public void InativarRow(RowImportacao rowBD)
        {
            rowBD.ativo = "0";
            _conn.Update(rowBD);
        }

        public void InserirRowAtualizada(RowImportacao rowPlanilha)
        {
            _conn.Insert(rowPlanilha);
        }

        public string ObterCST_PisProdutoPorEan(string ean)
        {
            _dicImportacaoAtivaPorEan.TryGetValue(ean, out var retorno);

            return retorno?.pis_cst_entrada ?? "";

            //var sql = $@"SELECT pis_cst_entrada FROM `sysspeddb`.`rowimportacao` WHERE ean = {ean} AND Ativo = 1";

            //var retorno = _conn.Query<string>(sql).FirstOrDefault();

            //return retorno;
        }

        public string ObterCST_CofinsProdutoPorEan(string ean)
        {
            _dicImportacaoAtivaPorEan.TryGetValue(ean, out var retorno);

            return retorno?.cofins_cst_entrada ?? "";

            //var sql = $@"SELECT cofins_cst_entrada FROM `sysspeddb`.`rowimportacao` WHERE ean = {ean} AND Ativo = 1";

            //var retorno = _conn.Query<string>(sql).FirstOrDefault();

            //return retorno;
        }

        public string ObterAliquotaPisProdutoPorEan(string ean)
        {
            _dicImportacaoAtivaPorEan.TryGetValue(ean, out var retorno);

            return retorno?.pis_aliquota_entrada ?? "";

            //var sql = $@"SELECT pis_aliquota_entrada FROM `sysspeddb`.`rowimportacao` WHERE ean = {ean}";

            //var retorno = _conn.Query<string>(sql).FirstOrDefault();

            //return retorno;
        }

        public string ObterAliquotaCofinsProdutoPorEan(string ean)
        {
            _dicImportacaoAtivaPorEan.TryGetValue(ean, out var retorno);

            return retorno?.cofins_aliquota_entrada ?? "";

            //var sql = $@"SELECT cofins_aliquota_entrada FROM `sysspeddb`.`rowimportacao` WHERE ean = {ean}";

            //var retorno = _conn.Query<string>(sql).FirstOrDefault();

            //return retorno;
        }

        public string ObterCST_PisProdutoPorNcm(string ncm)
        {
            var retorno = _importacaoAtiva.FirstOrDefault(x => x.ncm == ncm)?.pis_cst_entrada ?? "";

            return retorno;

            //var sql = $@"SELECT pis_cst_entrada FROM `sysspeddb`.`rowimportacao` WHERE ncm = '{ncm}' AND Ativo = 1";

            //var retorno = _conn.Query<string>(sql).FirstOrDefault();

            //return retorno;
        }

        public string ObterCST_CofinsProdutoPorNcm(string ncm)
        {
            var retorno = _importacaoAtiva.FirstOrDefault(x => x.ncm == ncm)?.cofins_cst_entrada ?? "";

            return retorno;

            //var sql = $@"SELECT cofins_cst_entrada FROM `sysspeddb`.`rowimportacao` WHERE ncm = '{ncm}' AND Ativo = 1";

            //var retorno = _conn.Query<string>(sql).FirstOrDefault();

            //return retorno;
        }

        public string ObterAliquotaPisProdutoPorNcm(string ncm)
        {
            var retorno = _importacaoAtiva.FirstOrDefault(x => x.ncm == ncm)?.pis_aliquota_entrada ?? "";

            return retorno;

            //var sql = $@"SELECT pis_aliquota_entrada FROM `sysspeddb`.`rowimportacao` WHERE ncm = '{ncm}'";

            //var retorno = _conn.Query<string>(sql).FirstOrDefault();

            //return retorno;
        }

        public string ObterAliquotaCofinsProdutoPorNcm(string ncm)
        {
            var retorno = _importacaoAtiva.FirstOrDefault(x => x.ncm == ncm)?.cofins_aliquota_entrada ?? "";

            return retorno;

            //var sql = $@"SELECT cofins_aliquota_entrada FROM `sysspeddb`.`rowimportacao` WHERE ncm = '{ncm}'";

            //var retorno = _conn.Query<string>(sql).FirstOrDefault();

            //return retorno;
        }
    }
}
