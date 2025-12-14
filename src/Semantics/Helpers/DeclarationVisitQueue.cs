using PsTiger.Ast;
using PsTiger.Ast.Declarations;

namespace PsTiger.Semantics.Helpers;

/// <summary>
/// Выполняет отложенный обход узлов объявлений для реализации взаимной рекурсии объявлений.
/// </summary>
/// <remarks>
/// Для поддержки взаимной рекурсии функций мы выполняем обход дочерних узлов необычным способом:
/// 1. Для подряд идущих объявлений функций лбо типов мы объявляем все их заранее (до посещения дочерних узлов)
/// 2. Как только подряд идущие функции либо типы заканчиваются — запускаем обход дочерних узлов.
/// </remarks>
public class DeclarationVisitQueue
{
    private readonly IAstVisitor _visitor;
    private readonly Queue<Declaration> _visitQueue;
    private VisitQueueType _visitQueueType;

    public DeclarationVisitQueue(IAstVisitor visitor)
    {
        _visitor = visitor;
        _visitQueue = [];
        _visitQueueType = VisitQueueType.None;
    }

    private enum VisitQueueType
    {
        None,
        Type,
        Function,
    }

    public void BeforeTypeDeclaration()
    {
        UpdateVisitQueueType(VisitQueueType.Type);
    }

    public void BeforeFunctionDeclaration()
    {
        UpdateVisitQueueType(VisitQueueType.Function);
    }

    public void Flush()
    {
        UpdateVisitQueueType(VisitQueueType.None);
    }

    public void Enqueue(Declaration declaration)
    {
        if (_visitQueueType != VisitQueueType.None)
        {
            _visitQueue.Enqueue(declaration);
        }
        else
        {
            declaration.Accept(_visitor);
        }
    }

    private void UpdateVisitQueueType(VisitQueueType type)
    {
        if (_visitQueueType != type)
        {
            ProcessVisitQueue();
            _visitQueueType = type;
        }
    }

    private void ProcessVisitQueue()
    {
        while (_visitQueue.TryDequeue(out Declaration? declaration))
        {
            declaration.Accept(_visitor);
        }
    }
}