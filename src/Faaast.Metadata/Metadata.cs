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

#pragma warning disable S2326 // Unused type parameters should be removed
    public class Metadata<TModel, TValue> : Metadata where TModel : class
#pragma warning restore S2326 // Unused type parameters should be removed
    {
        public Metadata(string name) : base(name)
        {
        }
    }
}
