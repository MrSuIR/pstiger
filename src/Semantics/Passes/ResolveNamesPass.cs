using PsTiger.Ast.Declarations;
using PsTiger.Ast.Expressions;
using PsTiger.Semantics.Exceptions;
using PsTiger.Semantics.Helpers;
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
        // Выполняем отложенный обход узлов объявлений для реализации взаимной рекурсии объявлений.
        DeclarationVisitQueue visitQueue = new(this);

        // Создаём дочернюю таблицу символов.
        _symbols = new SymbolsTable(_symbols);
        try
        {
            // Обходим объявления, при этом идущие подряд функции объявляем заранее.
            foreach (Declaration d in e.Declarations)
            {
                switch (d)
                {
                    case FunctionDeclaration f:
                        // Заранее объявляем эту функцию и добавляем в очередь обхода.
                        visitQueue.BeforeFunctionDeclaration();
                        _symbols.DeclareFunction(f);
                        visitQueue.Enqueue(d);
                        break;
                    case TypeDeclaration t:
                        // Заранее объявляем этот тип и добавляем в очередь обхода.
                        visitQueue.BeforeTypeDeclaration();
                        _symbols.DeclareType(t);
                        visitQueue.Enqueue(d);
                        break;
                    default:
                        visitQueue.Flush();
                        d.Accept(this);
                        break;
                }
            }

            visitQueue.Flush();

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

    public override void Visit(RecordTypeExpression e)
    {
        base.Visit(e);

        Dictionary<string, AbstractTypeDeclaration> fields = [];
        foreach (FieldDeclaration declaration in e.FieldDeclarations)
        {
            if (!fields.TryAdd(declaration.Name, declaration.Type))
            {
                throw DuplicateSymbolException.DuplicateField(declaration.Name);
            }
        }

        e.Fields = fields;
    }

    public override void Visit(FieldDeclaration d)
    {
        base.Visit(d);
        d.Type = _symbols.GetTypeDeclaration(d.TypeName);
    }

    public override void Visit(ArrayLiteralExpression e)
    {
        base.Visit(e);

        e.ArrayType = _symbols.GetTypeDeclaration(e.ArrayTypeName);
    }

    public override void Visit(RecordLiteralExpression e)
    {
        base.Visit(e);

        e.RecordType = _symbols.GetTypeDeclaration(e.RecordTypeName);
    }
}