namespace PsTiger.Execution;

#pragma warning disable RCS1194 // Конструкторы исключения не нужны, т.к. это не класс общего назначения.
/// <summary>
/// Исключение вызывается при раннем выходе из программы.
/// </summary>
public class ProgramExitedException : Exception
{
    public ProgramExitedException(int code)
        : base($"Program exited with code {code}")
    {
        ExitCode = code;
    }

    public int ExitCode { get; }
}
#pragma warning restore RCS1194