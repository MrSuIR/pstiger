using PsTiger.Ast.Declarations;
using PsTiger.Execution;
using PsTiger.Runtime;

namespace PsTiger.VirtualMachine;

public class TigerVm
{
    private readonly IEnvironment _environment;
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
    /// Стек словарей для хранения переменных.
    /// </summary>
    private readonly Stack<Dictionary<string, Value>> _variableScopes;

    /// <summary>
    /// Словарь встроенных функций.
    /// </summary>
    private readonly IReadOnlyDictionary<string, BuiltinFunction> _builtinFunctionsMap;

    /// <summary>
    /// Стек с номерами инструкций, сохранённых перед вызовами незавершённых функций.
    /// </summary>
    private Stack<int> _functionReturnStack;

    public TigerVm(IEnvironment environment, IReadOnlyList<Instruction> instructions)
    {
        ValidateInstructions(instructions);

        _environment = environment;
        _instructions = instructions;
        _instructionPointer = 0;
        _exitCode = 0;
        _evaluationStack = new Stack<Value>();
        _variableScopes = new Stack<Dictionary<string, Value>>();
        _builtinFunctionsMap = new Builtins(environment).Functions.ToDictionary(x => x.Name);
        _functionReturnStack = [];

        _variableScopes.Push(new Dictionary<string, Value>());
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

                case InstructionCode.Pop:
                    _evaluationStack.Pop();
                    break;

                case InstructionCode.StoreVar:
                    {
                        Value value = _evaluationStack.Pop();
                        string variableName = instruction.Operand.AsString();
                        _variableScopes.Peek()[variableName] = value;
                    }

                    break;

                case InstructionCode.LoadVar:
                    {
                        string variableName = instruction.Operand.AsString();
                        Value value = _variableScopes.Peek()[variableName];
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
                        Value value = _evaluationStack.Pop();
                        int index = _evaluationStack.Pop().AsInt();
                        Value array = _evaluationStack.Pop();
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
                        Value value = _evaluationStack.Pop();
                        Value record = _evaluationStack.Pop();
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
                    CallBuiltin(instruction.Operand.AsString());
                    break;

                case InstructionCode.Call:
                    {
                        _functionReturnStack.Push(_instructionPointer);
                        _instructionPointer = instruction.Operand.AsInt();
                    }

                    break;

                case InstructionCode.Return:
                    {
                        _instructionPointer = _functionReturnStack.Pop();
                    }

                    break;

                case InstructionCode.Halt:
                    _exitCode = instruction.Operand.AsInt();
                    return _evaluationStack.TryPop(out Value? result) ? result : Value.Void;

                case InstructionCode.PushVars:
                    _variableScopes.Push(new Dictionary<string, Value>());
                    break;

                case InstructionCode.PopVars:
                    _variableScopes.Pop();
                    break;

                default:
                    throw new NotImplementedException($"Unsupported instruction code: {instruction.Code}");
            }
        }
    }

    /// <summary>
    /// Выполняет вызов встроенной функции.
    /// </summary>
    private void CallBuiltin(string name)
    {
        if (!_builtinFunctionsMap.TryGetValue(name, out BuiltinFunction? function))
        {
            throw new ArgumentException($"Unknown builtin function: {name}");
        }

        // Извлекаем из стека список аргументов встроенной функции.
        int parametersCount = function.Parameters.Count;
        List<Value> arguments = new(parametersCount);
        for (int i = 0; i < parametersCount; i++)
        {
            arguments.Add(_evaluationStack.Pop());
        }

        // Переворачиваем список аргументов, так как они были извлечены из стека в обратном порядке.
        arguments.Reverse();

        // Вызываем встроенную функцию.
        Value value = function.Invoke(arguments);

        // Добавляем результат в стек, если функция возвращает что-либо.
        if (!value.IsVoid())
        {
            _evaluationStack.Push(value);
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
}