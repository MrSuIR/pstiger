using System.CommandLine;

using PsTiger.Compiler.Drivers;

namespace PsTiger.Compiler.CommandLine;

/// <summary>
/// Фабрика для создания интерфейса командной строки компилятора.
/// Построена на базе библиотеки "System.CommandLine" от Microsoft.
/// </summary>
public static class RootCommandFactory
{
    public static RootCommand Create()
    {
        RootCommand command = new(
            "Compiles Tiger program into .NET executable"
        );

        Argument<string> inputPathArgument = new("input")
        {
            Description = "Path to Tiger program source file", Arity = ArgumentArity.ExactlyOne,
        };
        command.Add(inputPathArgument);

        Option<string?> outputPathOption = new("output")
        {
            Description = "Output path for generated .NET executable", Required = true,
        };
        command.Add(outputPathOption);

        command.SetAction((result) =>
        {
            string inputPath = result.GetRequiredValue(inputPathArgument);
            string? outputPath = result.GetRequiredValue(outputPathOption);

            return Compile(inputPath, outputPath);
        });

        return command;
    }

    private static int Compile(string inputPath, string? outputPath)
    {
        if (string.IsNullOrEmpty(outputPath))
        {
            outputPath = Path.ChangeExtension(inputPath, ".exe");
        }

        CompilerDriver driver = new();
        driver.Compile(inputPath, outputPath);

        return 0;
    }
}