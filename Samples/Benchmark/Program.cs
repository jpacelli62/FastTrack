using System;
using BenchmarkDotNet.Running;

namespace Benchmark
{
    class Program
    {
        static void Main()
        {
            BenchmarkRunner.Run<BenchmarkInstanceCreation>();
            BenchmarkRunner.Run<BenchmarkPropertySetter>();

            Console.ReadLine();
        }
    }
}
