using System.Diagnostics;

using PsTiger.Ast.Declarations;
using PsTiger.Semantics.Exceptions;

namespace PsTiger.Semantics.Symbols;

/// <summary>
/// Таблица символов, основанная на лексических областях видимости (областях действия) символов в коде.
/// </summary>
public sealed class SymbolsTable
{
    private readonly SymbolsTable? _parent;

    private readonly Dictionary<string, Declaration> _variablesAndFunctions;
    private readonly Dictionary<string, Declaration> _types;

    public SymbolsTable(SymbolsTable? parent)
    {
        _parent = parent;
        _variablesAndFunctions = [];
        _types = [];
    }

    public SymbolsTable? Parent => _parent;

    public AbstractVariableDeclaration GetVariableDeclaration(string name)
    {
        Declaration? declaration = FindDeclaration(table => table._variablesAndFunctions, name);
        return declaration switch
        {
            AbstractVariableDeclaration variable => variable,
            AbstractFunctionDeclaration _ => throw new InvalidSymbolException(name, "function", "variable"),
            null => throw UnknownSymbolException.UndefinedVariableOrFunction(name),
            _ => throw new UnreachableException(),
        };
    }

    public AbstractFunctionDeclaration GetFunctionDeclaration(string name)
    {
        Declaration? declaration = FindDeclaration(table => table._variablesAndFunctions, name);
        return declaration switch
        {
            AbstractFunctionDeclaration function => function,
            AbstractVariableDeclaration _ => throw new InvalidSymbolException(name, "function", "variable"),
            null => throw UnknownSymbolException.UndefinedVariableOrFunction(name),
            _ => throw new UnreachableException(),
        };
    }

    public AbstractTypeDeclaration GetTypeDeclaration(string name)
    {
        Declaration? declaration = FindDeclaration(table => table._types, name);
        if (declaration is null)
        {
            throw UnknownSymbolException.UndefinedType(name);
        }

        return (AbstractTypeDeclaration)declaration;
    }

    public void DeclareVariable(AbstractVariableDeclaration symbol)
    {
        if (!_variablesAndFunctions.TryAdd(symbol.Name, symbol))
        {
            throw DuplicateSymbolException.DuplicateVariableOrFunction(symbol.Name);
        }
    }

    public void DeclareFunction(AbstractFunctionDeclaration symbol)
    {
        if (!_variablesAndFunctions.TryAdd(symbol.Name, symbol))
        {
            throw DuplicateSymbolException.DuplicateVariableOrFunction(symbol.Name);
        }
    }

    public void DeclareType(AbstractTypeDeclaration symbol)
    {
        if (!_types.TryAdd(symbol.Name, symbol))
        {
            throw DuplicateSymbolException.DuplicateType(symbol.Name);
        }
    }

    private Declaration? FindDeclaration(Func<SymbolsTable, Dictionary<string, Declaration>> getTable, string name)
    {
        if (getTable(this).TryGetValue(name, out Declaration? declaration))
        {
            return declaration;
        }

        return _parent?.FindDeclaration(getTable, name);
    }
}