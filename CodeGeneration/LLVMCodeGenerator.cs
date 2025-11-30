using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using LLVMSharp.Interop;

namespace Compiler.CodeGeneration
{
    public class LLVMCodeGenerator : Oberon0BaseListener
    {
        private readonly LLVMModuleRef module;
        private readonly LLVMBuilderRef builder;
        private readonly Dictionary<string, LLVMValueRef> globalVariables = new();
        private readonly Dictionary<string, LLVMValueRef> localVariables = new();
        private readonly Dictionary<string, LLVMValueRef> functions = new();
        private readonly Stack<Dictionary<string, LLVMValueRef>> scopeStack = new();
        private readonly Dictionary<string, LLVMTypeRef> userTypes = new();

        private LLVMValueRef currentFunction;
        private string moduleName = "OberonModule";
        private LLVMBasicBlockRef currentReturnBlock;
        private LLVMValueRef currentReturnValue;

        public LLVMCodeGenerator(string moduleName)
        {
            this.moduleName = moduleName;
            module = LLVMModuleRef.CreateWithName(moduleName);
            builder = module.Context.CreateBuilder();
        }

        public string GetIR()
        {
            return module.ToString();
        }

        public void WriteToFile(string filename)
        {
            if (module.TryPrintToFile(filename, out string error))
            {
                Console.WriteLine($"Error writing LLVM IR: {error}");
            }
        }

        public override void EnterModule(Oberon0Parser.ModuleContext ctx)
        {
            moduleName = ctx.ID(0).GetText();

            // Declare external C functions for I/O
            DeclarePrintf();
            DeclareScanf();
        }

        private void DeclarePrintf()
        {
            var i8PtrType = LLVMTypeRef.CreatePointer(LLVMTypeRef.Int8, 0);
            var printfType = LLVMTypeRef.CreateFunction(LLVMTypeRef.Int32, new[] { i8PtrType }, true);
            var printf = module.AddFunction("printf", printfType);
            functions["printf"] = printf;
        }

        private void DeclareScanf()
        {
            var i8PtrType = LLVMTypeRef.CreatePointer(LLVMTypeRef.Int8, 0);
            var scanfType = LLVMTypeRef.CreateFunction(LLVMTypeRef.Int32, new[] { i8PtrType }, true);
            var scanf = module.AddFunction("scanf", scanfType);
            functions["scanf"] = scanf;
        }

        public override void EnterVarDecl(Oberon0Parser.VarDeclContext ctx)
        {
            var typeNode = ctx.type();
            var llvmType = GetLLVMType(typeNode);

            foreach (var id in ctx.identList().ID())
            {
                string varName = id.GetText();

                if (currentFunction.Handle == IntPtr.Zero)
                {
                    // Global variable
                    var globalVar = module.AddGlobal(llvmType, varName);
                    globalVar.Initializer = GetDefaultValue(llvmType);
                    globalVariables[varName] = globalVar;
                }
                else
                {
                    // Local variable
                    var alloca = builder.BuildAlloca(llvmType, varName);
                    GetCurrentScope()[varName] = alloca;
                }
            }
        }

        public override void EnterConstDecl(Oberon0Parser.ConstDeclContext ctx)
        {
            string name = ctx.ID().GetText();
            var exprValue = EvaluateExpression(ctx.expression());

            if (exprValue.Handle != IntPtr.Zero)
            {
                var constGlobal = module.AddGlobal(exprValue.TypeOf, name);
                constGlobal.IsGlobalConstant = true;
                constGlobal.Initializer = exprValue;
                globalVariables[name] = constGlobal;
            }
        }

        public override void EnterTypeDecl(Oberon0Parser.TypeDeclContext ctx)
        {
            string typeName = ctx.ID().GetText();
            var llvmType = GetLLVMType(ctx.type());
            userTypes[typeName] = llvmType;
        }

        public override void EnterProcDecl(Oberon0Parser.ProcDeclContext ctx)
        {
            var heading = ctx.procHeading();
            string funcName = heading.ID().GetText();

            // Determine return type
            LLVMTypeRef returnType = LLVMTypeRef.Void;
            if (heading.type() != null)
            {
                returnType = GetLLVMType(heading.type());
            }

            // Get parameter types
            var paramTypes = new List<LLVMTypeRef>();
            var paramNames = new List<string>();

            if (heading.formalParameters() != null)
            {
                foreach (var fp in heading.formalParameters().fpSection())
                {
                    var paramType = GetLLVMType(fp.type());
                    foreach (var id in fp.identList().ID())
                    {
                        paramTypes.Add(paramType);
                        paramNames.Add(id.GetText());
                    }
                }
            }

            // Create function
            var funcType = LLVMTypeRef.CreateFunction(returnType, paramTypes.ToArray());
            var function = module.AddFunction(funcName, funcType);
            functions[funcName] = function;
            currentFunction = function;

            // Create entry block
            var entryBlock = currentFunction.AppendBasicBlock("entry");
            builder.PositionAtEnd(entryBlock);

            // Push new scope for function
            scopeStack.Push(new Dictionary<string, LLVMValueRef>());

            // For functions with return values, create return variable
            if (returnType.Kind != LLVMTypeKind.LLVMVoidTypeKind)
            {
                currentReturnValue = builder.BuildAlloca(returnType, "retval");
                currentReturnBlock = currentFunction.AppendBasicBlock("return");
            }

            // Allocate space for parameters and store them
            // NOTE: We don't set param.Name because it causes DLL entry point errors
            // with some LLVM/LLVMSharp version combinations
            for (int i = 0; i < paramNames.Count; i++)
            {
                var param = currentFunction.GetParam((uint)i);
                // param.Name = paramNames[i];  // ← REMOVED - causes "LLVMSetValueName" DLL error
                var alloca = builder.BuildAlloca(param.TypeOf, paramNames[i]);
                builder.BuildStore(param, alloca);
                GetCurrentScope()[paramNames[i]] = alloca;
            }
        }

        public override void ExitProcDecl(Oberon0Parser.ProcDeclContext ctx)
        {
            var returnType = currentFunction.TypeOf.ReturnType;

            // Jump to return block if we have one
            if (returnType.Kind != LLVMTypeKind.LLVMVoidTypeKind)
            {
                // Only add branch if current block doesn't have terminator
                if (builder.InsertBlock.Terminator.Handle == IntPtr.Zero)
                {
                    builder.BuildBr(currentReturnBlock);
                }

                // Build return block
                builder.PositionAtEnd(currentReturnBlock);
                var retVal = builder.BuildLoad2(returnType, currentReturnValue, "retval");
                builder.BuildRet(retVal);

                currentReturnValue = default;
                currentReturnBlock = default;
            }
            else
            {
                // Void function - add return if missing
                if (builder.InsertBlock.Terminator.Handle == IntPtr.Zero)
                {
                    builder.BuildRetVoid();
                }
            }

            scopeStack.Pop();
            currentFunction = default;
        }

        public override void EnterReturnStatement(Oberon0Parser.ReturnStatementContext ctx)
        {
            if (ctx.expression() != null)
            {
                var returnValue = EvaluateExpression(ctx.expression());
                if (returnValue.Handle != IntPtr.Zero && currentReturnValue.Handle != IntPtr.Zero)
                {
                    builder.BuildStore(returnValue, currentReturnValue);
                }
            }

            // Branch to return block
            if (currentReturnBlock.Handle != IntPtr.Zero)
            {
                builder.BuildBr(currentReturnBlock);

                // Create new block for any code after return (unreachable)
                var afterReturn = currentFunction.AppendBasicBlock("after_return");
                builder.PositionAtEnd(afterReturn);
            }
        }

        public override void EnterAssignment(Oberon0Parser.AssignmentContext ctx)
        {
            var designator = ctx.designator();
            string varName = designator.ID().GetText();
            var value = EvaluateExpression(ctx.expression());

            if (value.Handle == IntPtr.Zero)
                return;

            var varPtr = LookupVariable(varName);

            if (varPtr.Handle != IntPtr.Zero)
            {
                // Handle array indexing
                if (designator.selector() != null && designator.selector().Length > 0)
                {
                    varPtr = HandleArrayAccess(designator);
                }

                builder.BuildStore(value, varPtr);
            }
        }

        public override void EnterProcedureCall(Oberon0Parser.ProcedureCallContext ctx)
        {
            string procName = ctx.ID().GetText();

            // Handle built-in I/O
            if (procName == "WRITE" || procName == "WRITELN")
            {
                GenerateWrite(ctx, procName == "WRITELN");
                return;
            }
            if (procName == "READ")
            {
                GenerateRead(ctx);
                return;
            }

            // Regular function call
            if (functions.TryGetValue(procName, out var function))
            {
                var args = new List<LLVMValueRef>();
                if (ctx.expressionList() != null)
                {
                    foreach (var expr in ctx.expressionList().expression())
                    {
                        var arg = EvaluateExpression(expr);
                        if (arg.Handle != IntPtr.Zero)
                            args.Add(arg);
                    }
                }
                builder.BuildCall2(function.TypeOf, function, args.ToArray(), "");
            }
        }

        public override void EnterIfStatement(Oberon0Parser.IfStatementContext ctx)
        {
            var condition = EvaluateExpression(ctx.expression(0));

            if (condition.Handle == IntPtr.Zero)
                return;

            var thenBlock = currentFunction.AppendBasicBlock("if.then");
            var mergeBlock = currentFunction.AppendBasicBlock("if.merge");
            var elseBlock = ctx.statementSequence().Length > 1 ? currentFunction.AppendBasicBlock("if.else") : mergeBlock;

            builder.BuildCondBr(condition, thenBlock, elseBlock);

            // Generate THEN block
            builder.PositionAtEnd(thenBlock);
        }

        public override void EnterLoopStatement(Oberon0Parser.LoopStatementContext ctx)
        {
            if (ctx.GetChild(0).GetText() == "WHILE")
            {
                var condBlock = currentFunction.AppendBasicBlock("while.cond");
                var bodyBlock = currentFunction.AppendBasicBlock("while.body");
                var exitBlock = currentFunction.AppendBasicBlock("while.exit");

                builder.BuildBr(condBlock);
                builder.PositionAtEnd(condBlock);

                var condition = EvaluateExpression(ctx.expression(0));
                if (condition.Handle != IntPtr.Zero)
                {
                    builder.BuildCondBr(condition, bodyBlock, exitBlock);
                    builder.PositionAtEnd(bodyBlock);
                }
            }
        }

        private LLVMValueRef EvaluateExpression(Oberon0Parser.ExpressionContext ctx)
        {
            if (ctx == null) return default;

            var simpleExprs = ctx.simpleExpression();
            var left = EvaluateSimpleExpression(simpleExprs[0]);

            if (left.Handle == IntPtr.Zero)
                return default;

            // Handle comparison operators
            if (simpleExprs.Length > 1)
            {
                var right = EvaluateSimpleExpression(simpleExprs[1]);
                if (right.Handle == IntPtr.Zero)
                    return default;

                var op = ctx.GetChild(1).GetText();

                return op switch
                {
                    "=" => builder.BuildICmp(LLVMIntPredicate.LLVMIntEQ, left, right, "cmpeq"),
                    "#" => builder.BuildICmp(LLVMIntPredicate.LLVMIntNE, left, right, "cmpne"),
                    "<" => builder.BuildICmp(LLVMIntPredicate.LLVMIntSLT, left, right, "cmplt"),
                    "<=" => builder.BuildICmp(LLVMIntPredicate.LLVMIntSLE, left, right, "cmple"),
                    ">" => builder.BuildICmp(LLVMIntPredicate.LLVMIntSGT, left, right, "cmpgt"),
                    ">=" => builder.BuildICmp(LLVMIntPredicate.LLVMIntSGE, left, right, "cmpge"),
                    _ => left
                };
            }

            return left;
        }

        private LLVMValueRef EvaluateSimpleExpression(Oberon0Parser.SimpleExpressionContext ctx)
        {
            var terms = ctx.term();
            var result = EvaluateTerm(terms[0]);

            if (result.Handle == IntPtr.Zero)
                return default;

            for (int i = 1; i < terms.Length; i++)
            {
                var right = EvaluateTerm(terms[i]);
                if (right.Handle == IntPtr.Zero)
                    continue;

                int opIndex = i * 2 - 1;
                if (opIndex < ctx.ChildCount)
                {
                    var op = ctx.GetChild(opIndex).GetText();

                    result = op switch
                    {
                        "+" => builder.BuildAdd(result, right, "add"),
                        "-" => builder.BuildSub(result, right, "sub"),
                        "OR" => builder.BuildOr(result, right, "or"),
                        _ => result
                    };
                }
            }

            return result;
        }

        private LLVMValueRef EvaluateTerm(Oberon0Parser.TermContext ctx)
        {
            var factors = ctx.factor();
            var result = EvaluateFactor(factors[0]);

            if (result.Handle == IntPtr.Zero)
                return default;

            for (int i = 1; i < factors.Length; i++)
            {
                var right = EvaluateFactor(factors[i]);
                if (right.Handle == IntPtr.Zero)
                    continue;

                int opIndex = i * 2 - 1;
                if (opIndex < ctx.ChildCount)
                {
                    var op = ctx.GetChild(opIndex).GetText();

                    result = op switch
                    {
                        "*" => builder.BuildMul(result, right, "mul"),
                        "/" => builder.BuildSDiv(result, right, "div"),
                        "DIV" => builder.BuildSDiv(result, right, "div"),
                        "MOD" => builder.BuildSRem(result, right, "mod"),
                        "AND" => builder.BuildAnd(result, right, "and"),
                        _ => result
                    };
                }
            }

            return result;
        }

        private LLVMValueRef EvaluateFactor(Oberon0Parser.FactorContext ctx)
        {
            // Literal
            if (ctx.literal() != null)
            {
                var lit = ctx.literal();
                if (lit.INTEGER_LITERAL() != null)
                    return LLVMValueRef.CreateConstInt(LLVMTypeRef.Int32, ulong.Parse(lit.GetText()));
                if (lit.REAL_LITERAL() != null)
                    return LLVMValueRef.CreateConstReal(LLVMTypeRef.Double,
                        double.Parse(lit.GetText(), CultureInfo.InvariantCulture));
                if (lit.BOOLEAN_LITERAL() != null)
                    return LLVMValueRef.CreateConstInt(LLVMTypeRef.Int1, lit.GetText() == "TRUE" ? 1UL : 0UL);
                if (lit.STRING_LITERAL() != null)
                {
                    // STRING literal - create global string constant
                    string strValue = lit.GetText().Trim('"');
                    return builder.BuildGlobalStringPtr(strValue, "str");
                }
            }

            // Procedure/function call
            if (ctx.designator() != null && ctx.expressionList() != null)
            {
                var designator = ctx.designator();
                string funcName = designator.ID().GetText();

                if (functions.TryGetValue(funcName, out var function))
                {
                    var args = new List<LLVMValueRef>();
                    foreach (var expr in ctx.expressionList().expression())
                    {
                        var arg = EvaluateExpression(expr);
                        if (arg.Handle != IntPtr.Zero)
                            args.Add(arg);
                    }
                    return builder.BuildCall2(function.TypeOf, function, args.ToArray(), "call");
                }
            }

            // Variable reference
            if (ctx.designator() != null)
            {
                var designator = ctx.designator();
                string varName = designator.ID().GetText();
                var varPtr = LookupVariable(varName);

                if (varPtr.Handle != IntPtr.Zero)
                {
                    // Handle array access if present
                    if (designator.selector() != null && designator.selector().Length > 0)
                    {
                        varPtr = HandleArrayAccess(designator);
                    }

                    // Load the value
                    var elementType = varPtr.TypeOf.ElementType;
                    return builder.BuildLoad2(elementType, varPtr, "load");
                }
            }

            // Parenthesized expression
            if (ctx.expression() != null)
            {
                return EvaluateExpression(ctx.expression());
            }

            // NOT factor
            if (ctx.GetChild(0).GetText() == "NOT")
            {
                var factorCtx = ctx.factor();
                if (factorCtx != null)
                {
                    var operand = EvaluateFactor(factorCtx);
                    if (operand.Handle != IntPtr.Zero)
                        return builder.BuildNot(operand, "not");
                }
            }

            // Default fallback
            return default;
        }

        private void GenerateWrite(Oberon0Parser.ProcedureCallContext ctx, bool newline)
        {
            if (ctx.expressionList() == null || ctx.expressionList().expression().Length == 0)
                return;

            var expr = ctx.expressionList().expression(0);
            var value = EvaluateExpression(expr);

            if (value.Handle == IntPtr.Zero)
                return;

            // Create format string based on type
            string format = "%d";
            if (value.TypeOf.Kind == LLVMTypeKind.LLVMDoubleTypeKind)
                format = "%f";
            else if (value.TypeOf.Kind == LLVMTypeKind.LLVMPointerTypeKind)
                format = "%s";

            if (newline) format += "\\n";

            var formatStr = builder.BuildGlobalStringPtr(format, "fmt");
            builder.BuildCall2(functions["printf"].TypeOf, functions["printf"],
                new[] { formatStr, value }, "");
        }

        private void GenerateRead(Oberon0Parser.ProcedureCallContext ctx)
        {
            if (ctx.expressionList() == null || ctx.expressionList().expression().Length == 0)
                return;

            var expr = ctx.expressionList().expression(0);
            string varName = ExtractVariableName(expr);
            if (string.IsNullOrEmpty(varName)) return;

            var varPtr = LookupVariable(varName);
            if (varPtr.Handle == IntPtr.Zero) return;

            var formatStr = builder.BuildGlobalStringPtr("%d", "fmt");
            builder.BuildCall2(functions["scanf"].TypeOf, functions["scanf"],
                new[] { formatStr, varPtr }, "");
        }

        private string ExtractVariableName(Oberon0Parser.ExpressionContext expr)
        {
            try
            {
                var simpleExpr = expr.simpleExpression(0);
                var term = simpleExpr.term(0);
                var factor = term.factor(0);
                var designator = factor.designator();
                if (designator != null)
                {
                    return designator.ID().GetText();
                }
            }
            catch { }
            return "";
        }

        private LLVMValueRef HandleArrayAccess(Oberon0Parser.DesignatorContext ctx)
        {
            var basePtr = LookupVariable(ctx.ID().GetText());
            var indices = new List<LLVMValueRef> { LLVMValueRef.CreateConstInt(LLVMTypeRef.Int32, 0) };

            foreach (var sel in ctx.selector())
            {
                if (sel.expressionList() != null)
                {
                    foreach (var expr in sel.expressionList().expression())
                    {
                        var idx = EvaluateExpression(expr);
                        if (idx.Handle != IntPtr.Zero)
                            indices.Add(idx);
                    }
                }
            }

            return builder.BuildGEP2(basePtr.TypeOf.ElementType, basePtr, indices.ToArray(), "arrayptr");
        }

        private LLVMTypeRef GetLLVMType(Oberon0Parser.TypeContext ctx)
        {
            var typeText = ctx.GetChild(0).GetText();

            return typeText switch
            {
                "INTEGER" => LLVMTypeRef.Int32,
                "REAL" => LLVMTypeRef.Double,
                "BOOLEAN" => LLVMTypeRef.Int1,
                "STRING" => LLVMTypeRef.CreatePointer(LLVMTypeRef.Int8, 0),
                "ARRAY" => CreateArrayType(ctx),
                _ => userTypes.GetValueOrDefault(typeText, LLVMTypeRef.Int32)
            };
        }

        private LLVMTypeRef CreateArrayType(Oberon0Parser.TypeContext ctx)
        {
            var dimensions = new List<uint>();
            var current = ctx;

            while (current.GetChild(0).GetText() == "ARRAY")
            {
                var exprCount = current.expression().Length;
                for (int i = 0; i < exprCount; i++)
                {
                    dimensions.Add(10);
                }
                current = current.type();
            }

            var elementType = GetLLVMType(current);

            for (int i = dimensions.Count - 1; i >= 0; i--)
            {
                elementType = LLVMTypeRef.CreateArray(elementType, dimensions[i]);
            }

            return elementType;
        }

        private LLVMValueRef GetDefaultValue(LLVMTypeRef type)
        {
            return type.Kind switch
            {
                LLVMTypeKind.LLVMIntegerTypeKind => LLVMValueRef.CreateConstInt(type, 0),
                LLVMTypeKind.LLVMDoubleTypeKind => LLVMValueRef.CreateConstReal(type, 0.0),
                LLVMTypeKind.LLVMPointerTypeKind => LLVMValueRef.CreateConstPointerNull(type),
                LLVMTypeKind.LLVMArrayTypeKind => LLVMValueRef.CreateConstArray(type.ElementType, Array.Empty<LLVMValueRef>()),
                _ => default
            };
        }

        private LLVMValueRef LookupVariable(string name)
        {
            foreach (var scope in scopeStack.Reverse())
            {
                if (scope.TryGetValue(name, out var value))
                    return value;
            }

            if (globalVariables.TryGetValue(name, out var globalValue))
                return globalValue;

            return default;
        }

        private Dictionary<string, LLVMValueRef> GetCurrentScope()
        {
            return scopeStack.Count > 0 ? scopeStack.Peek() : localVariables;
        }

        public void CreateMainFunction(Oberon0Parser.ModuleContext ctx)
        {
            var mainType = LLVMTypeRef.CreateFunction(LLVMTypeRef.Int32, Array.Empty<LLVMTypeRef>());
            var mainFunc = module.AddFunction("main", mainType);
            currentFunction = mainFunc;

            var entryBlock = mainFunc.AppendBasicBlock("entry");
            builder.PositionAtEnd(entryBlock);

            scopeStack.Push(new Dictionary<string, LLVMValueRef>());

            if (ctx.statementSequence() != null)
            {
                var walker = new Antlr4.Runtime.Tree.ParseTreeWalker();
                walker.Walk(this, ctx.statementSequence());
            }

            builder.BuildRet(LLVMValueRef.CreateConstInt(LLVMTypeRef.Int32, 0));

            scopeStack.Pop();
            currentFunction = default;
        }
    }
}