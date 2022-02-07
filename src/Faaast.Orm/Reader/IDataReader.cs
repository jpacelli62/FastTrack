namespace Faaast.Orm.Reader
{
    public abstract class DataReader<T> : DataReader
    {
        public T Value { get; set; }
    }

    public abstract class DataReader
    {
        internal BaseRowReader RowReader { get; set; }

        public int Start { get; set; }

        public virtual int End { get; set; }

        public abstract void Read();
    }
}
