namespace PsTiger.Execution;

#pragma warning disable RCS1194 // Конструкторы исключения не нужны, т.к. это не класс общего назначения.
/// <summary>
/// Исключение вызывается при аварийной остановке программы.
/// </summary>
public class ProgramAbortedException : Exception
{
    public ProgramAbortedException(string message)
        : base(message)
    {
    }
}
#pragma warning restore RCS1194