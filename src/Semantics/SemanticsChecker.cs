using PsTiger.Ast;
using PsTiger.Ast.Declarations;
using PsTiger.Ast.Expressions;

namespace Semantics;

/// <summary>
/// Класс для проверки семантики программы.
/// Реализован как фасад над несколькими проходами (passes), каждый из которых реализует шаблон «Посетитель» (Visitor).
/// </summary>
public class SemanticsChecker
{
    private readonly List<IAstVisitor> _passes;

    public SemanticsChecker(IReadOnlyDictionary<string, BuiltinFunction> builtins)
    {
        _passes =
        [
            new FunctionsChecker(builtins),
            new TypeChecker(builtins),
        ];
    }

    public void Check(Expression program)
    {
        foreach (IAstVisitor pass in _passes)
        {
            program.Accept(pass);
        }
    }
}