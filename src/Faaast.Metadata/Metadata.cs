namespace Faaast.Metadata
{
    public class Metadata
    {
        public virtual string Name { get; set; }

        public Metadata(string name)
        {
            Name = name;
        }
    }

    public class Metadata<TValue> : Metadata
    {
        public Metadata(string name) : base(name)
        {
        }
    }
}
