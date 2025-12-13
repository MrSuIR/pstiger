using System.Diagnostics;

using PsTiger.Ast.Declarations;
using PsTiger.Semantics.Exceptions;

namespace PsTiger.Semantics.Symbols;

/// <summary>
/// Таблица символов, основанная на лексических областях видимости (областях действия) символов в коде.
/// </summary>
public sealed class SymbolsTable
{
    /// <summary>
    /// В языке есть два пространства имён:
    ///  1. Имена типов данных;
    ///  2. Имена переменных и функций.
    /// Категория — это человекочитаемое название пространства имён.
    /// </summary>
    private const string TypeCategory = "type";

    private const string VariableOrFunctionCategory = "variable or function";

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
            null => throw new UnknownSymbolException(VariableOrFunctionCategory, name),
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
            null => throw new UnknownSymbolException(VariableOrFunctionCategory, name),
            _ => throw new UnreachableException(),
        };
    }

    public AbstractTypeDeclaration GetTypeDeclaration(string name)
    {
        Declaration? declaration = FindDeclaration(table => table._types, name);
        if (declaration is null)
        {
            throw new UnknownSymbolException(TypeCategory, name);
        }

        return (AbstractTypeDeclaration)declaration;
    }

    public void DeclareVariable(AbstractVariableDeclaration symbol)
    {
        if (!_variablesAndFunctions.TryAdd(symbol.Name, symbol))
        {
            throw new DuplicateSymbolException(VariableOrFunctionCategory, symbol.Name);
        }
    }

    public void DeclareFunction(AbstractFunctionDeclaration symbol)
    {
        if (!_variablesAndFunctions.TryAdd(symbol.Name, symbol))
        {
            throw new DuplicateSymbolException(VariableOrFunctionCategory, symbol.Name);
        }
    }

    public void DeclareType(AbstractTypeDeclaration symbol)
    {
        if (!_types.TryAdd(symbol.Name, symbol))
        {
            throw new DuplicateSymbolException(TypeCategory, symbol.Name);
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