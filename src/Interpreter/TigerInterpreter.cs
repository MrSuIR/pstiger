using Codegen;

using PsTiger.Ast.Expressions;
using PsTiger.Execution;
using PsTiger.Parsing;
using PsTiger.Runtime;
using PsTiger.Semantics;
using PsTiger.VirtualMachine;

namespace PsTiger.Interpreter;

public class TigerInterpreter
{
    private readonly IEnvironment _environment;
    private readonly Builtins _builtins;
    private int _exitCode;

    public TigerInterpreter(IEnvironment environment)
    {
        _environment = environment;
        _builtins = new Builtins(environment);
    }

    public int ExitCode => _exitCode;

    public Value Execute(string code)
    {
        // 1. Разбор программы.
        Parser parser = new(code);
        Expression program = parser.ParseProgram();

        // 2. Проверка соответствия типов в программе.
        SemanticsChecker checker = new(_builtins.Functions, _builtins.Types);
        checker.Check(program);

        // 3. Генерация кода для виртуальной машины.
        TigerVmCodegen codegen = new();
        List<Instruction> instructions = codegen.GenerateCode(program);

        // 4. Исполнение программы на виртуальной машине.
        TigerVm vm = new(_environment, instructions);
        Value result = vm.RunProgram();
        _exitCode = vm.ExitCode;

        return result;
    }
}