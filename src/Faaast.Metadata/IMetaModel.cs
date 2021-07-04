namespace Faaast.Metadata
{
    public interface IMetaModel<TModel> where TModel : class
    {
        T Get<T>(Metadata<TModel, T> metadata);

        void Set<T>(Metadata<TModel, T> metadata, T value);
    }
}
