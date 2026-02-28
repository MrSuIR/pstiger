using PsTiger.Compiler.CommandLine;

return CompilerApplication.Run(
    args: args,
    stdoutWriter: Console.Out,
    stderrWriter: Console.Error
);