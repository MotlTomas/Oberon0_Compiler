using Antlr4.Runtime.Misc;

namespace Compiler.Semantics
{
    /// <summary>
    /// Semantic analyzer using Visitor pattern instead of Listener pattern
    /// Provides better control flow and ability to return values from visits
    /// </summary>
    public class SemanticAnalyzer : Oberon0BaseVisitor<object>
    {
        // Global symbol table
        private readonly SymbolTable globalSymbols = new();

        // Map user defined array types to their element types
        private readonly Dictionary<string, string> arrayTypes = new();
        
        // Store procedure parameter types for type checking
        private readonly Dictionary<string, List<string>> procedureParams = new();

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
            
            // Store parameter types for later type checking
            var paramTypes = new List<string>();
            var heading = context.procHeading();
            if (heading.formalParameters() != null)
            {
                foreach (var fp in heading.formalParameters().fpSection())
                {
                    string typeName = fp.type().GetText();
                    foreach (var _ in fp.identList().ID())
                    {
                        paramTypes.Add(typeName);
                    }
                }
            }
            procedureParams[name] = paramTypes;
            
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

        // Visit procedure call - check existence and argument types
        public override object VisitProcedureCall([NotNull] Oberon0Parser.ProcedureCallContext context)
        {
            string procName = context.ID().GetText();
            var proc = globalSymbols.Lookup(procName);
            if (proc == null && !IsBuiltIn(procName))
            {
                Errors.Add($"Call to undefined procedure: {procName}");
                return null;
            }
            
            // Check argument types against parameter types
            if (procedureParams.TryGetValue(procName, out var expectedTypes) && context.expressionList() != null)
            {
                var args = context.expressionList().expression();
                if (args.Length != expectedTypes.Count)
                {
                    Errors.Add($"Procedure {procName}: expected {expectedTypes.Count} arguments, got {args.Length}");
                }
                else
                {
                    for (int i = 0; i < args.Length; i++)
                    {
                        string argType = InferExpressionType(args[i]);
                        if (argType != null && argType != expectedTypes[i])
                        {
                            Errors.Add($"Procedure {procName}: argument {i + 1} type mismatch, expected {expectedTypes[i]}, got {argType}");
                        }
                    }
                }
            }
            
            return null;
        }
        
        // Visit IF statement - check that condition is BOOLEAN
        public override object VisitIfStatement([NotNull] Oberon0Parser.IfStatementContext context)
        {
            foreach (var expr in context.expression())
            {
                string condType = InferExpressionType(expr);
                if (condType != null && condType != "BOOLEAN")
                {
                    Errors.Add($"IF condition must be BOOLEAN, got {condType}");
                }
            }
            
            // Continue visiting children
            return base.VisitIfStatement(context);
        }
        
        // Visit WHILE/FOR loop - check that condition is BOOLEAN
        public override object VisitLoopStatement([NotNull] Oberon0Parser.LoopStatementContext context)
        {
            string keyword = context.GetChild(0).GetText();
            
            if (keyword == "WHILE")
            {
                string condType = InferExpressionType(context.expression(0));
                if (condType != null && condType != "BOOLEAN")
                {
                    Errors.Add($"WHILE condition must be BOOLEAN, got {condType}");
                }
            }
            
            // Continue visiting children
            return base.VisitLoopStatement(context);
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
        // Also checks for type mismatches in binary operations
        private string InferExpressionType(Oberon0Parser.ExpressionContext ctx)
        {
            if (ctx == null) return null;
            
            // Handle comparison expressions (return BOOLEAN)
            if (ctx.simpleExpression().Length > 1)
            {
                // This is a comparison: left op right
                string leftType = InferSimpleExpressionType(ctx.simpleExpression(0));
                string rightType = InferSimpleExpressionType(ctx.simpleExpression(1));
                
                if (leftType != null && rightType != null && leftType != rightType)
                {
                    Errors.Add($"Type mismatch in comparison: {leftType} vs {rightType}");
                }
                
                return "BOOLEAN";
            }
            
            return InferSimpleExpressionType(ctx.simpleExpression(0));
        }
        
        private string InferSimpleExpressionType(Oberon0Parser.SimpleExpressionContext ctx)
        {
            if (ctx == null) return null;
            
            var terms = ctx.term();
            if (terms.Length == 0) return null;
            
            string resultType = InferTermType(terms[0]);
            
            // Check all terms have the same type
            for (int i = 1; i < terms.Length; i++)
            {
                string termType = InferTermType(terms[i]);
                if (resultType != null && termType != null && resultType != termType)
                {
                    Errors.Add($"Type mismatch in arithmetic expression: {resultType} vs {termType}");
                }
            }
            
            return resultType;
        }
        
        private string InferTermType(Oberon0Parser.TermContext ctx)
        {
            if (ctx == null) return null;
            
            var factors = ctx.factor();
            if (factors.Length == 0) return null;
            
            string resultType = InferFactorType(factors[0]);
            
            // Check all factors have the same type
            for (int i = 1; i < factors.Length; i++)
            {
                string factorType = InferFactorType(factors[i]);
                if (resultType != null && factorType != null && resultType != factorType)
                {
                    Errors.Add($"Type mismatch in arithmetic expression: {resultType} vs {factorType}");
                }
            }
            
            return resultType;
        }
        
        private string InferFactorType(Oberon0Parser.FactorContext factor)
        {
            if (factor == null) return null;
            
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
            
            if (factor.expression() != null)
                return InferExpressionType(factor.expression());
            
            if (factor.factor() != null)
            {
                // NOT factor
                string innerType = InferFactorType(factor.factor());
                if (innerType != null && innerType != "BOOLEAN")
                {
                    Errors.Add($"NOT operator requires BOOLEAN, got {innerType}");
                }
                return "BOOLEAN";
            }
            
            return null;
        }
    }
}
