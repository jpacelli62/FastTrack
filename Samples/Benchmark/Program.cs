using System;
using BenchmarkDotNet.Running;

namespace Benchmark
{
    public static class Program
    {
        public static void Main()
        {
            BenchmarkRunner.Run<BenchmarkInstanceCreation>();
            BenchmarkRunner.Run<BenchmarkPropertySetter>();

            Console.ReadLine();
        }
    }
}
