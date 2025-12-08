using System.Collections.Generic;
using Antlr4.Runtime.Misc;

namespace Compiler.Semantics
{
    /// <summary>
    /// Semantic analyzer using Visitor pattern instead of Listener pattern
    /// Provides better control flow and ability to return values from visits
    /// </summary>
    public class SemanticVisitor : Oberon0BaseVisitor<object>
    {
        // Global symbol table
        private readonly SymbolTable globalSymbols = new();

        // Map user defined array types to their element types
        private readonly Dictionary<string, string> arrayTypes = new();

        // Stack of symbol tables representing nested scopes
        private readonly List<SymbolTable> scopes = new();

        // Collected semantic errors
        public List<string> Errors { get; } = new();

        // Visit module - initialize scopes
        public override object VisitModule([NotNull] Oberon0Parser.ModuleContext context)
        {
            scopes.Clear();
            scopes.Add(globalSymbols);
            
            // Continue visiting children
            return base.VisitModule(context);
        }

        // Visit type declaration - collect user-defined types
        public override object VisitTypeDecl([NotNull] Oberon0Parser.TypeDeclContext context)
        {
            string typeName = context.ID().GetText();
            var typeNode = context.type();
            if (typeNode.GetChild(0).GetText() == "ARRAY")
            {
                var elementType = typeNode.type().GetText();
                arrayTypes[typeName] = elementType;
            }
            
            return base.VisitTypeDecl(context);
        }

        // Visit variable declaration - track variables and check for redeclarations
        public override object VisitVarDecl([NotNull] Oberon0Parser.VarDeclContext context)
        {
            string typeName = context.type().GetText();
            foreach (var id in context.identList().ID())
            {
                string varName = id.GetText();
                if (Lookup(varName) != null)
                    Errors.Add($"Variable redeclaration: {varName}");
                else
                    scopes[^1].Add(new Symbol(varName, SymbolKind.Variable, typeName));
            }
            
            return null;
        }

        // Visit constant declaration - track constants and check for redeclarations
        public override object VisitConstDecl([NotNull] Oberon0Parser.ConstDeclContext context)
        {
            string name = context.ID().GetText();
            if (globalSymbols.Lookup(name) != null)
                Errors.Add($"Constant redeclaration: {name}");
            else
                globalSymbols.Add(new Symbol(name, SymbolKind.Constant, null));
            
            return null;
        }

        // Visit procedure declaration - track procedures with redeclaration check
        public override object VisitProcDecl([NotNull] Oberon0Parser.ProcDeclContext context)
        {
            string name = context.procHeading().ID().GetText();
            if (globalSymbols.Lookup(name) != null)
                Errors.Add($"Procedure redeclaration: {name}");
            else
                globalSymbols.Add(new Symbol(name, SymbolKind.Procedure));
            
            // Visit procedure body (which will handle scope)
            return base.VisitProcDecl(context);
        }

        // Visit procedure body - enter new scope, add formal parameters
        public override object VisitProcBody([NotNull] Oberon0Parser.ProcBodyContext context)
        {
            scopes.Add(new SymbolTable());
            
            var parent = (Oberon0Parser.ProcDeclContext)context.Parent;
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
            
            // Visit children
            var result = base.VisitProcBody(context);
            
            // Exit procedure scope
            scopes.RemoveAt(scopes.Count - 1);
            
            return result;
        }

        // Visit assignment - check types and existence
        public override object VisitAssignment([NotNull] Oberon0Parser.AssignmentContext context)
        {
            string expectedType = GetDesignatorType(context.designator());
            string exprType = InferExpressionType(context.expression());
            string designator = context.designator().GetText();

            if (expectedType == null)
            {
                Errors.Add($"Assignment to undeclared variable: {designator}");
                return null;
            }
            
            if (exprType != null && expectedType != exprType)
                Errors.Add($"Type mismatch in assignment: {designator} is {expectedType}, but expression is {exprType}");
            
            return null;
        }

        // Visit procedure call - check existence
        public override object VisitProcedureCall([NotNull] Oberon0Parser.ProcedureCallContext context)
        {
            string procName = context.ID().GetText();
            var proc = globalSymbols.Lookup(procName);
            if (proc == null && !IsBuiltIn(procName))
                Errors.Add($"Call to undefined procedure: {procName}");
            
            // Visit expression list if present
            if (context.expressionList() != null)
            {
                Visit(context.expressionList());
            }
            
            return null;
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
