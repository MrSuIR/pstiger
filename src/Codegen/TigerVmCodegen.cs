using PsTiger.Ast;
using PsTiger.Ast.Declarations;
using PsTiger.Ast.Expressions;
using PsTiger.Execution;
using PsTiger.VirtualMachine;

using ValueType = PsTiger.Runtime.ValueType;

namespace Codegen;

/// <summary>
/// Генерирует инструкции виртуальной машины TigerVm путём обхода абстрактного синтаксического дерева (AST) программы.
/// </summary>
public class TigerVmCodegen : IAstVisitor
{
    private List<Instruction> _instructions = [];
    private int _scopeDepth;

    public List<Instruction> GenerateCode(Expression program)
    {
        _instructions = [];
        program.Accept(this);

        if (program.ResultType != ValueType.Void)
        {
            _instructions.Add(new Instruction(InstructionCode.StoreResult));
        }

        _instructions.Add(new Instruction(InstructionCode.Push, 0));
        _instructions.Add(new Instruction(InstructionCode.Halt));

        return _instructions;
    }

    public void Visit(LiteralExpression e)
    {
        _instructions.Add(new Instruction(InstructionCode.Push, e.Value));
    }

    public void Visit(BinaryOperationExpression e)
    {
        InstructionCode code = e.Operation switch
        {
            BinaryOperation.Add => InstructionCode.Add,
            BinaryOperation.Subtract => InstructionCode.Subtract,
            BinaryOperation.Multiply => InstructionCode.Multiply,
            BinaryOperation.Divide => InstructionCode.Divide,
            BinaryOperation.Equal => InstructionCode.Equal,
            BinaryOperation.NotEqual => InstructionCode.NotEqual,
            BinaryOperation.LessThan => InstructionCode.Less,
            BinaryOperation.LessThanOrEqual => InstructionCode.LessOrEqual,

            _ => throw new NotImplementedException(
                $"Code generation for binary operation {e.Operation} not implemented"
            ),
        };

        e.Left.Accept(this);
        e.Right.Accept(this);
        _instructions.Add(new Instruction(code));
    }

    public void Visit(SequenceExpression e)
    {
        GenerateExpressionsSequenceCode(e.Sequence);
    }

    public void Visit(UnaryMinusExpression e)
    {
        e.Operand.Accept(this);
        _instructions.Add(new Instruction(InstructionCode.Negate));
    }

    public void Visit(FunctionCallExpression e)
    {
        foreach (Expression argument in e.Arguments)
        {
            argument.Accept(this);
        }

        switch (e.Function)
        {
            case BuiltinFunction builtin:
                Instruction instruction = builtin.Name switch
                {
                    Builtins.Not => new Instruction(InstructionCode.Not),
                    Builtins.Exit => new Instruction(InstructionCode.Halt),
                    _ => new Instruction(InstructionCode.CallBuiltin, builtin.Name),
                };
                _instructions.Add(instruction);
                break;

            case FunctionDeclaration function:
                throw new NotImplementedException();

            default:
                throw new NotImplementedException();
        }
    }

    public void Visit(ScopeExpression e)
    {
        ++_scopeDepth;
        _instructions.Add(new Instruction(InstructionCode.PushVars, _scopeDepth));

        foreach (Declaration declaration in e.Declarations)
        {
            declaration.Accept(this);
        }

        GenerateExpressionsSequenceCode(e.Expressions);

        _instructions.Add(new Instruction(InstructionCode.PopVars));
        --_scopeDepth;
    }

    public void Visit(VariableAccessExpression e)
    {
        _instructions.Add(new Instruction(InstructionCode.LoadVar, e.Variable.Name));
    }

    public void Visit(AssignmentExpression e)
    {
        if (e.Left is VariableAccessExpression variableAccess)
        {
            e.Right.Accept(this);
            _instructions.Add(new Instruction(InstructionCode.StoreVar, variableAccess.Variable.Name));
        }
        else
        {
            throw new NotImplementedException();
        }
    }

    public void Visit(IfElseExpression e)
    {
        throw new NotImplementedException();
    }

    public void Visit(VariableDeclaration d)
    {
        d.InitialValue.Accept(this);
        _instructions.Add(new Instruction(InstructionCode.DefineVar, d.Name));
    }

    public void Visit(FunctionDeclaration d)
    {
        throw new NotImplementedException();
    }

    public void Visit(ParameterDeclaration d)
    {
        throw new NotImplementedException();
    }

    public void Visit(WhileLoopExpression e)
    {
        throw new NotImplementedException();
    }

    public void Visit(ForLoopExpression e)
    {
        throw new NotImplementedException();
    }

    public void Visit(ForIteratorDeclaration d)
    {
        throw new NotImplementedException();
    }

    public void Visit(BreakLoopExpression e)
    {
        throw new NotImplementedException();
    }

    public void Visit(TypeDeclaration d)
    {
        throw new NotImplementedException();
    }

    public void Visit(NamedTypeExpression e)
    {
        throw new NotImplementedException();
    }

    public void Visit(ArrayTypeExpression e)
    {
        throw new NotImplementedException();
    }

    public void Visit(ArrayAccessExpression e)
    {
        throw new NotImplementedException();
    }

    public void Visit(ArrayLiteralExpression e)
    {
        throw new NotImplementedException();
    }

    public void Visit(RecordTypeExpression e)
    {
        throw new NotImplementedException();
    }

    public void Visit(FieldDeclaration d)
    {
        throw new NotImplementedException();
    }

    public void Visit(RecordLiteralExpression e)
    {
        throw new NotImplementedException();
    }

    public void Visit(FieldInitializer e)
    {
        throw new NotImplementedException();
    }

    public void Visit(FieldAccessExpression e)
    {
        throw new NotImplementedException();
    }

    private void GenerateExpressionsSequenceCode(IReadOnlyList<Expression> sequence)
    {
        for (int i = 0, iMax = sequence.Count - 1; i <= iMax; ++i)
        {
            Expression expression = sequence[i];
            expression.Accept(this);

            // Отбрасываем результат всех выражений, кроме последнего.
            if (i != iMax && expression.ResultType != ValueType.Void)
            {
                _instructions.Add(new Instruction(InstructionCode.Pop));
            }
        }
    }
}