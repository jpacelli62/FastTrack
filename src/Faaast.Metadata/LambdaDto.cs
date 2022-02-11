using System;
using Faaast.Metadata;

namespace Faaast.Metadata
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
