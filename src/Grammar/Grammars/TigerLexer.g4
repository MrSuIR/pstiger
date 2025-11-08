lexer grammar TigerLexer;

// Ключевые слова.
ARRAY: 'array';
BREAK: 'break';
DO: 'do';
ELSE: 'else';
END: 'end';
FOR: 'for';
FUNCTION: 'function';
IF: 'if';
IN: 'in';
LET: 'let';
NIL: 'nil';
OF: 'of';
THEN: 'then';
TO: 'to';
TYPE: 'type';
VAR: 'var';
WHILE: 'while';

// Операторы и разделители.
COMMA: ',';
COLON: ':';
SEMICOLON: ';';
LPAREN: '(';
RPAREN: ')';
LBRACK: '[';
RBRACK: ']';
LBRACE: '{';
RBRACE: '}';
DOT: '.';
PLUS: '+';
MINUS: '-';
MULTIPLY: '*';
DIVIDE: '/';
EQUAL: '=';
LESS: '<';
GREATER: '>';
LESSEQUAL: '<=';
GREATEREQUAL: '>=';
AMPERSAND: '&';
BAR: '|';
ASSIGN: ':=';
NOTEQUAL: '<>';

// Идентификаторы.
ID: [a-zA-Z][a-zA-Z0-9_]*;

// Пробельные символы и комментарии.
WS: [ \t\r\n\f]+ -> skip;
COMMENT: '/*' (COMMENT | .)*? '*/' -> skip;

// Литералы (числовые и строковые).
INTEGER: DIGIT+;
STRING: '"' (ESC | .)*? '"';

fragment ESC: '\\' ('n' | 't' | '"' | '\\' | '^' CONTROL | 'd' DIGIT DIGIT DIGIT )
    | '\\' [\p{White_Space}]+ '\\';

fragment DIGIT: '0'..'9' ;

fragment CONTROL : '@' | 'A' | 'B' | 'C' | 'D' | 'E' | 'F' | 'G' | 'H' | 'I' | 'J' | 'K' | 'L' | 'M' | 'N' | 'O' | 'P' | 'Q' | 'R' | 'S' | 'T' | 'U' | 'V' | 'W' | 'X' | 'Y' | 'Z' | '[' | '\\' | ']' | '^' | '_' | '?' ;
