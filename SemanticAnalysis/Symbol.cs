namespace Compiler.Semantics;

public enum SymbolKind { Variable, Procedure }

public record Symbol(string Name, SymbolKind Kind, string Type = null);
