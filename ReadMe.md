# Oberon0 Compiler (C# + ANTLR4)

A compact compiler and semantic analyzer for an Oberon0 language inspired subset, made with C# and ANTLR4.

## Features

- Parses an Oberon-like language with support for INTEGER, REAL, BOOLEAN, STRING, and user-defined array types.
- Handles modular/global variables, local scopes, constants, function/procedure declarations, and nested procedures.
- Performs semantic checks:
  - Redeclaration of variables, procedures, constants
  - Vaariable lookup with nesting support
  - Type safety for assignments and procedure calls
  - Verification of array element access and multidimensional arrays

## Generating C# Files from G4 Grammar

To generate parser/lexer/listener C# targets from the `.g4` grammar file, run:

	java -jar Tools/antlr-4.13.2-complete.jar -Dlanguage=CSharp -o Generated Grammar/Oberon0.g4

- Ensure grammar files are referenced correctly (e.g., `Grammar/Oberon0.g4`)
- The generated `.cs` files will appear in the `Generated` folder