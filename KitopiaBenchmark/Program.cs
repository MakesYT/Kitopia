using BenchmarkDotNet.Running;

namespace KitopiaBenchmark;

class Program
{
    static void Main(string[] args)
    {
        BenchmarkRunner.Run<NameSolver>();
    }
}