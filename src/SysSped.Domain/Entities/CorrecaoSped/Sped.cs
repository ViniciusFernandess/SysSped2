using System.Collections.Generic;

namespace SysSped.Domain.Entities.CorrecaoSped
{
    public class Sped
    {
        public Bloco0000 Bloco0000 = new Bloco0000();
        public List<C100> BlocosC100 = new List<C100>();
        public List<Bloco0200> Blocos0200 = new List<Bloco0200>();
    }
}
