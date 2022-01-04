namespace Faaast.Metadata
{
    public class Metadata
    {
        public virtual string Name { get; set; }

        public Metadata(string name) => this.Name = name;
    }

    public class Metadata<TModel, TValue> : Metadata
    {
        public Metadata(string name) : base(name)
        {
        }
    }
}
