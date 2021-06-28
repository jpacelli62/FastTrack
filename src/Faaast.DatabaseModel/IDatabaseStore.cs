namespace Faaast.DatabaseModel
{
    public interface IDatabaseStore
    {
        IDatabase this[string name]
        {
            get;
            set;
        }
    }
}
