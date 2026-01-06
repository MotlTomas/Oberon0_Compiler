using Antlr4.Runtime.Misc;

namespace Compiler.Semantics
{
    /// <summary>
    /// Semantic analyzer for Oberon-0 language.
    /// </summary>
    public class SemanticAnalyzer : Oberon0BaseVisitor<object>
    {
        // Maps array type names to their element types
        private readonly Dictionary<string, string> arrayTypes = new();
        // Stores parameter types for each procedure
        private readonly Dictionary<string, List<string>> procedureParams = new();
        // Stack of symbol tables (scopes)
        private readonly List<Dictionary<string, Symbol>> scopes = new();
        // Tracks nesting depth for BREAK/CONTINUE validation
        private int loopDepth;

        public List<string> Errors { get; } = new();

        private Dictionary<string, Symbol> GlobalScope => scopes[0];

        // Initialize global scope when visiting module
        public override object VisitModule([NotNull] Oberon0Parser.ModuleContext context)
        {
            scopes.Clear();
            scopes.Add(new Dictionary<string, Symbol>());
            return base.VisitModule(context);
        }

        // Register array types for element type resolution
        public override object VisitTypeDecl([NotNull] Oberon0Parser.TypeDeclContext context)
        {
            var typeNode = context.type();
            if (typeNode.GetChild(0).GetText() == "ARRAY")
                arrayTypes[context.ID().GetText()] = typeNode.type().GetText();

            return null;
        }

        // Add declared variables to symbol table
        public override object VisitVarDecl([NotNull] Oberon0Parser.VarDeclContext context)
        {
            var typeName = context.type().GetText();
            foreach (var id in context.identList().ID())
                AddSymbol(id.GetText(), SymbolKind.Variable, typeName, "Variable");

            return null;
        }


        // Register procedure and check for redeclaration
        public override object VisitProcDecl([NotNull] Oberon0Parser.ProcDeclContext context)
        {
            var name = context.procHeading().ID().GetText();
            if (GlobalScope.ContainsKey(name))
                Errors.Add($"Procedure redeclaration: {name}");
            else
                GlobalScope[name] = new Symbol(name, SymbolKind.Procedure);

            procedureParams[name] = ExtractParameterTypes(context.procHeading());
            return base.VisitProcDecl(context);
        }

        // Create local scope for procedure body
        public override object VisitProcBody([NotNull] Oberon0Parser.ProcBodyContext context)
        {
            scopes.Add(new Dictionary<string, Symbol>());

            var parent = (Oberon0Parser.ProcDeclContext)context.Parent;
            AddParametersToScope(parent.procHeading());

            var result = base.VisitProcBody(context);
            scopes.RemoveAt(scopes.Count - 1);

            return result;
        }

        // Validate variable existence and type compatibility
        public override object VisitAssignment([NotNull] Oberon0Parser.AssignmentContext context)
        {
            var expectedType = GetDesignatorType(context.designator());
            var exprType = InferExpressionType(context.expression());
            var designator = context.designator().GetText();

            if (expectedType == null)
            {
                Errors.Add($"Assignment to undeclared variable: {designator}");
                return null;
            }

            if (exprType != null && expectedType != exprType)
                Errors.Add($"Type mismatch in assignment: {designator} is {expectedType}, but expression is {exprType}");

            return null;
        }

        // Validate procedure exists and arguments match
        public override object VisitProcedureCall([NotNull] Oberon0Parser.ProcedureCallContext context)
        {
            var procName = context.ID().GetText();

            if (!GlobalScope.ContainsKey(procName) && !IsBuiltIn(procName))
            {
                Errors.Add($"Call to undefined procedure: {procName}");
                return null;
            }

            ValidateProcedureArguments(procName, context.expressionList());
            return null;
        }

        // Ensure IF/ELSIF conditions are boolean
        public override object VisitIfStatement([NotNull] Oberon0Parser.IfStatementContext context)
        {
            foreach (var expr in context.expression())
                ValidateCondition(expr, "IF");

            return base.VisitIfStatement(context);
        }

        // Validate loop conditions and track nesting depth
        public override object VisitLoopStatement([NotNull] Oberon0Parser.LoopStatementContext context)
        {
            var keyword = context.GetChild(0).GetText();

            // Validate condition based on loop type
            if (keyword == "WHILE")
                ValidateCondition(context.expression(0), "WHILE");
            else if (keyword == "REPEAT")
                ValidateCondition(context.expression(0), "REPEAT");

            loopDepth++;
            var result = base.VisitLoopStatement(context);
            loopDepth--;

            return result;
        }

        // Validate CASE expression and labels are integers
        public override object VisitSwitchStatement([NotNull] Oberon0Parser.SwitchStatementContext context)
        {
            var exprType = InferExpressionType(context.expression());
            if (exprType != null && exprType != "INTEGER")
                Errors.Add($"CASE expression must be INTEGER, got {exprType}");

            foreach (var caseBranch in context.caseBranch())
            {
                foreach (var literal in caseBranch.literal())
                {
                    var literalType = InferLiteralType(literal);
                    if (literalType != null && literalType != "INTEGER")
                        Errors.Add($"CASE label must be INTEGER, got {literalType}");
                }
            }

            return base.VisitSwitchStatement(context);
        }

        // Check BREAK/CONTINUE are used inside loops
        public override object VisitStatement([NotNull] Oberon0Parser.StatementContext context)
        {
            if (context.ChildCount == 1)
            {
                var text = context.GetChild(0).GetText();
                if (text == "CONTINUE" && loopDepth == 0)
                    Errors.Add("CONTINUE statement must be inside a loop");
                else if (text == "BREAK" && loopDepth == 0)
                    Errors.Add("BREAK statement must be inside a loop");
            }

            return base.VisitStatement(context);
        }

        #region Helper Methods

        // Add symbol to current scope with redeclaration check
        private void AddSymbol(string name, SymbolKind kind, string type, string errorPrefix)
        {
            if (Lookup(name) != null)
                Errors.Add($"{errorPrefix} redeclaration: {name}");
            else
                scopes[^1][name] = new Symbol(name, kind, type);
        }

        // Extract parameter types from procedure heading
        private List<string> ExtractParameterTypes(Oberon0Parser.ProcHeadingContext heading)
        {
            var types = new List<string>();
            if (heading.formalParameters() == null) return types;

            foreach (var fp in heading.formalParameters().fpSection())
            {
                var typeName = fp.type().GetText();
                types.AddRange(Enumerable.Repeat(typeName, fp.identList().ID().Length));
            }

            return types;
        }

        // Add procedure parameters to local scope
        private void AddParametersToScope(Oberon0Parser.ProcHeadingContext heading)
        {
            if (heading.formalParameters() == null) return;

            foreach (var fp in heading.formalParameters().fpSection())
            {
                var typeName = fp.type().GetText();
                foreach (var id in fp.identList().ID())
                    scopes[^1][id.GetText()] = new Symbol(id.GetText(), SymbolKind.Variable, typeName);
            }
        }

        // Validate argument count and types for procedure call
        private void ValidateProcedureArguments(string procName, Oberon0Parser.ExpressionListContext? exprList)
        {
            if (!procedureParams.TryGetValue(procName, out var expectedTypes)) return;

            if (exprList == null)
            {
                if (expectedTypes.Count > 0)
                    Errors.Add($"Procedure {procName}: expected {expectedTypes.Count} arguments, got 0");
                return;
            }

            var args = exprList.expression();
            if (args.Length != expectedTypes.Count)
            {
                Errors.Add($"Procedure {procName}: expected {expectedTypes.Count} arguments, got {args.Length}");
                return;
            }

            for (int i = 0; i < args.Length; i++)
            {
                var argType = InferExpressionType(args[i]);
                if (argType != null && argType != expectedTypes[i])
                    Errors.Add($"Procedure {procName}: argument {i + 1} type mismatch, expected {expectedTypes[i]}, got {argType}");
            }
        }

        // Ensure condition expression is boolean
        private void ValidateCondition(Oberon0Parser.ExpressionContext expr, string statement)
        {
            var condType = InferExpressionType(expr);
            if (condType != null && condType != "BOOLEAN")
                Errors.Add($"{statement} condition must be BOOLEAN, got {condType}");
        }

        // Search for symbol from innermost to outermost scope
        private Symbol Lookup(string name) =>
            scopes.AsEnumerable().Reverse()
                .Select(scope => scope.GetValueOrDefault(name))
                .FirstOrDefault(s => s != null);

        private bool IsBuiltIn(string name) =>
            name is "WRITE" or "WRITELN" or "READ";

        // Resolve type of variable or array element
        private string GetDesignatorType(Oberon0Parser.DesignatorContext ctx)
        {
            var name = ctx.ID().GetText();
            var symbol = Lookup(name);
            if (symbol == null) return null;

            var currType = symbol.Type;
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

        // Infer expression type; comparisons return BOOLEAN
        private string InferExpressionType(Oberon0Parser.ExpressionContext ctx)
        {
            if (ctx == null) return null;

            // Comparison expression
            if (ctx.simpleExpression().Length > 1)
            {
                var leftType = InferSimpleExpressionType(ctx.simpleExpression(0));
                var rightType = InferSimpleExpressionType(ctx.simpleExpression(1));

                if (leftType != null && rightType != null && leftType != rightType)
                    Errors.Add($"Type mismatch in comparison: {leftType} vs {rightType}");

                return "BOOLEAN";
            }

            return InferSimpleExpressionType(ctx.simpleExpression(0));
        }

        // Infer type of additive expression (terms with +/-)
        private string InferSimpleExpressionType(Oberon0Parser.SimpleExpressionContext ctx)
        {
            if (ctx?.term() == null || ctx.term().Length == 0) return null;

            var resultType = InferTermType(ctx.term(0));

            for (int i = 1; i < ctx.term().Length; i++)
            {
                var termType = InferTermType(ctx.term(i));
                if (resultType != null && termType != null && resultType != termType)
                    Errors.Add($"Type mismatch in arithmetic expression: {resultType} vs {termType}");
            }

            return resultType;
        }

        // Infer type of multiplicative expression (factors with */)
        private string InferTermType(Oberon0Parser.TermContext ctx)
        {
            if (ctx?.factor() == null || ctx.factor().Length == 0) return null;

            var resultType = InferFactorType(ctx.factor(0));

            for (int i = 1; i < ctx.factor().Length; i++)
            {
                var factorType = InferFactorType(ctx.factor(i));
                if (resultType != null && factorType != null && resultType != factorType)
                    Errors.Add($"Type mismatch in arithmetic expression: {resultType} vs {factorType}");
            }

            return resultType;
        }

        // Infer type of factor (literal, variable, or parenthesized expression)
        private string InferFactorType(Oberon0Parser.FactorContext factor)
        {
            if (factor == null) return null;

            if (factor.literal() != null)
                return InferLiteralType(factor.literal());

            if (factor.designator() != null)
                return GetDesignatorType(factor.designator());

            if (factor.expression() != null)
                return InferExpressionType(factor.expression());

            // NOT operator
            if (factor.factor() != null)
            {
                var innerType = InferFactorType(factor.factor());
                if (innerType != null && innerType != "BOOLEAN")
                    Errors.Add($"NOT operator requires BOOLEAN, got {innerType}");
                return "BOOLEAN";
            }

            return null;
        }

        // Determine type from literal token
        private string? InferLiteralType(Oberon0Parser.LiteralContext lit) =>
            lit.INTEGER_LITERAL() != null ? "INTEGER" :
            lit.REAL_LITERAL() != null ? "REAL" :
            lit.STRING_LITERAL() != null ? "STRING" :
            lit.BOOLEAN_LITERAL() != null ? "BOOLEAN" : null;

        #endregion
    }
}