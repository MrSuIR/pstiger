using Grammar;

using PsTiger.Interpreter;
using PsTiger.Semantics.Exceptions;
using PsTiger.Tests.TestLibrary.TestDoubles;

namespace Interpreter.IntegrationTests;

public class ArrayTest
{
    [Theory]
    [MemberData(nameof(GetInvalidArrayUsageData))]
    public void Rejects_invalid_array_usage(string code, Type exceptionType)
    {
        TigerGrammar.CheckProgramSyntax(code);

        FakeEnvironment environment = new();
        TigerInterpreter interpreter = new(environment);

        Assert.Throws(exceptionType, () => interpreter.Execute(code));
    }

    public static TheoryData<string, Type> GetInvalidArrayUsageData()
    {
        return new TheoryData<string, Type>
        {
            // Для массивов не действует оператор сравнения `<`
            {
                """
                let
                    type intArray = array of int
                    var numsCount := 2
                    var nums1: intArray := intArray[numsCount] of 3
                    var nums2: intArray := intArray[numsCount] of 3
                in
                    printi(nums1 < nums2)
                end
                """,
                typeof(TypeErrorException)
            },

            // Для массивов не действует оператор сложения `+`
            {
                """
                let
                    type intArray = array of int
                    var numsCount := 2
                    var nums1: intArray := intArray[numsCount] of 3
                    var nums2: intArray := intArray[numsCount] of 3
                    var allNums: intArray := intArray[numsCount * 2] of 0
                in
                    allNums := nums1 + nums2
                end
                """,
                typeof(TypeErrorException)
            },

            // Нельзя сравнивать массивы разных типов, если эти типы не являются синонимами
            {
                """
                let
                    type intArray = array of int
                    type numArray = array of int
                    var numsCount := 2
                    var nums1: intArray := intArray[numsCount] of 3
                    var nums2: numArray := numArray[numsCount] of 3
                in
                    printi(nums1 = nums2)
                end
                """,
                typeof(TypeErrorException)
            },

            // Индекс массива должен быть целым числом
            {
                """
                let
                    type intArray = array of int
                    var numsCount := 3
                    var nums1: intArray := intArray[numsCount] of 0
                in
                    nums1["2"] := 3;
                    printi(nums1[2])
                end
                """,
                typeof(TypeErrorException)
            },

            // Нельзя запрашивать элемент массива у переменной, не являющейся массивом
            {
                """
                let
                    var text: string := "Hello"
                in
                    print(text[0])
                end
                """,
                typeof(TypeErrorException)
            },

            // Индекс массива не должен выходить за границы массива при выполнении программы
            {
                """
                let
                    type intArray = array of int
                    var numsCount := 3
                    var nums1: intArray := intArray[numsCount] of 0
                in
                    nums1[3] := 3;
                    printi(nums1[2])
                end
                """,
                typeof(IndexOutOfRangeException)
            },

            // Нельзя для инициализации массива использовать значение типа, отличного от типа элементов
            {
                """
                let
                    type intArray = array of int
                    var numsCount := 3
                    var nums1: intArray := intArray[numsCount] of "0"
                in
                    printi(nums1[2])
                end
                """,
                typeof(TypeErrorException)
            },

            // Нельзя использовать для литерала массива тип, не являющийся массивом
            {
                """
                let
                    type intArray = array of int
                    var numsCount := 3
                    var nums1: intArray := int[numsCount] of 0
                in
                    printi(nums1[2])
                end
                """,
                typeof(TypeErrorException)
            },
        };
    }
}