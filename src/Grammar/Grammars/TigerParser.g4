parser grammar TigerParser;

options {
    tokenVocab = TigerLexer;
}

// В языке Tiger вся программа — это одно выражение
program
    : expr EOF
    ;

// Выражение
expr
    : assignmentExpr
    ;

// Присваивание
assignmentExpr
    : lvalue ASSIGN expr
    | logicalOrExpr
    ;

// Логическое ИЛИ (низший приоритет)
logicalOrExpr
    : logicalAndExpr (BAR logicalAndExpr)*
    ;

// Логическое И
logicalAndExpr
    : equalityExpr (AMPERSAND equalityExpr)*
    ;

// Равенство/неравенство
equalityExpr
    : relationalExpr ((EQUAL | NOTEQUAL) relationalExpr)*
    ;

// Отношения (меньше, больше и т.д.)
relationalExpr
    : additiveExpr ((LESS | GREATER | LESSEQUAL | GREATEREQUAL) additiveExpr)*
    ;

// Сложение/вычитание
additiveExpr
    : multiplicativeExpr ((PLUS | MINUS) multiplicativeExpr)*
    ;

// Умножение/деление
multiplicativeExpr
    : unaryOperatorExpr ((MULTIPLY | DIVIDE) unaryOperatorExpr)*
    ;

// Унарный оператор
unaryOperatorExpr
    : MINUS unaryOperatorExpr
    | primaryExpr
    ;

// Выражение без операторов.
primaryExpr
    : STRING
    | INTEGER
    | NIL
    | lvalue
    | IDENTIFIER LPAREN exprListOpt RPAREN              // Вызов функции
    | LPAREN exprSeqOpt RPAREN                          // Выражение в скобках (явное указание приоритета)
    | IDENTIFIER LBRACE fieldListOpt RBRACE             // Создание структуры
    | IDENTIFIER NIL                                    // Создание неинициализированной структуры
    | IDENTIFIER LBRACK expr RBRACK OF expr             // Создание массива
    | IF expr THEN expr (ELSE expr)?                    // Условный оператор
    | WHILE expr DO expr                                // Оператор цикла по условию
    | FOR IDENTIFIER ASSIGN expr TO expr DO expr        // Оператор цикла по диапазону
    | BREAK                                             // Оператор прерывания цикла
    | LET declarationList IN exprSeqOpt END ;           // Оператор области видимости

// Left-values — значения, которые могут стоять слева в присваивании.
lvalue
    : IDENTIFIER                             // Доступ к переменной
    | lvalue DOT IDENTIFIER                  // Доступ к полю структуры (record)
    | lvalue LBRACK expr RBRACK              // Доступ к элементу массива (array)
    ;

exprListOpt
    : /* пустая цепочка */
    | exprList
    ;

// Список выражений, разделённых запятыми ","
exprList
    : expr
    | expr COMMA exprList
    ;

exprSeqOpt
    : /* пустая цепочка */
    | exprSeq
    ;

// Последовательность выражений, разделённых точкой с запятой ";"
exprSeq
    : expr
    | expr SEMICOLON exprSeq
    ;

// Список полей для создания структуры
fieldListOpt
    : /* пустая цепочка */
    | fieldList
    ;

fieldList
    : IDENTIFIER EQUAL expr
    | IDENTIFIER EQUAL expr COMMA fieldList
    ;

// Список объявлений
declarationList
    : declaration
    | declaration declarationList
    ;

declaration
    : typeDeclaration
    | variableDeclaration
    | functionDeclaration
    ;

// Объявление типа
typeDeclaration
    : TYPE IDENTIFIER EQUAL type
    ;

type
    : IDENTIFIER
    | LBRACE typeFieldsOpt RBRACE
    | ARRAY OF IDENTIFIER
    ;

typeFieldsOpt
    : /* пустая цепочка */
    | typeFields
    ;

typeFields
    : typeField
    | typeField COMMA typeFields
    ;

typeField
    : IDENTIFIER COLON IDENTIFIER
    ;

// Объявление переменной
variableDeclaration
    : VAR IDENTIFIER ASSIGN expr
    | VAR IDENTIFIER COLON IDENTIFIER ASSIGN expr
    ;

// Объявление функции
functionDeclaration
    : FUNCTION IDENTIFIER LPAREN typeFieldsOpt RPAREN EQUAL expr
    | FUNCTION IDENTIFIER LPAREN typeFieldsOpt RPAREN COLON IDENTIFIER EQUAL expr
    ;

// Бинарные операторы
binaryOperator
    : PLUS
    | MINUS
    | MULTIPLY
    | DIVIDE
    | EQUAL
    | NOTEQUAL
    | LESS
    | GREATER
    | LESSEQUAL
    | GREATEREQUAL
    | AMPERSAND
    | BAR
    ;