namespace SysSped.Domain.Entities.CorrecaoSped
{
    public abstract class LinhaSpedBase<T>
    {
        public int IndiceArquivo { get; set; }
        public bool Tratado { get; set; }
        public string Regra { get; set; }

        public T ShallowCopy()
        {
            return (T)this.MemberwiseClone();
        }
    }
}
