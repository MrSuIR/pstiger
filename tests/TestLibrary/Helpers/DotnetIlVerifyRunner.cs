namespace PsTiger.Tests.TestLibrary.Helpers;

public static class DotnetIlVerifyRunner
{
    public static async Task<string> Run(string executablePath, CancellationToken ct)
    {
        if (!File.Exists(executablePath))
        {
            throw new FileNotFoundException($"Executable file not found: {executablePath}");
        }

        string workingDirectory = Path.GetDirectoryName(executablePath)!;
        List<string> command =
        [
            "ilverify",
            executablePath,
            "-r",
            "/usr/lib/dotnet/shared/Microsoft.NETCore.App/10.0.1/*.dll",
        ];

        ConsoleSubprocessRunner runner = new(
            workingDirectory: workingDirectory
        );

        await runner.Run(command, ct);
        runner.ThrowOnNonZeroExitCode();

        return runner.Stdout;
    }
}