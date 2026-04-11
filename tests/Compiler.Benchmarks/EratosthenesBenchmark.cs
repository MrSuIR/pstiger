using System.Globalization;

using BenchmarkDotNet.Attributes;

using PsTiger.Compiler.Drivers;
using PsTiger.Tests.TestLibrary;
using PsTiger.Tests.TestLibrary.Helpers;

namespace PsTiger.Tests.Compiler.Benchmarks;

[SimpleJob(launchCount: 1, warmupCount: 2, iterationCount: 10)]
public class EratosthenesBenchmark
{
    private const int RunTimeoutSeconds = 120;
    private static readonly TimeSpan RunTimeout = TimeSpan.FromSeconds(RunTimeoutSeconds);

    private string? _sourceCodePath;
    private TempFile? _compiledProgram;

    [Params(1000, 10000, 50000)]
    public int N { get; set; }

    [GlobalSetup]
    public async Task GlobalSetup()
    {
        _sourceCodePath = Samples.GetSampleProgramPath("programs/eratosthenes.tig");
        _compiledProgram = TempFile.CreateEmpty("program-", "*.exe");

        CompilerDriver driver = new();
        driver.Compile(_sourceCodePath, _compiledProgram.Path);

        await DotnetIlVerifyRunner.Run(_compiledProgram.Path);
    }

    [Benchmark]
    public async Task ListPrimesUpTo()
    {
        string input = N.ToString(CultureInfo.InvariantCulture);

        // Запускаем программу с таймаутом.
        CancellationTokenSource cts = new(RunTimeout);
        await DotnetConsoleProgramRunner.RunAndReadOutputWithCheck(
            _compiledProgram!.Path, input, cts.Token
        );
    }

    [GlobalCleanup]
    public void GlobalCleanup()
    {
        _compiledProgram?.Dispose();
    }
}