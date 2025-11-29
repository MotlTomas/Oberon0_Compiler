grammar OberonSubset;

// -----------------------------
// Entry Point
// -----------------------------
file
    : module EOF
    ;

// -----------------------------
// Program Structure
// -----------------------------
module
    : 'MODULE' IDENT ';'
      declaration_section*
      ('BEGIN' statement_block)?
      'END' IDENT '.'
    ;

// -----------------------------
// Declarations
// -----------------------------
declaration_section
    : const_declaration
    | var_declaration
    | function_def
    ;

const_declaration
    : 'CONST' const_assign ( ';' const_assign )*
    ;

const_assign
    : IDENT '=' expression
    ;

var_declaration
    : 'VAR' var_decl ( ';' var_decl )*
    ;

var_decl
    : IDENT_LIST ':' type
    ;

type
    : 'INTEGER'
    | 'REAL'
    | 'STRING'
    | array_type
    ;

array_type
    : 'ARRAY' expression ( ',' expression )* 'OF' type
    ;

function_def
    : 'PROCEDURE' IDENT '(' parameter_list? ')' 
      (';' declaration_section*)*
      ('BEGIN' statement_block)?
      ('RETURN' expression)?
      'END' IDENT
    ;

// -----------------------------
// Parameters
// -----------------------------
parameter_list
    : parameter ( ';' parameter )*
    ;

parameter
    : ('VAR')? IDENT_LIST ':' type
    ;

// -----------------------------
// Statements
// -----------------------------
statement_block
    : statement*
    ;

statement
    : assignment
    | function_call
    | if_statement
    | while_statement
    | for_statement
    | repeat_statement
    | case_statement
    ;

// -----------------------------
// Control Flow
// -----------------------------
if_statement
    : 'IF' expression 'THEN' statement_block
      ('ELSIF' expression 'THEN' statement_block)*
      ('ELSE' statement_block)?
      'END'
    ;

while_statement
    : 'WHILE' expression 'DO' statement_block
      ('ELSIF' expression 'DO' statement_block)*
      'END'
    ;

for_statement
    : 'FOR' IDENT ':=' expression 'TO' expression ('BY' expression)?
      'DO' statement_block 'END'
    ;

repeat_statement
    : 'REPEAT' statement_block 'UNTIL' expression
    ;

case_statement
    : 'CASE' expression 'OF' case_clause ('|' case_clause)* 'END'
    ;

case_clause
    : literal_list ':' statement_block
    ;

literal_list
    : literal (',' literal)*
    ;

// -----------------------------
// Simple Statements
// -----------------------------
assignment
    : IDENT ':=' expression
    ;

function_call
    : IDENT '(' (expression (',' expression)*)? ')'
    ;

// -----------------------------
// Expressions
// -----------------------------
expression
    : logical_expr
    ;

logical_expr
    : equality_expr (('OR') equality_expr)*
    ;

equality_expr
    : relational_expr (('=' | '#') relational_expr)*
    ;

relational_expr
    : additive_expr (('<' | '<=' | '>' | '>=') additive_expr)*
    ;

additive_expr
    : multiplicative_expr (('+' | '-') multiplicative_expr)*
    ;

multiplicative_expr
    : unary_expr (('*' | '/' | 'DIV' | 'MOD' | '&') unary_expr)*
    ;

unary_expr
    : ('+' | '-' | '~')? primary
    ;

primary
    : IDENT
    | NUMBER
    | STRING
    | '(' expression ')'
    ;

// -----------------------------
// Data Structures
// -----------------------------
IDENT_LIST
    : IDENT (',' IDENT)*
    ;

literal
    : NUMBER
    | STRING
    | 'TRUE'
    | 'FALSE'
    ;

// -----------------------------
// Lexer Rules
// -----------------------------
IDENT
    : [A-Za-z] [A-Za-z0-9_]*
    ;

NUMBER
    : DIGIT+ ('.' DIGIT+)?
    ;

fragment DIGIT
    : [0-9]
    ;

STRING
    : '"' (~["\r\n])* '"'
    ;

WS
    : [ \t\r\n]+ -> skip
    ;

COMMENT
    : '(*' .*? '*)' -> skip
    ;
