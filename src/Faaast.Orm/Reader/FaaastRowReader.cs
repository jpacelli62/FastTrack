using System;

namespace Faaast.Orm.Reader
{
    public sealed class FaaastRowReader : BaseRowReader, IDisposable
    {
        public FaaastRowReader(FaaastCommand source) : base(source)
        {
        }

        public void Dispose() => this.Reader.Dispose();

        internal void Prepare()
        {
            this.Source.Command.CommandTimeout = this.Source.CommandTimeout ?? this.Source.Command.CommandTimeout;
            this.Reader = this.Source.Command.ExecuteReader(this.Source.CommandBehavior);
            this.InitFields();
        }

        public bool Read() => this.FillBuffer(this.Reader.Read());
    }
}
