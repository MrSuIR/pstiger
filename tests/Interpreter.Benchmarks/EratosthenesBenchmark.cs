using System.Globalization;

using BenchmarkDotNet.Attributes;

using PsTiger.Interpreter;
using PsTiger.Tests.TestLibrary;
using PsTiger.Tests.TestLibrary.TestDoubles;

namespace PsTiger.Tests.Interpreter.Benchmarks;

[SimpleJob(launchCount: 1, warmupCount: 2, iterationCount: 10)]
public class EratosthenesBenchmark
{
    private string? _program;
    private TigerInterpreter? _interpreter;

    [Params(1000, 10000)]
    public int N { get; set; }

    [GlobalSetup]
    public void GlobalSetup()
    {
        _program = Samples.GetSampleProgram("eratosthenes.tig");
    }

    [IterationSetup]
    public void IterationSetup()
    {
        FakeEnvironment fakeEnvironment = new();
        fakeEnvironment.AddInput(N.ToString(CultureInfo.InvariantCulture));
        _interpreter = new TigerInterpreter(fakeEnvironment);
    }

    [Benchmark]
    public void ListPrimesUpTo()
    {
        _interpreter!.Execute(_program!);
    }

    [IterationCleanup]
    public void IterationCleanup()
    {
        _interpreter = null;
    }
}