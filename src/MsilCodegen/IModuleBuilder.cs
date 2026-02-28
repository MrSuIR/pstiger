using System.Reflection.Emit;

namespace PsTiger.MsilCodegen;

public interface IModuleBuilder
{
    public TypeBuilder DefineClass(string name);
}