using System.Reflection;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Faaast.Metadata;
using Sample.Metadata;

namespace Benchmark
{
    //[SimpleJob(runtimeMoniker: RuntimeMoniker.Net461)]
    [SimpleJob(runtimeMoniker: RuntimeMoniker.Net50)]
    public class BenchmarkPropertySetter
    {
        private readonly MethodInfo _reflexion;
        private DtoProperty Lambda { get; set; }

        private readonly Customer _current;
        public BenchmarkPropertySetter()
        {
            _current = new Customer();
            _reflexion = typeof(Customer).GetProperty(nameof(Customer.Id)).GetSetMethod();

            var lambdaMapper = new ObjectMapper();
            var lambdaDto = lambdaMapper.Get(typeof(Customer));
            this.Lambda = lambdaDto[nameof(Customer.Id)];

            //IObjectMapper emitMapper = new EmitObjectMapper();
            //var emitDto = emitMapper.Get(typeof(Customer));
            //this.Emit = emitDto[nameof(Customer.Id)];
        }

        [Benchmark]
        public void Reflexion() => _reflexion.Invoke(_current, new object[] { 1 });

        [Benchmark(Description = "DefaultObjectMapper from Faaast.Metadata", Baseline = true)]
        public void DefaultObjectMapper() => this.Lambda.Write(_current, 1);

        //[Benchmark(Description = "EmitObjectMapper from Faaast.Metadata")]
        //public void EmitObjectMapper() => this.Emit.Write(_current, 1);

        [Benchmark(Description = "Direct call to the setter")]
        public void Set() => _current.Id = 1;

        public override string ToString() => _current.ToString();
    }
}
