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
        Declaration symbol = _symbols.GetSymbol(e.Name);
        if (symbol is BuiltinFunction function)
        {
            e.Function = function;
        }
        else
        {
            throw new InvalidFunctionCallException(
                $"Name {e.Name} does not refer to a function"
            );
        }

        base.Visit(e);
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

        Declaration symbol = _symbols.GetSymbol(e.Name);
        if (symbol is VariableDeclaration variable)
        {
            e.Variable = variable;
        }
        else
        {
            throw new InvalidVariableAccess(
                $"Name {e.Name} does not refer to a variable"
            );
        }
    }

    public override void Visit(VariableDeclaration e)
    {
        base.Visit(e);
        _symbols.DefineSymbol(e.Name, e);
    }
}