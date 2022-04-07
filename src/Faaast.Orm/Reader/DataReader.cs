using System;

namespace Faaast.Orm.Reader
{
    public abstract class DataReader<T> : DataReader
    {
        public T Value { get; set; }

        public virtual DataReader<T> Distinct()
        {
            throw new NotImplementedException("Only available while reading DTO classes");
        }

        public virtual DataReader<TChild> ExtendedBy<TChild>() where TChild : T
        {
            throw new NotImplementedException("Only available while reading DTO classes");
        }
    }

    public abstract class DataReader
    {
        internal BaseRowReader RowReader { get; set; }

        public int Start { get; set; }

        public virtual int End { get; set; }

        public abstract void Read();
    }
}
