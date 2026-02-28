using System.Text;

using PsTiger.Compiler.CommandLine;
using PsTiger.VirtualMachine;

using Xunit.Sdk;

namespace PsTiger.Tests.Compiler.Specs.Drivers;

public class CompilerTestDriver
{
    public void RunCompiler(string inputPath, string outputPath)
    {
        StringWriter stdoutWriter = new();
        StringWriter stderrWriter = new();

        int exitCode = CompilerApplication.Run(
            args:
            [
                inputPath,
                "--output",
                outputPath,
            ],
            stdoutWriter: stdoutWriter,
            stderrWriter: stderrWriter
        );

        if (exitCode != 0)
        {
            StringBuilder sb = new();
            sb.Append("Compilation failed with exit code ");
            sb.Append(exitCode);

            string stdout = stdoutWriter.ToString();
            if (stdout != string.Empty)
            {
                sb.AppendLine();
                sb.AppendLine("Stdout:");
                sb.Append(stdout);
            }

            string stderr = stderrWriter.ToString();
            if (stderr != string.Empty)
            {
                sb.AppendLine();
                sb.AppendLine("Stderr:");
                sb.Append(stderr);
            }

            throw FailException.ForFailure(sb.ToString());
        }
    }
}