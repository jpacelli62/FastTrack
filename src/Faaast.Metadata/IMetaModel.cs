namespace Faaast.Metadata
{
    public interface IMetaModel<TModel>
    {
        T Get<T>(Metadata<TModel, T> metadata);

        void Set<T>(Metadata<TModel, T> metadata, T value);

        bool Has<T>(Metadata<TModel, T> metadata);
    }
}
