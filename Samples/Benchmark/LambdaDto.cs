using System;
using Faaast.Metadata;

namespace Benchmark
{
    public class LambdaDto : DtoClass
    {
        public LambdaDto(Type type) : base(type)
        {
        }

        public Func<object> Lambda{ get; set; }
        public override object CreateInstance() => this.Lambda();
    }
}
