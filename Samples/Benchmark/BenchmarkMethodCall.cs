using System.Reflection;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Faaast.Metadata;
using Sample.Metadata;

namespace Benchmark
{
    public class SampleClass
    {
        public int Value { get; set; }

        public string GetText()
        {
            return $"hello{this.Value}";
        }
    }

    public static class SampleClassExtensions
    {
        public static string GetTextExtension(this SampleClass instance)
        {
            return $"hello{instance.Value}";
        }
    }

    //[SimpleJob(runtimeMoniker: RuntimeMoniker.Net461)]
    [SimpleJob(runtimeMoniker: RuntimeMoniker.Net50)]
    public class BenchmarkMethodCall
    {

        public SampleClass Instance { get; set; }
        public string value { get; set; }
        public BenchmarkMethodCall()
        {
            this.Instance = new SampleClass();
        }

        [Benchmark]
        public void Direct()
        {
            for (int i = 0; i < 1000000; i++)
            {
                this.value = this.Instance.GetText();
            }
        }

        [Benchmark]
        public void Extension()
        {
            for (int i = 0; i < 1000000; i++)
            {
                this.value = this.Instance.GetTextExtension();
            }
        }
    }
}
