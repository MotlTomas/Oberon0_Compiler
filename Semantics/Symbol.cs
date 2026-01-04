namespace Compiler.Semantics
{
    public enum SymbolKind
    {
        Variable,
        Constant,
        Procedure,
    }

    public class Symbol
    {
        public string Name { get; }
        public SymbolKind Kind { get; }
        public string Type { get; }
        public Symbol(string name, SymbolKind kind, string type = null)
        {
            Name = name;
            Kind = kind;
            Type = type;
        }
    }


}
