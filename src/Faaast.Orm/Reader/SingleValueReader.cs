using System;

namespace Faaast.Orm.Reader
{
    public class SingleValueReader<T> : DataReader<T>
    {
        public override void Read()
        {
            var value = this.RowReader.Buffer[this.Start];
            this.Value = value == DBNull.Value ? default : (T)value;
        }
    }
}
