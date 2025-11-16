using PsTiger.Ast.Expressions;
using PsTiger.Execution;
using PsTiger.Parsing;
using PsTiger.Runtime;

namespace PsTiger.Interpreter;

public class TigerInterpreter
{
    public Value Execute(string code)
    {
        Parser parser = new(code);
        AstEvaluator evaluator = new();

        Expression expression = parser.ParseExpression();
        Value result = evaluator.Evaluate(expression);

        return result;
    }
}