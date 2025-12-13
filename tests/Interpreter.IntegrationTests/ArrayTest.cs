using Grammar;

using Interpreter.IntegrationTests.TestDoubles;

using PsTiger.Interpreter;

namespace Interpreter.IntegrationTests;

public class ArrayTest
{
    [Theory]
    [MemberData(nameof(GetUseArrayTypesData))]
    public void Can_use_array_types(string code, string expectedOutput)
    {
        TigerGrammar.CheckProgramSyntax(code);

        FakeEnvironment environment = new();
        TigerInterpreter interpreter = new(environment);

        interpreter.Execute(code);
        Assert.Equal(expectedOutput, environment.BufferedOutput);
    }

    public static TheoryData<string, string> GetUseArrayTypesData()
    {
        return new TheoryData<string, string>
        {
            // Можно создать одномерный массив чисел, присвоить и прочитать его элементы
            {
                """
                let
                    type intArray = array of int
                    var numsCount := 3
                    var nums: intArray := intArray[numsCount] of 7
                in
                    nums[0] := 3;
                    nums[1] := 4;
                    for i := 0 to numsCount - 1 do
                        printi(nums[i])
                end
                """,
                "347"
            },

            // Можно создать двумерный массив чисел, присвоить и прочитать его элементы
            {
                """
                let
                    type row = array of int
                    type table = array of row
                    var rowCount := 3
                    var columnCount := 4
                    var values: table := table[rowCount] of row[columnCount] of 0
                in
                    values[0][1] := 3;
                    values[2][2] := 1;
                    values[2][3] := 5;
                    for rowIndex := 0 to rowCount - 1 do (
                        for columnIndex := 0 to columnCount - 1 do
                            printi(values[rowIndex][columnIndex]);
                        print("\n")
                    )
                end
                """,
                "0300\n0000\n0015\n"
            },

            // Присваивание переменных с типом массива создаёт ссылку на массив, а не его копию
            {
                """
                let
                    type intArray = array of int
                    var numsCount := 3
                    var nums: intArray := intArray[numsCount] of 7
                    var numsCopy := nums
                in
                    numsCopy[0] := 3;
                    numsCopy[1] := 4;
                    for i := 0 to numsCount - 1 do
                        printi(nums[i])
                end
                """,
                "347"
            },

            // Массив передаётся в функцию по ссылке, а не по значению
            {
                """
                let
                    type intArray = array of int
                    var numsCount := 4
                    var nums: intArray := intArray[numsCount] of 2
                    function setNumber(nums: intArray, index: int, value: int) = nums[index] := value
                in
                    setNumber(nums, 0, 8);
                    setNumber(nums, 1, 5);
                    for i := 0 to numsCount - 1 do
                        printi(nums[i])
                end
                """,
                "8522"
            },
        };
    }
}