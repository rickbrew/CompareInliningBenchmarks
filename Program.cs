using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompareInliningBenchmarks;

public static class Program
{
    public static void Main(string[] args)
    {
        BenchmarkRunner.Run<SortFloatsBenchmark>();
        BenchmarkRunner.Run<SortDoublesBenchmark>();
    }
}
