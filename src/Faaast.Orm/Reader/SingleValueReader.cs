using System;

namespace Faaast.Orm.Reader
{
    public class SingleValueReader<T> : DataReader<T>
    {

        public Type NullType { get; set; }

        public SingleValueReader() => this.NullType = Nullable.GetUnderlyingType(typeof(T));

        public override void Read()
        {
            var value = this.RowReader.Buffer[this.Start];
            this.Value = value == DBNull.Value ? default : this.ReadValue(value);
        }

        private T ReadValue(object value)
        {
            if (this.NullType == null)
            {
                return (T)value;
            }
            else
            {
                return (T)Convert.ChangeType(value, this.NullType);
            }
        }
    }
}
