using Dapper.Contrib.Extensions;
using System;

namespace SysSped.Domain.Entities.CorrecaoSped
{
    [Table("bloco0000")]
    public class Bloco0000
    {
        [Key]
        public int Id { get; set; }
        public string REG { get; set; }
        public string COD_VER { get; set; }
        public string TIPO_ESCRIT { get; set; }
        public string IND_SIT_ESP { get; set; }
        public string NUM_REC_ANTERIOR { get; set; }
        public string DT_INI { get; set; }
        public string DT_FIN { get; set; }
        public string NOME { get; set; }
        public string CNPJ { get; set; }
        public string UF { get; set; }
        public string COD_MUN { get; set; }
        public string SUFRAMA { get; set; }
        public string IND_NAT_PJ { get; set; }
        public string IND_ATIV { get; set; }

        public DateTime DataCadastro { get; set; }
        public bool Ativo { get; set; }
    }
}
