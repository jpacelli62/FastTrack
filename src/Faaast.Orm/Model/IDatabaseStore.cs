namespace Faaast.Orm.Model
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
