grammar Oberon0;

file
    : module EOF
    ;

module
    : 'MODULE' ID ';' declarations
      ('BEGIN' statementSequence)? 'END' ID '.'
    ;

declarations
    : ('TYPE' (typeDecl ';')*)?
      ('VAR' (varDecl ';')*)?
      (procDecl ';')*
    ;

typeDecl
    : ID '=' type
    ;

varDecl
    : identList ':' type
    ;

identList
    : ID (',' ID)*
    ;

type
    : 'BOOLEAN' | 'INTEGER' | 'REAL' | 'STRING'
    | 'ARRAY' expression (',' expression)* 'OF' type
    | 'RECORD' (fieldDecl (';' fieldDecl)*)? 'END'
    | 'POINTER' 'TO' type
    | ID
    ;

fieldDecl
    : identList ':' type
    ;

procDecl
    : procHeading ';' procBody
    ;

procHeading
    : ('PROCEDURE' | 'FUNCTION') ID formalParameters (':' type)?
    ;

procBody
    : declarations 'BEGIN' statementSequence? 'END' ID
    | EXTERNAL
    ;

formalParameters
    : '(' (fpSection (';' fpSection)*)? ')'
    ;

fpSection
    : 'VAR'? identList ':' type
    ;

statementSequence
    : statement (';' statement)* ';'?
    ;

statement
    : assignment
    | procedureCall
    | ifStatement
    | loopStatement
    | switchStatement
    | ioStatement
    | returnStatement 
    | 'CONTINUE'
    | 'BREAK'
    ;

assignment
    : designator ':=' expression
    ;

designator
    : ID selector*
    ;

selector
    : '.' ID
    | '[' expressionList ']'
    | '^'
    ;

procedureCall
    : ID selector* '(' expressionList? ')'
    ;

ifStatement
    : 'IF' expression 'THEN' statementSequence
      ('ELSIF' expression 'THEN' statementSequence)*
      ('ELSE' statementSequence)?
      'END'
    ;

loopStatement
    : 'WHILE' expression 'DO' statementSequence 'END'
    | 'REPEAT' statementSequence 'UNTIL' expression
    | 'FOR' ID ':=' expression ('TO' | 'DOWNTO') expression
        'DO' statementSequence 'END'
    ;

switchStatement
    : 'CASE' expression 'OF'
      caseBranch ('|' caseBranch)*
      ('ELSE' statementSequence)?
      'END'
    ;

caseBranch
    : literal (',' literal)* ':' statementSequence
    ;

ioStatement
    : 'WRITE' '(' expression ')'
    | 'WRITELN' '(' expression? ')'
    | 'READ' '(' designator ')'
    ;

returnStatement: 'RETURN' expression?;

expression
    : simpleExpression (('=' | '#' | '<' | '<=' | '>' | '>=') simpleExpression)?
    ;

simpleExpression
    : ('+' | '-')? term (('+' | '-' | 'OR') term)*
    ;

term
    : factor (('*' | '/' | 'DIV' | 'MOD' | 'AND') factor)*
    ;

factor
    : designator ('(' expressionList? ')')?
    | literal
    | '(' expression ')'
    | 'NOT' factor
    ;

expressionList
    : expression (',' expression)*
    ;

literal
    : BOOLEAN_LITERAL
    | INTEGER_LITERAL
    | REAL_LITERAL
    | STRING_LITERAL
    | 'NIL'
    ;

BOOLEAN_LITERAL : 'TRUE' | 'FALSE' ;
INTEGER_LITERAL : [0-9]+ ;
REAL_LITERAL : [0-9]+ '.' [0-9]+ ;
STRING_LITERAL : '"' (~["\r\n])* '"' ;
ID : [a-zA-Z_] [a-zA-Z_0-9]* ;
COMMENT : '(*' .*? '*)' -> skip ;
EXTERNAL : 'EXTERNAL' ;
WS : [ \t\r\n]+ -> skip ;
