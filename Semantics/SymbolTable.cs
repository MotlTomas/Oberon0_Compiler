namespace Compiler.Semantics
{
    public class SymbolTable
    {
        private readonly Dictionary<string, Symbol> symbols = new();

        public void Add(Symbol symbol)
        {
            if (symbols.ContainsKey(symbol.Name))
                throw new Exception($"Redeklarace identifikátoru: {symbol.Name}");
            symbols[symbol.Name] = symbol;
        }

        public Symbol Lookup(string name)
        {
            return symbols.TryGetValue(name, out var s) ? s : null;
        }

    }

}
