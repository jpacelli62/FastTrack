using System;
using System.Reflection;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Faaast.Metadata;
using Sample.Metadata;

namespace Benchmark
{
    [SimpleJob(runtimeMoniker: RuntimeMoniker.Net461)]
    [SimpleJob(runtimeMoniker: RuntimeMoniker.Net50)]
    //[RPlotExporter]
    public class BenchmarkInstanceCreation
    {
        private readonly ConstructorInfo _ctor;
        private IDtoClass LambdaDto { get; set; }
        private IDtoClass EmitDto { get; set; }

        private object _current;
        public BenchmarkInstanceCreation()
        {
            _ctor = typeof(Customer).GetConstructor(Type.EmptyTypes);

            IObjectMapper lambdaMapper = new DefaultObjectMapper();
            this.LambdaDto = lambdaMapper.Get(typeof(Customer));

            //IObjectMapper emitMapper = new EmitObjectMapper();
            //this.EmitDto = emitMapper.Get(typeof(Customer));
        }

        [Benchmark]
        public void Reflexion() => _current = _ctor.Invoke(null);

        [Benchmark]
        public void Activator() => _current = System.Activator.CreateInstance(typeof(Customer));

        [Benchmark(Description = "DefaultObjectMapper from Faaast.Metadata", Baseline = true)]
        public void IObjectMapper() => _current = this.LambdaDto.CreateInstance();

        //[Benchmark(Description = "EmitObjectMapper from Faaast.Metadata")]
        //public void IObjectMapperDirect() => _current = this.EmitDto.CreateInstance();

        [Benchmark(Description = "Direct call to the constructor")]
        public void New() => _current = new Customer();

        public override string ToString() => _current.ToString();
    }
}
