using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;

namespace Compiler.CodeGeneration
{
    /// <summary>
    /// LLVM IR Code Generator for Oberon-0 language
    /// Generates LLVM IR text that can be compiled with clang or llc
    /// </summary>
    public class LLVMCodeGenerator : Oberon0BaseListener
    {
        private readonly StringBuilder ir = new StringBuilder();
        private readonly string moduleName;
        private int tempCounter = 0;
        private int labelCounter = 0;
        private int stringCounter = 0;

        // Symbol tables for tracking variables and their LLVM names
        private readonly Stack<Dictionary<string, LLVMVariable>> scopes = new();
        private readonly Dictionary<string, LLVMVariable> globalVars = new();
        private readonly Dictionary<string, LLVMFunction> functions = new();
        private readonly Dictionary<string, string> stringLiterals = new();
        private readonly Dictionary<string, ArrayTypeInfo> arrayTypes = new();

        // Current function context
        private LLVMFunction currentFunction = null;
        private readonly Stack<string> loopExitLabels = new();
        private readonly Stack<string> loopContinueLabels = new();

        // Track nested procedure declarations
        private readonly Stack<LLVMFunction> functionStack = new();

        public LLVMCodeGenerator(string moduleName)
        {
            this.moduleName = moduleName;
            InitializeBuiltIns();
        }

        /// <summary>
        /// Initialize built-in function declarations (printf, scanf, etc.)
        /// </summary>
        private void InitializeBuiltIns()
        {
            // NOTE: Don't emit anything here - order matters!
            // Target triple and module comment are emitted in EnterModule
            // Then we emit declares, then global strings, then global vars
        }

        private void DefineGlobalString(string name, string value)
        {
            int len = value.Length + 1; // +1 for null terminator
            string escaped = EscapeString(value);
            Emit($"@.{name} = private unnamed_addr constant [{len} x i8] c\"{escaped}\\00\"");
            stringLiterals[name] = $"@.{name}";
        }

        private string EscapeString(string s)
        {
            return s.Replace("\\", "\\\\")
                    .Replace("\n", "\\0A")
                    .Replace("\r", "\\0D")
                    .Replace("\t", "\\09")
                    .Replace("\"", "\\22");
        }

        #region Core Emission Methods

        private void Emit(string line)
        {
            ir.AppendLine(line);
        }

        private void EmitIndented(string line)
        {
            ir.AppendLine("  " + line);
        }

        private string NewTemp()
        {
            return $"%t{tempCounter++}";
        }

        private string NewLabel(string prefix = "label")
        {
            return $"{prefix}{labelCounter++}";
        }

        public string GetIR()
        {
            return ir.ToString();
        }

        public void WriteToFile(string filename)
        {
            File.WriteAllText(filename, GetIR());
        }

        #endregion

        #region Module and Declarations

        public override void EnterModule([NotNull] Oberon0Parser.ModuleContext context)
        {
            // CORRECT ORDER FOR LLVM IR:
            // 1. Module comment and target triple
            Emit($"; Module: {context.ID(0).GetText()}");
            Emit($"target triple = \"x86_64-pc-linux-gnu\"");
            Emit("");

            // 2. External function declarations
            Emit("declare i32 @printf(i8*, ...)");
            Emit("declare i32 @scanf(i8*, ...)");
            Emit("declare i32 @puts(i8*)");
            Emit("");

            // 3. Global format strings
            DefineGlobalString("int_fmt", "%lld");
            DefineGlobalString("real_fmt", "%lf");
            DefineGlobalString("str_fmt", "%s");
            DefineGlobalString("newline", "\n");
            DefineGlobalString("scan_int", "%lld");
            DefineGlobalString("scan_real", "%lf");
            Emit("");

            // 4. Push global scope (global variables will be emitted during traversal)
            scopes.Push(globalVars);
        }

        public override void ExitModule([NotNull] Oberon0Parser.ModuleContext context)
        {
            scopes.Pop();
        }

        public override void EnterTypeDecl([NotNull] Oberon0Parser.TypeDeclContext context)
        {
            string typeName = context.ID().GetText();
            var typeNode = context.type();

            // Check if it's an array type
            if (typeNode.ChildCount > 0 && typeNode.GetChild(0).GetText() == "ARRAY")
            {
                var dimensions = new List<int>();
                var currentType = typeNode;

                // Parse all dimensions
                while (currentType.ChildCount > 0 && currentType.GetChild(0).GetText() == "ARRAY")
                {
                    // Count number of dimensions by counting expressions
                    int dimCount = 0;
                    for (int i = 0; i < currentType.ChildCount; i++)
                    {
                        if (currentType.GetChild(i) is Oberon0Parser.ExpressionContext)
                        {
                            var exprCtx = (Oberon0Parser.ExpressionContext)currentType.GetChild(i);
                            int size = EvaluateConstantExpression(exprCtx);
                            dimensions.Add(size);
                            dimCount++;
                        }
                    }

                    // Find the element type after OF keyword
                    bool foundOf = false;
                    for (int i = 0; i < currentType.ChildCount; i++)
                    {
                        if (currentType.GetChild(i).GetText() == "OF")
                        {
                            foundOf = true;
                            if (i + 1 < currentType.ChildCount && currentType.GetChild(i + 1) is Oberon0Parser.TypeContext)
                            {
                                currentType = (Oberon0Parser.TypeContext)currentType.GetChild(i + 1);
                                break;
                            }
                        }
                    }

                    if (!foundOf || currentType.GetChild(0).GetText() != "ARRAY")
                        break;
                }

                string elementType = GetLLVMType(currentType.GetText());
                arrayTypes[typeName] = new ArrayTypeInfo
                {
                    Dimensions = dimensions,
                    ElementType = elementType,
                    BaseTypeName = currentType.GetText()
                };
            }
        }

        public override void EnterVarDecl([NotNull] Oberon0Parser.VarDeclContext context)
        {
            string typeText = context.type().GetText();
            string llvmType = GetLLVMType(typeText);

            foreach (var id in context.identList().ID())
            {
                string varName = id.GetText();
                string llvmName;

                if (currentFunction == null)
                {
                    // Global variable
                    llvmName = $"@{varName}";
                    Emit($"{llvmName} = global {llvmType} zeroinitializer");
                    globalVars[varName] = new LLVMVariable
                    {
                        Name = varName,
                        LLVMName = llvmName,
                        Type = typeText,
                        LLVMType = llvmType,
                        IsGlobal = true
                    };
                }
                else
                {
                    // Local variable
                    llvmName = $"%{varName}";
                    EmitIndented($"{llvmName} = alloca {llvmType}");
                    scopes.Peek()[varName] = new LLVMVariable
                    {
                        Name = varName,
                        LLVMName = llvmName,
                        Type = typeText,
                        LLVMType = llvmType,
                        IsGlobal = false
                    };
                }
            }
        }

        public override void EnterConstDecl([NotNull] Oberon0Parser.ConstDeclContext context)
        {
            string name = context.ID().GetText();

            // For constants, we'll just track them but not generate code yet
            // Constants will be inlined when used
        }

        #endregion

        #region Procedures and Functions

        public override void EnterProcDecl([NotNull] Oberon0Parser.ProcDeclContext context)
        {
            var heading = context.procHeading();
            string procName = heading.ID().GetText();
            bool isFunction = heading.GetChild(0).GetText() == "FUNCTION";

            string returnType = "void";
            if (isFunction && heading.type() != null)
            {
                returnType = GetLLVMType(heading.type().GetText());
            }

            // Build parameter list
            var parameters = new List<LLVMParameter>();
            if (heading.formalParameters() != null)
            {
                foreach (var fp in heading.formalParameters().fpSection())
                {
                    string paramType = GetLLVMType(fp.type().GetText());
                    bool isByRef = fp.ChildCount > 0 && fp.GetChild(0).GetText() == "VAR";

                    foreach (var id in fp.identList().ID())
                    {
                        parameters.Add(new LLVMParameter
                        {
                            Name = id.GetText(),
                            Type = fp.type().GetText(),
                            LLVMType = isByRef ? $"{paramType}*" : paramType,
                            IsByRef = isByRef
                        });
                    }
                }
            }

            var func = new LLVMFunction
            {
                Name = procName,
                ReturnType = returnType,
                Parameters = parameters,
                IsFunction = isFunction
            };

            // Save current function and push new one
            if (currentFunction != null)
            {
                functionStack.Push(currentFunction);
            }
            currentFunction = func;
            functions[procName] = func;

            // Emit function signature
            string paramList = string.Join(", ", parameters.Select(p => $"{p.LLVMType} %{p.Name}"));
            Emit("");
            Emit($"define {returnType} @{procName}({paramList}) {{");
            EmitIndented("entry:");

            // Create new scope for function
            var localScope = new Dictionary<string, LLVMVariable>();
            scopes.Push(localScope);

            // Allocate space for parameters and store them
            foreach (var param in parameters)
            {
                if (!param.IsByRef)
                {
                    string allocaName = $"%{param.Name}.addr";
                    EmitIndented($"{allocaName} = alloca {param.LLVMType}");
                    EmitIndented($"store {param.LLVMType} %{param.Name}, {param.LLVMType}* {allocaName}");

                    localScope[param.Name] = new LLVMVariable
                    {
                        Name = param.Name,
                        LLVMName = allocaName,
                        Type = param.Type,
                        LLVMType = param.LLVMType,
                        IsGlobal = false
                    };
                }
                else
                {
                    // By-reference parameters are already pointers
                    localScope[param.Name] = new LLVMVariable
                    {
                        Name = param.Name,
                        LLVMName = $"%{param.Name}",
                        Type = param.Type,
                        LLVMType = param.LLVMType.TrimEnd('*'),
                        IsGlobal = false,
                        IsByRef = true
                    };
                }
            }
        }

        public override void ExitProcDecl([NotNull] Oberon0Parser.ProcDeclContext context)
        {
            // Add default return if not present
            if (currentFunction != null)
            {
                if (currentFunction.ReturnType == "void")
                {
                    EmitIndented("ret void");
                }
                else
                {
                    // Return zero/null by default
                    EmitIndented($"ret {currentFunction.ReturnType} {GetDefaultValue(currentFunction.ReturnType)}");
                }

                Emit("}");
            }

            scopes.Pop();

            // Restore previous function context (for nested procedures)
            if (functionStack.Count > 0)
            {
                currentFunction = functionStack.Pop();
            }
            else
            {
                currentFunction = null;
            }
        }

        #endregion

        #region Statements

        public override void EnterAssignment([NotNull] Oberon0Parser.AssignmentContext context)
        {
            var designator = context.designator();
            string varName = designator.ID().GetText();
            var variable = LookupVariable(varName);

            if (variable == null)
            {
                throw new Exception($"Variable not found: {varName}");
            }

            // Evaluate the expression
            var exprResult = EvaluateExpression(context.expression());

            // Handle type conversion if needed
            string valueToStore = exprResult.Value;
            if (exprResult.LLVMType != variable.LLVMType)
            {
                valueToStore = ConvertType(exprResult.Value, exprResult.LLVMType, variable.LLVMType);
            }

            // Handle array indexing
            string targetPtr = variable.LLVMName;
            if (designator.selector().Length > 0)
            {
                targetPtr = HandleDesignatorSelectors(designator, variable);
            }

            // Store the value
            EmitIndented($"store {variable.LLVMType} {valueToStore}, {variable.LLVMType}* {targetPtr}");
        }

        public override void EnterProcedureCall([NotNull] Oberon0Parser.ProcedureCallContext context)
        {
            string procName = context.ID().GetText();

            // Handle built-in procedures
            if (procName == "WRITE" || procName == "WRITELN")
            {
                HandleWriteCall(context, procName == "WRITELN");
                return;
            }
            else if (procName == "READ")
            {
                HandleReadCall(context);
                return;
            }

            // Handle user-defined procedure call
            var func = functions.ContainsKey(procName) ? functions[procName] : null;
            if (func == null)
            {
                throw new Exception($"Procedure not found: {procName}");
            }

            // Evaluate arguments
            var args = new List<ExpressionResult>();
            if (context.expressionList() != null)
            {
                foreach (var expr in context.expressionList().expression())
                {
                    args.Add(EvaluateExpression(expr));
                }
            }

            // Build argument list with type conversions
            var argStrings = new List<string>();
            for (int i = 0; i < args.Count && i < func.Parameters.Count; i++)
            {
                var arg = args[i];
                var param = func.Parameters[i];

                string argValue = arg.Value;
                if (arg.LLVMType != param.LLVMType && !param.IsByRef)
                {
                    argValue = ConvertType(arg.Value, arg.LLVMType, param.LLVMType);
                }

                argStrings.Add($"{param.LLVMType} {argValue}");
            }

            string argList = string.Join(", ", argStrings);

            if (func.ReturnType == "void")
            {
                EmitIndented($"call void @{procName}({argList})");
            }
            else
            {
                string result = NewTemp();
                EmitIndented($"{result} = call {func.ReturnType} @{procName}({argList})");
            }
        }

        public override void EnterIfStatement([NotNull] Oberon0Parser.IfStatementContext context)
        {
            string thenLabel = NewLabel("if.then");
            string elseLabel = NewLabel("if.else");
            string endLabel = NewLabel("if.end");

            // Evaluate condition
            var condResult = EvaluateExpression(context.expression(0));
            string condBool = ConvertToBool(condResult);

            EmitIndented($"br i1 {condBool}, label %{thenLabel}, label %{elseLabel}");

            // THEN block
            Emit($"{thenLabel}:");
        }

        public override void ExitIfStatement([NotNull] Oberon0Parser.IfStatementContext context)
        {
            string endLabel = NewLabel("if.end");
            EmitIndented($"br label %{endLabel}");

            string elseLabel = NewLabel("if.else");
            Emit($"{elseLabel}:");

            EmitIndented($"br label %{endLabel}");
            Emit($"{endLabel}:");
        }

        public override void EnterLoopStatement([NotNull] Oberon0Parser.LoopStatementContext context)
        {
            string firstChild = context.GetChild(0).GetText();

            if (firstChild == "WHILE")
            {
                HandleWhileLoop(context);
            }
            else if (firstChild == "REPEAT")
            {
                HandleRepeatLoop(context);
            }
            else if (firstChild == "FOR")
            {
                HandleForLoop(context);
            }
        }

        private void HandleWhileLoop(Oberon0Parser.LoopStatementContext context)
        {
            string condLabel = NewLabel("while.cond");
            string bodyLabel = NewLabel("while.body");
            string endLabel = NewLabel("while.end");

            loopExitLabels.Push(endLabel);
            loopContinueLabels.Push(condLabel);

            EmitIndented($"br label %{condLabel}");
            Emit($"{condLabel}:");

            var condResult = EvaluateExpression(context.expression(0));
            string condBool = ConvertToBool(condResult);
            EmitIndented($"br i1 {condBool}, label %{bodyLabel}, label %{endLabel}");

            Emit($"{bodyLabel}:");
        }

        public override void ExitLoopStatement([NotNull] Oberon0Parser.LoopStatementContext context)
        {
            string firstChild = context.GetChild(0).GetText();

            if (firstChild == "WHILE" && loopExitLabels.Count > 0)
            {
                string condLabel = loopContinueLabels.Pop();
                string endLabel = loopExitLabels.Pop();

                EmitIndented($"br label %{condLabel}");
                Emit($"{endLabel}:");
            }
        }

        private void HandleRepeatLoop(Oberon0Parser.LoopStatementContext context)
        {
            string bodyLabel = NewLabel("repeat.body");
            string condLabel = NewLabel("repeat.cond");
            string endLabel = NewLabel("repeat.end");

            loopExitLabels.Push(endLabel);
            loopContinueLabels.Push(condLabel);

            EmitIndented($"br label %{bodyLabel}");
            Emit($"{bodyLabel}:");
        }

        private void HandleForLoop(Oberon0Parser.LoopStatementContext context)
        {
            string varName = context.ID().GetText();
            var variable = LookupVariable(varName);

            var startExpr = EvaluateExpression(context.expression(0));
            var endExpr = EvaluateExpression(context.expression(1));

            // Initialize loop variable
            EmitIndented($"store i64 {startExpr.Value}, i64* {variable.LLVMName}");

            string condLabel = NewLabel("for.cond");
            string bodyLabel = NewLabel("for.body");
            string incLabel = NewLabel("for.inc");
            string endLabel = NewLabel("for.end");

            loopExitLabels.Push(endLabel);
            loopContinueLabels.Push(incLabel);

            EmitIndented($"br label %{condLabel}");
            Emit($"{condLabel}:");

            // Load current value and check condition
            string current = NewTemp();
            EmitIndented($"{current} = load i64, i64* {variable.LLVMName}");
            string cmp = NewTemp();

            bool isDownTo = context.ChildCount > 5 && context.GetChild(5).GetText() == "DOWNTO";
            string cmpOp = isDownTo ? "sgt" : "sle";
            EmitIndented($"{cmp} = icmp {cmpOp} i64 {current}, {endExpr.Value}");
            EmitIndented($"br i1 {cmp}, label %{bodyLabel}, label %{endLabel}");

            Emit($"{bodyLabel}:");
        }

        public override void EnterReturnStatement([NotNull] Oberon0Parser.ReturnStatementContext context)
        {
            if (context.expression() != null)
            {
                var result = EvaluateExpression(context.expression());

                // Convert if needed to match function return type
                string valueToReturn = result.Value;
                if (currentFunction != null && result.LLVMType != currentFunction.ReturnType)
                {
                    valueToReturn = ConvertType(result.Value, result.LLVMType, currentFunction.ReturnType);
                }

                EmitIndented($"ret {currentFunction?.ReturnType ?? result.LLVMType} {valueToReturn}");
            }
            else
            {
                EmitIndented("ret void");
            }
        }

        #endregion

        #region Expression Evaluation

        private ExpressionResult EvaluateExpression(Oberon0Parser.ExpressionContext context)
        {
            if (context.simpleExpression().Length == 1)
            {
                return EvaluateSimpleExpression(context.simpleExpression(0));
            }
            else
            {
                // Comparison operation
                var left = EvaluateSimpleExpression(context.simpleExpression(0));
                var right = EvaluateSimpleExpression(context.simpleExpression(1));
                string op = context.GetChild(1).GetText();

                return EvaluateComparison(left, right, op);
            }
        }

        private ExpressionResult EvaluateSimpleExpression(Oberon0Parser.SimpleExpressionContext context)
        {
            ExpressionResult result = null;
            int termStartIndex = 0;

            // Handle unary + or -
            if (context.ChildCount > 0 && (context.GetChild(0).GetText() == "+" || context.GetChild(0).GetText() == "-"))
            {
                result = EvaluateTerm(context.term(0));
                if (context.GetChild(0).GetText() == "-")
                {
                    result = NegateValue(result);
                }
                termStartIndex = 1;
            }
            else
            {
                result = EvaluateTerm(context.term(0));
            }

            // Handle binary operations
            for (int i = termStartIndex; i < context.term().Length - 1; i++)
            {
                int childIndex = (i - termStartIndex) * 2 + (termStartIndex > 0 ? 2 : 1);
                if (childIndex < context.ChildCount)
                {
                    string op = context.GetChild(childIndex).GetText();
                    var right = EvaluateTerm(context.term(i + 1));

                    if (op == "OR")
                    {
                        result = EvaluateLogicalOr(result, right);
                    }
                    else
                    {
                        result = EvaluateBinaryOp(result, right, op);
                    }
                }
            }

            return result;
        }

        private ExpressionResult EvaluateTerm(Oberon0Parser.TermContext context)
        {
            var result = EvaluateFactor(context.factor(0));

            for (int i = 1; i < context.factor().Length; i++)
            {
                int opIndex = i * 2 - 1;
                if (opIndex < context.ChildCount)
                {
                    string op = context.GetChild(opIndex).GetText();
                    var right = EvaluateFactor(context.factor(i));

                    if (op == "AND")
                    {
                        result = EvaluateLogicalAnd(result, right);
                    }
                    else
                    {
                        result = EvaluateBinaryOp(result, right, op);
                    }
                }
            }

            return result;
        }

        private ExpressionResult EvaluateFactor(Oberon0Parser.FactorContext context)
        {
            // Literal
            if (context.literal() != null)
            {
                return EvaluateLiteral(context.literal());
            }

            // Parenthesized expression
            if (context.expression() != null)
            {
                return EvaluateExpression(context.expression());
            }

            // NOT factor
            if (context.ChildCount > 0 && context.GetChild(0).GetText() == "NOT")
            {
                var inner = EvaluateFactor(context.factor());
                return LogicalNot(inner);
            }

            // Designator (variable or function call)
            if (context.designator() != null)
            {
                var designator = context.designator();
                string varName = designator.ID().GetText();

                // Check if it's a function call - factor has optional expressionList
                if (context.expressionList() != null)
                {
                    // This is a function call: designator '(' expressionList? ')'
                    return EvaluateFunctionCallInFactor(varName, context.expressionList());
                }

                // Variable access
                var variable = LookupVariable(varName);
                if (variable == null)
                {
                    throw new Exception($"Variable not found: {varName}");
                }

                // Handle selectors (array indexing, record fields)
                if (designator.selector().Length > 0)
                {
                    string ptr = HandleDesignatorSelectors(designator, variable);
                    string temp = NewTemp();
                    EmitIndented($"{temp} = load {variable.LLVMType}, {variable.LLVMType}* {ptr}");
                    return new ExpressionResult
                    {
                        Value = temp,
                        Type = variable.Type,
                        LLVMType = variable.LLVMType
                    };
                }

                // Simple variable load
                string result = NewTemp();
                EmitIndented($"{result} = load {variable.LLVMType}, {variable.LLVMType}* {variable.LLVMName}");
                return new ExpressionResult
                {
                    Value = result,
                    Type = variable.Type,
                    LLVMType = variable.LLVMType
                };
            }

            throw new Exception("Unknown factor type");
        }

        private ExpressionResult EvaluateLiteral(Oberon0Parser.LiteralContext context)
        {
            if (context.INTEGER_LITERAL() != null)
            {
                return new ExpressionResult
                {
                    Value = context.INTEGER_LITERAL().GetText(),
                    Type = "INTEGER",
                    LLVMType = "i64"
                };
            }
            else if (context.REAL_LITERAL() != null)
            {
                return new ExpressionResult
                {
                    Value = context.REAL_LITERAL().GetText(),
                    Type = "REAL",
                    LLVMType = "double"
                };
            }
            else if (context.STRING_LITERAL() != null)
            {
                string str = context.STRING_LITERAL().GetText();
                str = str.Substring(1, str.Length - 2); // Remove quotes
                string globalName = CreateStringLiteral(str);
                return new ExpressionResult
                {
                    Value = globalName,
                    Type = "STRING",
                    LLVMType = "i8*"
                };
            }
            else if (context.BOOLEAN_LITERAL() != null)
            {
                string val = context.BOOLEAN_LITERAL().GetText() == "TRUE" ? "1" : "0";
                return new ExpressionResult
                {
                    Value = val,
                    Type = "BOOLEAN",
                    LLVMType = "i1"
                };
            }
            else if (context.GetText() == "NIL")
            {
                return new ExpressionResult
                {
                    Value = "null",
                    Type = "POINTER",
                    LLVMType = "i8*"
                };
            }

            throw new Exception("Unknown literal type");
        }

        private ExpressionResult EvaluateFunctionCallInFactor(string funcName, Oberon0Parser.ExpressionListContext exprList)
        {
            var func = functions.ContainsKey(funcName) ? functions[funcName] : null;

            if (func == null)
            {
                throw new Exception($"Function not found: {funcName}");
            }

            // Evaluate arguments
            var args = new List<ExpressionResult>();
            if (exprList != null)
            {
                foreach (var expr in exprList.expression())
                {
                    args.Add(EvaluateExpression(expr));
                }
            }

            // Build argument list
            var argStrings = new List<string>();
            for (int i = 0; i < args.Count && i < func.Parameters.Count; i++)
            {
                var arg = args[i];
                var param = func.Parameters[i];

                string argValue = arg.Value;
                if (arg.LLVMType != param.LLVMType && !param.IsByRef)
                {
                    argValue = ConvertType(arg.Value, arg.LLVMType, param.LLVMType);
                }

                argStrings.Add($"{param.LLVMType} {argValue}");
            }

            string argList = string.Join(", ", argStrings);
            string result = NewTemp();
            EmitIndented($"{result} = call {func.ReturnType} @{funcName}({argList})");

            return new ExpressionResult
            {
                Value = result,
                Type = func.IsFunction ? "FUNCTION_RESULT" : "UNKNOWN",
                LLVMType = func.ReturnType
            };
        }

        #endregion

        #region Binary Operations

        private ExpressionResult EvaluateBinaryOp(ExpressionResult left, ExpressionResult right, string op)
        {
            // Promote types if necessary
            string targetType = PromoteTypes(left.LLVMType, right.LLVMType);
            string leftVal = left.Value;
            string rightVal = right.Value;

            if (left.LLVMType != targetType)
                leftVal = ConvertType(leftVal, left.LLVMType, targetType);
            if (right.LLVMType != targetType)
                rightVal = ConvertType(rightVal, right.LLVMType, targetType);

            string result = NewTemp();
            string instruction = "";

            if (targetType == "i64")
            {
                instruction = op switch
                {
                    "+" => "add",
                    "-" => "sub",
                    "*" => "mul",
                    "/" => "sdiv",
                    "DIV" => "sdiv",
                    "MOD" => "srem",
                    _ => throw new Exception($"Unknown integer operation: {op}")
                };
                EmitIndented($"{result} = {instruction} i64 {leftVal}, {rightVal}");
            }
            else if (targetType == "double")
            {
                instruction = op switch
                {
                    "+" => "fadd",
                    "-" => "fsub",
                    "*" => "fmul",
                    "/" => "fdiv",
                    _ => throw new Exception($"Unknown real operation: {op}")
                };
                EmitIndented($"{result} = {instruction} double {leftVal}, {rightVal}");
            }

            return new ExpressionResult
            {
                Value = result,
                Type = targetType == "i64" ? "INTEGER" : "REAL",
                LLVMType = targetType
            };
        }

        private ExpressionResult EvaluateComparison(ExpressionResult left, ExpressionResult right, string op)
        {
            string targetType = PromoteTypes(left.LLVMType, right.LLVMType);
            string leftVal = left.Value;
            string rightVal = right.Value;

            if (left.LLVMType != targetType)
                leftVal = ConvertType(leftVal, left.LLVMType, targetType);
            if (right.LLVMType != targetType)
                rightVal = ConvertType(rightVal, right.LLVMType, targetType);

            string result = NewTemp();
            string predicate = "";

            if (targetType == "i64")
            {
                predicate = op switch
                {
                    "=" => "eq",
                    "#" => "ne",
                    "<" => "slt",
                    "<=" => "sle",
                    ">" => "sgt",
                    ">=" => "sge",
                    _ => throw new Exception($"Unknown comparison: {op}")
                };
                EmitIndented($"{result} = icmp {predicate} i64 {leftVal}, {rightVal}");
            }
            else if (targetType == "double")
            {
                predicate = op switch
                {
                    "=" => "oeq",
                    "#" => "one",
                    "<" => "olt",
                    "<=" => "ole",
                    ">" => "ogt",
                    ">=" => "oge",
                    _ => throw new Exception($"Unknown comparison: {op}")
                };
                EmitIndented($"{result} = fcmp {predicate} double {leftVal}, {rightVal}");
            }

            return new ExpressionResult
            {
                Value = result,
                Type = "BOOLEAN",
                LLVMType = "i1"
            };
        }

        private ExpressionResult EvaluateLogicalAnd(ExpressionResult left, ExpressionResult right)
        {
            string leftBool = ConvertToBool(left);
            string rightBool = ConvertToBool(right);
            string result = NewTemp();
            EmitIndented($"{result} = and i1 {leftBool}, {rightBool}");
            return new ExpressionResult
            {
                Value = result,
                Type = "BOOLEAN",
                LLVMType = "i1"
            };
        }

        private ExpressionResult EvaluateLogicalOr(ExpressionResult left, ExpressionResult right)
        {
            string leftBool = ConvertToBool(left);
            string rightBool = ConvertToBool(right);
            string result = NewTemp();
            EmitIndented($"{result} = or i1 {leftBool}, {rightBool}");
            return new ExpressionResult
            {
                Value = result,
                Type = "BOOLEAN",
                LLVMType = "i1"
            };
        }

        private ExpressionResult LogicalNot(ExpressionResult operand)
        {
            string boolVal = ConvertToBool(operand);
            string result = NewTemp();
            EmitIndented($"{result} = xor i1 {boolVal}, 1");
            return new ExpressionResult
            {
                Value = result,
                Type = "BOOLEAN",
                LLVMType = "i1"
            };
        }

        private ExpressionResult NegateValue(ExpressionResult operand)
        {
            string result = NewTemp();
            if (operand.LLVMType == "i64")
            {
                EmitIndented($"{result} = sub i64 0, {operand.Value}");
                return new ExpressionResult { Value = result, Type = "INTEGER", LLVMType = "i64" };
            }
            else if (operand.LLVMType == "double")
            {
                EmitIndented($"{result} = fsub double 0.0, {operand.Value}");
                return new ExpressionResult { Value = result, Type = "REAL", LLVMType = "double" };
            }
            throw new Exception("Cannot negate this type");
        }

        #endregion

        #region Built-in I/O Functions

        private void HandleWriteCall(Oberon0Parser.ProcedureCallContext context, bool writeln)
        {
            if (context.expressionList() == null || context.expressionList().expression().Length == 0)
            {
                if (writeln)
                {
                    // Just print newline
                    string nlPtr = NewTemp();
                    EmitIndented($"{nlPtr} = getelementptr [2 x i8], [2 x i8]* @.newline, i32 0, i32 0");
                    EmitIndented($"call i32 @puts(i8* {nlPtr})");
                }
                return;
            }

            var expr = context.expressionList().expression(0);
            var result = EvaluateExpression(expr);

            string fmtPtr = NewTemp();

            if (result.LLVMType == "i64")
            {
                EmitIndented($"{fmtPtr} = getelementptr [5 x i8], [5 x i8]* @.int_fmt, i32 0, i32 0");
                EmitIndented($"call i32 (i8*, ...) @printf(i8* {fmtPtr}, i64 {result.Value})");
            }
            else if (result.LLVMType == "double")
            {
                EmitIndented($"{fmtPtr} = getelementptr [4 x i8], [4 x i8]* @.real_fmt, i32 0, i32 0");
                EmitIndented($"call i32 (i8*, ...) @printf(i8* {fmtPtr}, double {result.Value})");
            }
            else if (result.LLVMType == "i8*")
            {
                EmitIndented($"{fmtPtr} = getelementptr [3 x i8], [3 x i8]* @.str_fmt, i32 0, i32 0");
                EmitIndented($"call i32 (i8*, ...) @printf(i8* {fmtPtr}, i8* {result.Value})");
            }

            if (writeln)
            {
                string nlPtr = NewTemp();
                EmitIndented($"{nlPtr} = getelementptr [2 x i8], [2 x i8]* @.newline, i32 0, i32 0");
                EmitIndented($"call i32 (i8*, ...) @printf(i8* {nlPtr})");
            }
        }

        private void HandleReadCall(Oberon0Parser.ProcedureCallContext context)
        {
            // READ takes a variable as argument in expressionList
            if (context.expressionList() == null || context.expressionList().expression().Length == 0)
            {
                throw new Exception("READ requires a variable argument");
            }

            // Get the variable from the expression
            var expr = context.expressionList().expression(0);
            var simpleExpr = expr.simpleExpression(0);
            var term = simpleExpr.term(0);
            var factor = term.factor(0);

            if (factor.designator() == null)
            {
                throw new Exception("READ requires a variable (designator) as argument");
            }

            var designator = factor.designator();
            string varName = designator.ID().GetText();
            var variable = LookupVariable(varName);

            if (variable == null)
            {
                throw new Exception($"Variable not found: {varName}");
            }

            string fmtPtr = NewTemp();

            if (variable.LLVMType == "i64")
            {
                EmitIndented($"{fmtPtr} = getelementptr [5 x i8], [5 x i8]* @.scan_int, i32 0, i32 0");
                EmitIndented($"call i32 (i8*, ...) @scanf(i8* {fmtPtr}, i64* {variable.LLVMName})");
            }
            else if (variable.LLVMType == "double")
            {
                EmitIndented($"{fmtPtr} = getelementptr [4 x i8], [4 x i8]* @.scan_real, i32 0, i32 0");
                EmitIndented($"call i32 (i8*, ...) @scanf(i8* {fmtPtr}, double* {variable.LLVMName})");
            }
        }

        #endregion

        #region Type System

        private string GetLLVMType(string oberonType)
        {
            return oberonType switch
            {
                "INTEGER" => "i64",
                "REAL" => "double",
                "BOOLEAN" => "i1",
                "STRING" => "i8*",
                _ => HandleComplexType(oberonType)
            };
        }

        private string HandleComplexType(string typeName)
        {
            // Check if it's a user-defined array type
            if (arrayTypes.ContainsKey(typeName))
            {
                var arrayInfo = arrayTypes[typeName];
                string elementType = arrayInfo.ElementType;

                // Build nested array type from outermost to innermost
                string llvmType = elementType;
                for (int i = arrayInfo.Dimensions.Count - 1; i >= 0; i--)
                {
                    llvmType = $"[{arrayInfo.Dimensions[i]} x {llvmType}]";
                }
                return llvmType;
            }

            return "i8*"; // Default for unknown types
        }

        private string PromoteTypes(string type1, string type2)
        {
            if (type1 == "double" || type2 == "double")
                return "double";
            return "i64";
        }

        private string ConvertType(string value, string fromType, string toType)
        {
            if (fromType == toType)
                return value;

            string result = NewTemp();

            if (fromType == "i64" && toType == "double")
            {
                EmitIndented($"{result} = sitofp i64 {value} to double");
            }
            else if (fromType == "double" && toType == "i64")
            {
                EmitIndented($"{result} = fptosi double {value} to i64");
            }
            else if (fromType == "i1" && toType == "i64")
            {
                EmitIndented($"{result} = zext i1 {value} to i64");
            }
            else if (fromType == "i64" && toType == "i1")
            {
                EmitIndented($"{result} = icmp ne i64 {value}, 0");
            }
            else
            {
                return value; // No conversion needed or possible
            }

            return result;
        }

        private string ConvertToBool(ExpressionResult expr)
        {
            if (expr.LLVMType == "i1")
                return expr.Value;

            string result = NewTemp();
            if (expr.LLVMType == "i64")
            {
                EmitIndented($"{result} = icmp ne i64 {expr.Value}, 0");
            }
            else if (expr.LLVMType == "double")
            {
                EmitIndented($"{result} = fcmp one double {expr.Value}, 0.0");
            }
            return result;
        }

        private string GetDefaultValue(string llvmType)
        {
            return llvmType switch
            {
                "i64" => "0",
                "double" => "0.0",
                "i1" => "0",
                "i8*" => "null",
                _ => "zeroinitializer"
            };
        }

        #endregion

        #region Array and Designator Handling

        private string HandleDesignatorSelectors(Oberon0Parser.DesignatorContext designator, LLVMVariable variable)
        {
            string currentPtr = variable.LLVMName;
            string currentType = variable.LLVMType;

            foreach (var selector in designator.selector())
            {
                if (selector.expressionList() != null)
                {
                    // Array indexing
                    var indices = new List<string>();
                    indices.Add("0"); // First index is always 0 for GEP

                    foreach (var expr in selector.expressionList().expression())
                    {
                        var indexResult = EvaluateExpression(expr);
                        indices.Add(indexResult.Value);
                    }

                    string newPtr = NewTemp();
                    string indexList = string.Join(", ", indices.Select(i => $"i64 {i}"));
                    EmitIndented($"{newPtr} = getelementptr {currentType}, {currentType}* {currentPtr}, {indexList}");
                    currentPtr = newPtr;
                }
            }

            return currentPtr;
        }

        #endregion

        #region Helper Methods

        private string CreateStringLiteral(string content)
        {
            string name = $"str{stringCounter++}";
            int len = content.Length + 1;
            string escaped = EscapeString(content);
            Emit($"@.{name} = private unnamed_addr constant [{len} x i8] c\"{escaped}\\00\"");

            string ptr = NewTemp();
            EmitIndented($"{ptr} = getelementptr [{len} x i8], [{len} x i8]* @.{name}, i32 0, i32 0");
            return ptr;
        }

        private LLVMVariable LookupVariable(string name)
        {
            // Search from innermost to outermost scope
            for (int i = scopes.Count - 1; i >= 0; i--)
            {
                if (scopes.ElementAt(i).ContainsKey(name))
                {
                    return scopes.ElementAt(i)[name];
                }
            }
            return null;
        }

        private int EvaluateConstantExpression(Oberon0Parser.ExpressionContext context)
        {
            // Simple constant evaluator for array dimensions
            var text = context.GetText();
            if (int.TryParse(text, out int value))
            {
                return value;
            }
            return 10; // Default size
        }

        public void CreateMainFunction(Oberon0Parser.ModuleContext moduleContext)
        {
            Emit("");
            Emit("; Main entry point");
            Emit("define i32 @main() {");
            Emit("entry:");

            // Call module initialization if there's a BEGIN block
            if (moduleContext.statementSequence() != null)
            {
                // The statements were already emitted during tree walk
            }

            EmitIndented("ret i32 0");
            Emit("}");
        }

        #endregion
    }

    #region Supporting Classes

    class LLVMVariable
    {
        public string Name { get; set; }
        public string LLVMName { get; set; }
        public string Type { get; set; }
        public string LLVMType { get; set; }
        public bool IsGlobal { get; set; }
        public bool IsConstant { get; set; }
        public bool IsByRef { get; set; }
    }

    class LLVMFunction
    {
        public string Name { get; set; }
        public string ReturnType { get; set; }
        public List<LLVMParameter> Parameters { get; set; }
        public bool IsFunction { get; set; }
    }

    class LLVMParameter
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string LLVMType { get; set; }
        public bool IsByRef { get; set; }
    }

    class ExpressionResult
    {
        public string Value { get; set; }
        public string Type { get; set; }
        public string LLVMType { get; set; }
    }

    class ArrayTypeInfo
    {
        public List<int> Dimensions { get; set; }
        public string ElementType { get; set; }
        public string BaseTypeName { get; set; }
    }

    #endregion
}