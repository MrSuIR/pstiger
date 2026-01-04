using System.Text;

using PsTiger.Runtime;

namespace PsTiger.VirtualMachine;

public class Instruction
{
    public Instruction(InstructionCode code)
    {
        Code = code;
        Operand = Value.Void;
    }

    public Instruction(InstructionCode code, int value)
    {
        Code = code;
        Operand = new Value(value);
    }

    public Instruction(InstructionCode code, string value)
    {
        Code = code;
        Operand = new Value(value);
    }

    public InstructionCode Code { get; set; }

    public Value Operand { get; set; }

    /// <summary>
    /// Печатает инструкцию в формате "Code Operand" либо просто "Code".
    /// </summary>
    public override string ToString()
    {
        StringBuilder sb = new();
        sb.Append(Code);
        if (!Operand.IsVoid())
        {
            sb.Append(' ');
            sb.Append(Operand);
        }

        return sb.ToString();
    }
}