using PsTiger.Ast.Declarations;
using PsTiger.Ast.Expressions;
using PsTiger.Semantics.Exceptions;
using PsTiger.Semantics.Symbols;

namespace PsTiger.Semantics.Passes;

/// <summary>
/// Проход по AST, устанавливающий соответствие имён и символов (объявлений).
/// </summary>
public sealed class ResolveNamesPass : AbstractPass
{
    /// <summary>
    /// В таблицу символов складываются объявления.
    /// </summary>
    private SymbolsTable _symbols;

    public ResolveNamesPass(SymbolsTable globalSymbols)
    {
        _symbols = globalSymbols;
    }

    public override void Visit(FunctionCallExpression e)
    {
        base.Visit(e);

        e.Function = ResolveFunction(e.Name);
    }

    public override void Visit(ScopeExpression e)
    {
        // Создаём дочернюю таблицу символов.
        _symbols = new SymbolsTable(_symbols);
        try
        {
            base.Visit(e);
        }
        finally
        {
            _symbols = _symbols.Parent!;
        }
    }

    public override void Visit(VariableAccessExpression e)
    {
        base.Visit(e);

        e.Variable = ResolveVariable(e.Name);
    }

    public override void Visit(VariableDeclaration e)
    {
        base.Visit(e);

        e.DeclaredType = e.DeclaredTypeName != null ? ResolveType(e.DeclaredTypeName) : null;
        _symbols.DefineSymbol(e.Name, e);
    }

    private AbstractFunctionDeclaration ResolveFunction(string name)
    {
        Declaration symbol = _symbols.GetSymbol(name);
        if (symbol is AbstractFunctionDeclaration function)
        {
            return function;
        }

        throw new InvalidSymbolException(
            $"Name {name} does not refer to a function"
        );
    }

    private VariableDeclaration ResolveVariable(string name)
    {
        Declaration symbol = _symbols.GetSymbol(name);
        if (symbol is VariableDeclaration variable)
        {
            return variable;
        }

        throw new InvalidSymbolException(
            $"Name {name} does not refer to a variable"
        );
    }

    private AbstractTypeDeclaration ResolveType(string name)
    {
        Declaration symbol = _symbols.GetSymbol(name);
        if (symbol is AbstractTypeDeclaration type)
        {
            return type;
        }

        throw new InvalidSymbolException(
            $"Name {name} does not refer to a type"
        );
    }
}