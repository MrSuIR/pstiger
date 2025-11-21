using PsTiger.Ast.Expressions;
using PsTiger.Execution;
using PsTiger.Parsing;
using PsTiger.Runtime;

using Semantics;

namespace PsTiger.Interpreter;

public class TigerInterpreter
{
    public Value Execute(string code)
    {
        // 1. Разбор программы.
        Parser parser = new(code);
        Expression program = parser.ParseProgram();

        // 2. Проверка соответствия типов в программе.
        TypeChecker typeChecker = new(Builtins.Functions);
        program.Accept(typeChecker);

        // 3. Исполнение программы.
        AstEvaluator evaluator = new(Builtins.Functions);
        Value result = evaluator.Evaluate(program);

        return result;
    }
}