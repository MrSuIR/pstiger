using PsTiger.Ast;
using PsTiger.Ast.Declarations;
using PsTiger.Ast.Expressions;
using PsTiger.Semantics.Passes;
using PsTiger.Semantics.Symbols;

namespace PsTiger.Semantics;

/// <summary>
/// Класс для проверки семантики программы.
/// Реализован как фасад над несколькими проходами (passes), каждый из которых реализует шаблон «Посетитель» (Visitor).
/// </summary>
public class SemanticsChecker
{
    private readonly AbstractPass[] _passes;

    public SemanticsChecker(
        IReadOnlyList<BuiltinFunction> builtinFunctions,
        IReadOnlyList<BuiltinType> builtinTypes
    )
    {
        SymbolsTable globalSymbols = new(parent: null);
        foreach (BuiltinFunction function in builtinFunctions)
        {
            globalSymbols.DeclareFunction(function);
        }

        foreach (BuiltinType type in builtinTypes)
        {
            globalSymbols.DeclareType(type);
        }

        _passes =
        [
            new ResolveNamesPass(globalSymbols),
            new CheckContextSensitiveRulesPass(),
            new ResolveTypesPass(),
            new CheckTypesPass(),
        ];
    }

    public void Check(Expression program)
    {
        foreach (AbstractPass pass in _passes)
        {
            program.Accept(pass);
        }
    }
}