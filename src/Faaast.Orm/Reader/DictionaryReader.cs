using System;
using System.Collections.Generic;

namespace Faaast.Orm.Reader
{
    public class DictionaryReader : DataReader<Dictionary<string, object>>
    {
        public override void Read()
        {
            this.Value = new();
            for (var i = this.Start; i < this.End; i++)
            {
                var value = this.RowReader.Buffer[i];
                this.Value.Add(this.RowReader.Columns[i], value == DBNull.Value ? null : value);
            }
        }
    }
}
