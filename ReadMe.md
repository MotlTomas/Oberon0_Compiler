# Oberon0 Kompilátor

Kompilátor pro podmnožinu jazyka Oberon-0 v C# s využitím ANTLR4 a LLVM.

## Funkce

- **Datové typy**: INTEGER, REAL, BOOLEAN, STRING, vícerozměrná pole, uživatelské typy
- **Řídicí struktury**: IF/ELSIF/ELSE, WHILE, FOR/DOWNTO, REPEAT/UNTIL, CASE/OF, BREAK, CONTINUE
- **Procedury a funkce**: parametry hodnotou i odkazem (VAR), rekurze, návratové hodnoty
- **Sémantická analýza**: kontrola typů, redefinice, nedeklarované proměnné, argumenty procedur
- **Generování kódu**: LLVM IR → exe soubor (clang)

## Struktura projektu

```
├── Grammar/Oberon0.g4          # Gramatika ANTLR4
├── Generated/                  # Vygenerované soubory (lexer, parser, visitor)
├── SemanticAnalysis/           # Sémantický analyzátor
├── CodeGeneration/             # Generátor LLVM IR
├── TestProgrammes/             # Testovací programy (.ob)
└── Output/                     # Výstupní soubory (.ll, .exe)
```

## Generování parseru z gramatiky

```bash
java -jar Tools/antlr-4.13.2-complete.jar -Dlanguage=CSharp -no-listener -visitor -o Generated Grammar/Oberon0.g4
```

## Build a spuštění

```bash
dotnet build
dotnet run
```

Kompilátor zpracuje všechny `.ob` soubory ve složce `TestProgrammes`, provede sémantickou analýzu a vygeneruje LLVM IR. Pokud je dostupný clang, zkompiluje výstup do spustitelného souboru.
