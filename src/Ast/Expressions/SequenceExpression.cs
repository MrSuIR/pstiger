namespace PsTiger.Ast.Expressions;

public sealed class SequenceExpression : Expression
{
    private readonly List<Expression> _sequence;

    public SequenceExpression(List<Expression> sequence)
    {
        _sequence = sequence;
    }

    public IReadOnlyList<Expression> Sequence => _sequence;

    public override void Accept(IAstVisitor visitor)
    {
        visitor.Visit(this);
    }
}