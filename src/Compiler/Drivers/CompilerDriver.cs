using System.Reflection.Emit;

using PsTiger.Ast.Expressions;
using PsTiger.MsilBackend;
using PsTiger.MsilCodegen;
using PsTiger.Parsing;
using PsTiger.Semantics;

namespace PsTiger.Compiler.Drivers;

public class CompilerDriver
{
    public void Compile(string inputPath, string outputPath)
    {
        string code = File.ReadAllText(inputPath);

        // Фронтенд компилятора:
        //  1. Лексический анализ
        //  2. Синтаксический анализ
        //  3. Семантический анализ
        Parser parser = new(code);
        Expression program = parser.ParseProgram();
        SemanticsChecker checker = new();
        checker.Check(program);

        // Бэкенд компилятора:
        //  1. Генерация MSIL-кода.
        //  2. Сохранение исполняемого файла.
        ExecutableBuilder executableBuilder = new(outputPath);
        MsilCodegenPass codegenPass = new(executableBuilder.ModuleBuilder);
        MethodBuilder mainMethod = codegenPass.GenerateProgramCode(program);
        executableBuilder.Save(mainMethod);
    }
}