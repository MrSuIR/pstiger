using PsTiger.Ast.Declarations;
using PsTiger.Ast.Expressions;
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

        e.Function = _symbols.GetFunctionDeclaration(e.Name);
    }

    public override void Visit(ScopeExpression e)
    {
        // NOTE: Для поддержки взаимной рекурсии функций мы выполняем обход дочерних узлов необычным способом:
        // 1. Для подряд идущих объявлений функций мы объявляем их заранее (до посещения дочерних узлов)
        // 2. Как только подряд идущие функции заканчиваются — запускаем обход узлов этих функций.
        Queue<Declaration> visitQueue = [];

        // Создаём дочернюю таблицу символов.
        _symbols = new SymbolsTable(_symbols);
        try
        {
            // Обходим объявления, при этом идущие подряд функции объявляем заранее.
            foreach (Declaration d in e.Declarations)
            {
                if (d is FunctionDeclaration f)
                {
                    // Заранее объявляем эту функцию.
                    _symbols.DeclareFunction(f);
                    visitQueue.Enqueue(d);
                }
                else
                {
                    ProcessVisitQueue();
                    d.Accept(this);
                }
            }

            ProcessVisitQueue();

            // Обходим последовательность выражений в данной области видимости.
            foreach (Expression nested in e.Expressions)
            {
                nested.Accept(this);
            }
        }
        finally
        {
            // Возвращаемся к прежней таблице символов.
            _symbols = _symbols.Parent!;
        }

        return;

        void ProcessVisitQueue()
        {
            while (visitQueue.TryDequeue(out Declaration? declaration))
            {
                declaration.Accept(this);
            }
        }
    }

    public override void Visit(VariableAccessExpression e)
    {
        base.Visit(e);

        e.Variable = _symbols.GetVariableDeclaration(e.Name);
    }

    public override void Visit(VariableDeclaration d)
    {
        base.Visit(d);

        d.DeclaredType = d.DeclaredTypeName != null ? _symbols.GetTypeDeclaration(d.DeclaredTypeName) : null;
        _symbols.DeclareVariable(d);
    }

    public override void Visit(FunctionDeclaration d)
    {
        // Находим заявленный тип результата.
        d.DeclaredType = d.DeclaredTypeName != null ? _symbols.GetTypeDeclaration(d.DeclaredTypeName) : null;

        // Создаём дочернюю таблицу символов.
        _symbols = new SymbolsTable(_symbols);
        try
        {
            // Обходим поддерево функции.
            base.Visit(d);
        }
        finally
        {
            _symbols = _symbols.Parent!;
        }
    }

    public override void Visit(ParameterDeclaration d)
    {
        base.Visit(d);

        d.Type = _symbols.GetTypeDeclaration(d.TypeName);
        _symbols.DeclareVariable(d);
    }

    public override void Visit(ForLoopExpression e)
    {
        // Создаём дочернюю таблицу символов.
        _symbols = new SymbolsTable(_symbols);
        try
        {
            base.Visit(e);
        }
        finally
        {
            // Возвращаемся к прежней таблице символов.
            _symbols = _symbols.Parent!;
        }
    }

    public override void Visit(ForIteratorDeclaration d)
    {
        base.Visit(d);
        _symbols.DeclareVariable(d);
    }

    public override void Visit(NamedTypeExpression e)
    {
        base.Visit(e);
        e.Type = _symbols.GetTypeDeclaration(e.TypeName);
    }

    public override void Visit(ArrayTypeExpression e)
    {
        base.Visit(e);
        e.ElementType = _symbols.GetTypeDeclaration(e.ElementTypeName);
    }

    public override void Visit(TypeDeclaration d)
    {
        base.Visit(d);
        _symbols.DeclareType(d);
    }

    public override void Visit(ArrayLiteralExpression e)
    {
        base.Visit(e);

        e.ArrayType = _symbols.GetTypeDeclaration(e.ArrayTypeName);
    }
}