using System.Collections.Generic;
using Antlr4.Runtime.Tree;

namespace Compiler.Semantics
{
    // Semantic analyzer for Oberon0 subset
    public class SemanticAnalyzer : Oberon0BaseListener
    {
        // Global symbol table
        private readonly SymbolTable globalSymbols = new();

        // Map user defined array types to their element types
        private readonly Dictionary<string, string> arrayTypes = new();

        // Stack of symbol tables representing nested scopes
        private readonly List<SymbolTable> scopes = new();

        // Collected semantic errors
        public List<string> Errors { get; } = new();

        // Initialize scopes
        public override void EnterModule(Oberon0Parser.ModuleContext ctx)
        {
            scopes.Clear();
            scopes.Add(globalSymbols);
        }

        // Collect user-defined types, store array element types
        public override void EnterTypeDecl(Oberon0Parser.TypeDeclContext ctx)
        {
            string typeName = ctx.ID().GetText();
            var typeNode = ctx.type();
            if (typeNode.GetChild(0).GetText() == "ARRAY")
            {
                var elementType = typeNode.type().GetText();
                arrayTypes[typeName] = elementType;
            }
        }

        // Track variable declarations, check for redeclarations
        public override void EnterVarDecl(Oberon0Parser.VarDeclContext ctx)
        {
            string typeName = ctx.type().GetText();
            foreach (var id in ctx.identList().ID())
            {
                if (Lookup(id.GetText()) != null)
                    Errors.Add($"Variable redeclaration: {id.GetText()}");
                else
                    scopes[^1].Add(new Symbol(id.GetText(), SymbolKind.Variable, typeName));
            }
        }

        // Track constant declarations, check for redeclarations
        public override void EnterConstDecl(Oberon0Parser.ConstDeclContext ctx)
        {
            string name = ctx.ID().GetText();
            if (globalSymbols.Lookup(name) != null)
                Errors.Add($"Constant redeclaration: {name}");
            else
                globalSymbols.Add(new Symbol(name, SymbolKind.Constant, null));
        }

        // Track procedure declarations with redeclaration check
        public override void EnterProcDecl(Oberon0Parser.ProcDeclContext ctx)
        {
            string name = ctx.procHeading().ID().GetText();
            if (globalSymbols.Lookup(name) != null)
                Errors.Add($"Procedure redeclaration: {name}");
            else
                globalSymbols.Add(new Symbol(name, SymbolKind.Procedure));
        }

        // Enter a new procedure body scope, add formal parameters
        public override void EnterProcBody(Oberon0Parser.ProcBodyContext ctx)
        {
            scopes.Add(new SymbolTable());
            var parent = (Oberon0Parser.ProcDeclContext)ctx.Parent;
            var heading = parent.procHeading();
            if (heading.formalParameters() != null)
            {
                foreach (var fp in heading.formalParameters().fpSection())
                {
                    string typeName = fp.type().GetText();
                    foreach (var id in fp.identList().ID())
                        scopes[^1].Add(new Symbol(id.GetText(), SymbolKind.Variable, typeName));
                }
            }
        }

        // Exit procedure scope
        public override void ExitProcBody(Oberon0Parser.ProcBodyContext ctx)
        {
            scopes.RemoveAt(scopes.Count - 1);
        }

        // Check assignment types and existence
        public override void EnterAssignment(Oberon0Parser.AssignmentContext ctx)
        {
            string expectedType = GetDesignatorType(ctx.designator());
            string exprType = InferExpressionType(ctx.expression());
            string designator = ctx.designator().GetText();

            if (expectedType == null)
            {
                Errors.Add($"Assignment to undeclared variable: {designator}");
                return;
            }
            if (exprType != null && expectedType != exprType)
                Errors.Add($"Type mismatch in assignment: {designator} is {expectedType}, but expression is {exprType}");
        }

        // Check procedure call existence
        public override void EnterProcedureCall(Oberon0Parser.ProcedureCallContext ctx)
        {
            string procName = ctx.ID().GetText();
            var proc = globalSymbols.Lookup(procName);
            if (proc == null && !IsBuiltIn(procName))
                Errors.Add($"Call to undefined procedure: {procName}");
        }

        // Lookup symbol through nested scopes
        private Symbol Lookup(string name)
        {
            for (int i = scopes.Count - 1; i >= 0; i--)
            {
                var s = scopes[i].Lookup(name);
                if (s != null) return s;
            }
            return null;
        }

        // Built-in procedures recognized by the compiler
        private bool IsBuiltIn(string name) =>
            name == "WRITE" || name == "WRITELN" || name == "READ";

        private string GetDesignatorType(Oberon0Parser.DesignatorContext ctx)
        {
            string name = ctx.ID().GetText();
            Symbol symbol = Lookup(name);
            if (symbol == null)
                return null;

            string currType = symbol.Type;
            foreach (var sel in ctx.selector())
            {
                if (sel.expressionList() != null)
                {
                    if (arrayTypes.ContainsKey(currType))
                        currType = arrayTypes[currType];
                    else
                        return null;
                }
            }
            return currType;
        }

        // Infer expression type from literals or referenced variables
        private string InferExpressionType(Oberon0Parser.ExpressionContext ctx)
        {
            if (ctx == null) return null;
            var se = ctx.simpleExpression()[0];
            var term = se.term()[0];
            var factor = term.factor()[0];
            if (factor.literal() != null)
            {
                var lit = factor.literal();
                if (lit.INTEGER_LITERAL() != null)
                    return "INTEGER";
                if (lit.REAL_LITERAL() != null)
                    return "REAL";
                if (lit.STRING_LITERAL() != null)
                    return "STRING";
                if (lit.BOOLEAN_LITERAL() != null)
                    return "BOOLEAN";
            }
            if (factor.designator() != null)
                return GetDesignatorType(factor.designator());

            return null;
        }
    }
}
