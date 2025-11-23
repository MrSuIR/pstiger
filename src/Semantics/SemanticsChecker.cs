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
        IReadOnlyDictionary<string, BuiltinFunction> builtinFunctions,
        IReadOnlyDictionary<string, BuiltinType> builtinTypes
    )
    {
        SymbolsTable globalSymbols = new(parent: null);
        foreach ((string name, BuiltinFunction function) in builtinFunctions)
        {
            globalSymbols.DefineSymbol(name, function);
        }

        foreach ((string name, BuiltinType type) in builtinTypes)
        {
            globalSymbols.DefineSymbol(name, type);
        }

        _passes =
        [
            new ResolveNamesPass(globalSymbols),
            new CheckContextSensitiveRulesPass(),
            new ResolveTypesPass(),
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