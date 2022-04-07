namespace Faaast.Orm.Reader
{
    public class DtoExtendsReader<TParent, TChild> : DtoReader<TChild> 
        where TChild : TParent
    {
        public virtual DtoReader<TParent> ParentReader { get; set;}

        public DtoExtendsReader(BaseRowReader source, int start) : base(source, start)
        {
           
        }
        
        protected override void CreateInstance()
        {
            this.Value = (TChild)this.ParentReader.Value;
        }
    }
}
