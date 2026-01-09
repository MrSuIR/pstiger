using PsTiger.Runtime;
using PsTiger.VirtualMachine.Builtins;
using PsTiger.VirtualMachine.Instructions;

namespace PsTiger.VirtualMachine;

public class TigerVm
{
    private readonly BuiltinFunctions _builtinFunctions;
    private readonly IReadOnlyList<Instruction> _instructions;

    /// <summary>
    /// Указатель на текущую инструкцию.
    /// </summary>
    private int _instructionPointer;

    /// <summary>
    /// Код завершения программы.
    /// </summary>
    private int _exitCode;

    /// <summary>
    /// Стек для вычисления выражений и передачи аргументов функций.
    /// </summary>
    private readonly Stack<Value> _evaluationStack;

    /// <summary>
    /// Текущая таблица переменных.
    /// </summary>
    private VariablesTable? _variables;

    /// <summary>
    /// Стек с номерами инструкций, сохранённых перед вызовами незавершённых функций.
    /// </summary>
    private readonly Stack<ReturnContext> _returnStack;

    /// <summary>
    /// Результат работы программы (произвольное значение либо отсутствие значения).
    /// </summary>
    private Value _result;

    public TigerVm(IEnvironment environment, IReadOnlyList<Instruction> instructions)
    {
        ValidateInstructions(instructions);

        _builtinFunctions = new BuiltinFunctions(environment);
        _instructions = instructions;
        _instructionPointer = 0;
        _exitCode = 0;
        _evaluationStack = new Stack<Value>();
        _variables = new VariablesTable();
        _returnStack = [];
        _result = Value.Void;
    }

    public int ExitCode => _exitCode;

    public Value RunProgram()
    {
        while (true)
        {
            Instruction instruction = _instructions[_instructionPointer++];
            switch (instruction.Code)
            {
                case InstructionCode.Push:
                    _evaluationStack.Push(instruction.Operand);
                    break;

                case InstructionCode.Duplicate:
                    _evaluationStack.Push(_evaluationStack.Peek());
                    break;

                case InstructionCode.Pop:
                    _evaluationStack.Pop();
                    break;

                case InstructionCode.StoreVar:
                    {
                        Value value = _evaluationStack.Pop();
                        string variableName = instruction.Operand.AsString();
                        _variables!.AssignVariable(variableName, value);
                    }

                    break;

                case InstructionCode.DefineVar:
                    {
                        Value value = _evaluationStack.Pop();
                        string variableName = instruction.Operand.AsString();
                        _variables!.DefineVariable(variableName, value);
                    }

                    break;

                case InstructionCode.LoadVar:
                    {
                        string variableName = instruction.Operand.AsString();
                        Value value = _variables!.GetVariable(variableName);
                        _evaluationStack.Push(value);
                    }

                    break;

                case InstructionCode.CreateArray:
                    {
                        Value initialValue = _evaluationStack.Pop();
                        int size = _evaluationStack.Pop().AsInt();
                        _evaluationStack.Push(Value.NewArray(size, initialValue));
                    }

                    break;

                case InstructionCode.LoadArray:
                    {
                        int index = _evaluationStack.Pop().AsInt();
                        Value array = _evaluationStack.Pop();
                        _evaluationStack.Push(array.GetElement(index));
                    }

                    break;

                case InstructionCode.StoreArray:
                    {
                        int index = _evaluationStack.Pop().AsInt();
                        Value array = _evaluationStack.Pop();
                        Value value = _evaluationStack.Pop();
                        array.SetElement(index, value);
                    }

                    break;

                case InstructionCode.InitField:
                    {
                        string fieldName = instruction.Operand.AsString();
                        Value value = _evaluationStack.Pop();
                        Value record = _evaluationStack.Peek();
                        record.SetField(fieldName, value);
                    }

                    break;

                case InstructionCode.StoreField:
                    {
                        string fieldName = instruction.Operand.AsString();
                        Value record = _evaluationStack.Pop();
                        Value value = _evaluationStack.Pop();
                        record.SetField(fieldName, value);
                    }

                    break;

                case InstructionCode.LoadField:
                    {
                        string fieldName = instruction.Operand.AsString();
                        Value record = _evaluationStack.Pop();
                        _evaluationStack.Push(record.GetField(fieldName));
                    }

                    break;

                case InstructionCode.Add:
                    {
                        Value right = _evaluationStack.Pop();
                        Value left = _evaluationStack.Pop();
                        _evaluationStack.Push(new Value(left.AsInt() + right.AsInt()));
                    }

                    break;

                case InstructionCode.Subtract:
                    {
                        Value right = _evaluationStack.Pop();
                        Value left = _evaluationStack.Pop();
                        _evaluationStack.Push(new Value(left.AsInt() - right.AsInt()));
                    }

                    break;

                case InstructionCode.Multiply:
                    {
                        Value right = _evaluationStack.Pop();
                        Value left = _evaluationStack.Pop();
                        _evaluationStack.Push(new Value(left.AsInt() * right.AsInt()));
                    }

                    break;

                case InstructionCode.Divide:
                    {
                        Value right = _evaluationStack.Pop();
                        Value left = _evaluationStack.Pop();
                        _evaluationStack.Push(new Value(left.AsInt() / right.AsInt()));
                    }

                    break;

                case InstructionCode.And:
                    {
                        Value right = _evaluationStack.Pop();
                        Value left = _evaluationStack.Pop();
                        _evaluationStack.Push(new Value((left.AsInt() != 0 && right.AsInt() != 0) ? 1 : 0));
                    }

                    break;

                case InstructionCode.Or:
                    {
                        Value right = _evaluationStack.Pop();
                        Value left = _evaluationStack.Pop();
                        _evaluationStack.Push(new Value((left.AsInt() != 0 || right.AsInt() != 0) ? 1 : 0));
                    }

                    break;

                case InstructionCode.Not:
                    {
                        Value operand = _evaluationStack.Pop();
                        _evaluationStack.Push(new Value(operand.AsInt() == 0 ? 1 : 0));
                    }

                    break;

                case InstructionCode.Equal:
                    {
                        Value right = _evaluationStack.Pop();
                        Value left = _evaluationStack.Pop();
                        _evaluationStack.Push(new Value(left.Equals(right) ? 1 : 0));
                    }

                    break;

                case InstructionCode.NotEqual:
                    {
                        Value right = _evaluationStack.Pop();
                        Value left = _evaluationStack.Pop();
                        _evaluationStack.Push(new Value(left.Equals(right) ? 0 : 1));
                    }

                    break;

                case InstructionCode.Less:
                    {
                        Value right = _evaluationStack.Pop();
                        Value left = _evaluationStack.Pop();
                        _evaluationStack.Push(new Value(left.LessThan(right) ? 1 : 0));
                    }

                    break;

                case InstructionCode.LessOrEqual:
                    {
                        Value right = _evaluationStack.Pop();
                        Value left = _evaluationStack.Pop();
                        _evaluationStack.Push(new Value(left.LessThanOrEqual(right) ? 1 : 0));
                    }

                    break;

                case InstructionCode.Negate:
                    {
                        Value operand = _evaluationStack.Pop();
                        _evaluationStack.Push(new Value(-operand.AsInt()));
                    }

                    break;

                case InstructionCode.Jump:
                    {
                        _instructionPointer = instruction.Operand.AsInt();
                    }

                    break;

                case InstructionCode.JumpIfTrue:
                    {
                        Value condition = _evaluationStack.Pop();
                        if (condition.AsInt() != 0)
                        {
                            _instructionPointer = instruction.Operand.AsInt();
                        }
                    }

                    break;

                case InstructionCode.JumpIfFalse:
                    {
                        Value condition = _evaluationStack.Pop();
                        if (condition.AsInt() == 0)
                        {
                            _instructionPointer = instruction.Operand.AsInt();
                        }
                    }

                    break;

                case InstructionCode.CallBuiltin:
                    CallBuiltin((BuiltinFunctionCode)instruction.Operand.AsInt());
                    break;

                case InstructionCode.Call:
                    {
                        _returnStack.Push(new ReturnContext(
                            _instructionPointer,
                            _variables
                        ));
                        _instructionPointer = instruction.Operand.AsInt();
                    }

                    break;

                case InstructionCode.Return:
                    {
                        ReturnContext context = _returnStack.Pop();
                        _instructionPointer = context.InstructionPointer;
                        _variables = context.Variables;
                    }

                    break;

                case InstructionCode.StoreResult:
                    // Сохраняем результат работы всей программы.
                    _result = _evaluationStack.Pop();
                    break;

                case InstructionCode.Halt:
                    // Получаем код возврата программы.
                    _exitCode = _evaluationStack.Pop().AsInt();
                    return _result;

                case InstructionCode.PushVars:
                    {
                        // Выполняем поиск родительской таблицы переменных.
                        int variableTableDepth = instruction.Operand.AsInt();
                        VariablesTable? parentTable = (variableTableDepth != 0)
                            ? _variables!.GetAncestor(variableTableDepth)
                            : null;
                        _variables = new VariablesTable(parentTable);
                    }

                    break;

                case InstructionCode.PopVars:
                    _variables = _variables!.Parent;
                    break;

                default:
                    throw new NotImplementedException($"Unsupported instruction code: {instruction.Code}");
            }
        }
    }

    /// <summary>
    /// Выполняет вызов встроенной функции.
    /// </summary>
    private void CallBuiltin(BuiltinFunctionCode code)
    {
        switch (code)
        {
            case BuiltinFunctionCode.Print:
                _builtinFunctions.Print(_evaluationStack.Pop());
                break;
            case BuiltinFunctionCode.PrintI:
                _builtinFunctions.PrintI(_evaluationStack.Pop());
                break;
            case BuiltinFunctionCode.Flush:
                _builtinFunctions.Flush();
                break;
            case BuiltinFunctionCode.GetChar:
                _evaluationStack.Push(_builtinFunctions.GetChar());
                break;
            case BuiltinFunctionCode.Ord:
                _evaluationStack.Push(_builtinFunctions.Ord(_evaluationStack.Pop()));
                break;
            case BuiltinFunctionCode.Chr:
                _evaluationStack.Push(_builtinFunctions.Chr(_evaluationStack.Pop()));
                break;
            case BuiltinFunctionCode.Size:
                _evaluationStack.Push(_builtinFunctions.Size(_evaluationStack.Pop()));
                break;
            case BuiltinFunctionCode.Substring:
                {
                    Value length = _evaluationStack.Pop();
                    Value fromIndex = _evaluationStack.Pop();
                    Value value = _evaluationStack.Pop();
                    _evaluationStack.Push(_builtinFunctions.Substring(value, fromIndex, length));
                }

                break;
            case BuiltinFunctionCode.Concat:
                {
                    Value s2 = _evaluationStack.Pop();
                    Value s1 = _evaluationStack.Pop();
                    _evaluationStack.Push(_builtinFunctions.Concat(s1, s2));
                }

                break;
            default:
                throw new ArgumentException($"Unknown builtin function: {code}");
        }
    }

    private static void ValidateInstructions(IReadOnlyList<Instruction> instructions)
    {
        if (instructions.Count == 0)
        {
            throw new InvalidOperationException("Invalid empty VM program");
        }

        InstructionCode lastInstructionCode = instructions[^1].Code;
        if (lastInstructionCode != InstructionCode.Halt
            && lastInstructionCode != InstructionCode.Return
            && lastInstructionCode != InstructionCode.Jump)
        {
            throw new InvalidOperationException(
                $"Last instruction must be {InstructionCode.Halt}," +
                $" {InstructionCode.Return} or {InstructionCode.Jump}, got {lastInstructionCode}"
            );
        }
    }

    private record struct ReturnContext(
        int InstructionPointer,
        VariablesTable? Variables
    );
}