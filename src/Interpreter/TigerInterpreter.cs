using PsTiger.Ast.Expressions;
using PsTiger.Execution;
using PsTiger.Parsing;
using PsTiger.Runtime;
using PsTiger.Semantics;

namespace PsTiger.Interpreter;

public class TigerInterpreter
{
    private readonly Builtins _builtins;
    private int _exitCode;

    public TigerInterpreter(IEnvironment environment)
    {
        _builtins = new Builtins(environment);
    }

    public int ExitCode => _exitCode;

    public Value Execute(string code)
    {
        // 1. Разбор программы.
        Parser parser = new(code);
        Expression program = parser.ParseProgram();

        // 2. Проверка соответствия типов в программе.
        SemanticsChecker checker = new(_builtins.Functions);
        checker.Check(program);

        // 3. Исполнение программы.
        AstEvaluator evaluator = new(_builtins.Functions);
        Value result = new();
        try
        {
            result = evaluator.Evaluate(program);
        }
        catch (ProgramExitedException e)
        {
            _exitCode = e.ExitCode;
        }

        return result;
    }
}