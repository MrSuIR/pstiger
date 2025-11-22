using Interpreter.IntegrationTests.TestDoubles;

using PsTiger.Interpreter;

namespace Interpreter.IntegrationTests;

public class VariablesTest
{
    [Fact]
    public void Can_calculate_rectangle_square()
    {
        const string code =
            """
            var
              x1 := 0
              y1 := 4
              x2 := 6
              y2 := 7
              width: int := 0
              height: int := 0
              square: int := 0
            in
              width := x2 - x1
              height := y2 - y1
              square := width * height
              printi(square)
            end
            """;

        FakeEnvironment environment = new();
        TigerInterpreter interpreter = new(environment);
        interpreter.Execute(code);

        Assert.Equal("7", environment.BufferedOutput);
    }

    [Fact]
    public void Can_print_hello_world()
    {
        const string code =
            """
            var
              greeting: string := ""
              exclamation := "!"
              space := " "
            in
              greeting := concat("Hello", space)
              greeting := concat(greeting, "world")
              greeting := concat(greeting, exclamation)
              print(greeting)
            end
            """;

        FakeEnvironment environment = new();
        TigerInterpreter interpreter = new(environment);
        interpreter.Execute(code);

        Assert.Equal("Hello world!", environment.BufferedOutput);
    }
}