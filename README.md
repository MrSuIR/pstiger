# Интерпретатор языка Tiger

Tiger — учебный язык программирования, разработанный Andrew Appel для книг и курсов в Princeton University.

Цель этого проекта — написать полный интерпретатор Tiger на C# для демонстрации в качестве примера.

## Статус

Статус поддержки возможностей языка:

1. [x] Валидация программы на соответствие грамматике на ANTLR4
2. [x] Лексический анализ
3. [x] Разбор и вычисление выражений
4. [x] Переменные, let...int... и присваивания
5. [x] Ветвления
6. [x] Пользовательские функции и процедуры
7. [x] Циклы while и for
8. [x] Массивы и объявления типов
9. [x] Структуры и nil

Статус поддержи фаз компиляции:

1. [x] Лексический анализ
2. [x] Синтаксический анализ
3. [x] Семантический анализ
4. [x] Генерация кода для виртуальной машины
5. [x] Выполнение программы

## Ветки

| Ветка               | Что добавляет                        |
|---------------------|--------------------------------------|
| 01_grammar_checker  | Валидатор синтаксиса на ANTLR4       |
| 02_lexical_analysis | Лексический анализ Tiger             |
| 03_expressions      | Разбор выражений                     |
| 04_variables        | Переменные и присваивания            |
| 05_if_else          | Ветвления                            |
| 06_functions        | Пользовательские функции и процедуры |
| 07_loops            | Циклы for и while, выражение break   |
| 08_arrays           | Массивы и объявления типов           |
| 09_records          | Структуры и nil                      |
| 10_benchmarks       | Бенчмарк программ на Tiger           |
| 11_virtual_machine  | Виртуальная машина для языка Tiger   |

## Клонирование проекта

Клонирование без авторизации в SourceCraft:

```bash
# Клонируем репозиторий в каталог pstiger/
git clone https://git@git.sourcecraft.dev/sshambir-public/pstiger.git
```

Если у вас есть аккаунт SourceCraft и вы настроили SSH-ключи, то можно клонировать по SSH:

```bash
# Клонируем репозиторий в каталог pstiger/
git clone ssh://ssh.sourcecraft.dev/sshambir-public/pstiger.git
```

## Сборка

Сборка консольной утилитой dotnet из .NET SDK:

```bash
# Сборка.
dotnet build

# Запуск тестов.
dotnet test

# Запуск бенчмарка
dotnet run -c Release --project tests/Interpreter.Benchmarks
```

## Результаты бенчмарка

Бенчмарки используют библиотеку [BenchmarkDotNet](https://benchmarkdotnet.org).

Ветка `10_benchmarks`, коммит `1c6b82f22186161cb6c21f1c7129261987bd28cd`, интерпретатор вычисляет программу путём обхода AST:

```
BenchmarkDotNet v0.15.8, Linux Ubuntu 24.04.2 LTS (Noble Numbat)
AMD Ryzen 7 4800H with Radeon Graphics 1.40GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 8.0.409
  [Host]     : .NET 8.0.16 (8.0.16, 8.0.1625.21506), X64 RyuJIT x86-64-v3
  Job-UNGBHD : .NET 8.0.16 (8.0.16, 8.0.1625.21506), X64 RyuJIT x86-64-v3

InvocationCount=1  IterationCount=10  LaunchCount=1  
UnrollFactor=1  WarmupCount=2  

| Method         | N     | Mean      | Error      | StdDev     |
|--------------- |------ |----------:|-----------:|-----------:|
| ListPrimesUpTo | 1000  |  4.996 ms |  0.2591 ms |  0.1542 ms |
| ListPrimesUpTo | 10000 | 44.261 ms | 25.8115 ms | 17.0727 ms |
```

Ветка `11_virtual_machine`, коммит `f3cde14214dcbce53f0439595ce24f20708e1646`, интерпретатор использует виртуальную машину:

```
| Method         | N     | Mean      | Error     | StdDev    |
|--------------- |------ |----------:|----------:|----------:|
| ListPrimesUpTo | 1000  |  2.455 ms | 0.0674 ms | 0.0401 ms |
| ListPrimesUpTo | 10000 | 20.918 ms | 4.3785 ms | 2.6056 ms |
```

Заметно двухкратное ускорение от перехода на виртуальную машину, несмотря на полное отсутствие оптимизаций.

Аналогичный алгоритм, реализованный на C#, на той же машине работает примерно в 500 раз быстрее при N=10000:

```
| Method         | N     | Mean      | Error     | StdDev    |
|--------------- |------ |----------:|----------:|----------:|
| ListPrimesUpTo | 1000  |  5.911 us | 0.1403 us | 0.0835 us |
| ListPrimesUpTo | 10000 | 95.027 us | 5.8080 us | 3.8416 us |
```

## Архитектура

Проект написан на C# 12 и .NET 8.

Взаимосвязь модулей:

```mermaid
graph TD
    Ast["Ast"]
    Interpreter["Interpreter"]
    Lexemes["Lexemes"]
    Parsing["Parsing"]
    Runtime["Runtime"]
    Semantics["Semantics"]
    VirtualMachine["VirtualMachine"]
    VirtualMachineCodegen["VirtualMachineCodegen"]

    Ast --> Runtime
    Interpreter --> Parsing
    Interpreter --> Runtime
    Interpreter --> Semantics
    Interpreter --> VirtualMachineCodegen
    Parsing --> Ast
    Parsing --> Lexemes
    Semantics --> Ast
    Semantics --> Runtime
    VirtualMachine --> Runtime
    VirtualMachineCodegen --> Ast
    VirtualMachineCodegen --> VirtualMachine
```

## Покрытие тестами

В проекте есть:

1. Приёмочные тесты реалистичных программ: `Interpreter.Specs`
2. Приёмочные интеграционные тесты возможностей языка: `Interpreter.IntegrationTests`
3. Тесты модуля Grammar, содержащего валидатор синтаксиса на ANTLR4: `Grammar.UnitTests`
4. Тесты модуля Lexer, содержащего лексический анализатор: `Lexemes.UnitTests`
5. Тесты модуля VirtualMachine, содержащего виртуальную машину для программ на Tiger: `VirtualMachine.UnitTests`

## Лицензия

- Исходный код интерпретатора доступен под лицензией MIT.
- Тесты в каталоге `tests/Grammar.UnitTests/valid/` используются на правах GPL 3.0, поскольку скопированы из репозитория https://gitlab.com/gwasser/lambdatiger
