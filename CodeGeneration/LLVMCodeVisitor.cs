using System;
using System.Collections.Generic;
using System.Linq;
using Antlr4.Runtime.Misc;
using LLVMSharp.Interop;

namespace Compiler.CodeGeneration
{
    /// <summary>
    /// LLVM IR Code Generator using Visitor pattern
    /// Provides better control flow and type safety compared to Listener pattern
    /// </summary>
    public class LLVMCodeVisitor : Oberon0BaseVisitor<LLVMValueRef>
    {
        private readonly LLVMModuleRef module;
        private readonly LLVMBuilderRef builder;
        private readonly string moduleName;

        // Symbol tables
        private readonly Stack<Dictionary<string, Variable>> scopes = new();
        private readonly Dictionary<string, Variable> globalVars = new();
        private readonly Dictionary<string, Function> functions = new();
        
        // Type registry for user-defined types (e.g., IntMatrix = ARRAY 2, 2 OF INTEGER)
        private readonly Dictionary<string, LLVMTypeRef> userTypes = new();
        private readonly Dictionary<string, Oberon0Parser.TypeContext> userTypeContexts = new();

        // Current context
        private Function? currentFunction = null;
        private readonly Stack<LoopContext> loopStack = new();
        private readonly Stack<Function> functionStack = new();
        
        // Store module BEGIN block for main() function
        private Oberon0Parser.StatementSequenceContext? moduleBeginBlock = null;

        public LLVMCodeVisitor(string moduleName)
        {
            this.moduleName = moduleName;

            // Create LLVM module and builder
            module = LLVMModuleRef.CreateWithName(moduleName);
            builder = module.Context.CreateBuilder();

            // Set target triple for Windows MSVC
            module.Target = "x86_64-pc-windows-msvc";

            InitializeBuiltIns();
        }

        public override LLVMValueRef VisitIoStatement([NotNull] Oberon0Parser.IoStatementContext context)
        {
            // Forms:
            // WRITE ( expression )
            // WRITELN ( [expression] )
            // READ ( designator )
            var first = context.GetChild(0).GetText();
            if (first == "WRITE")
            {
                var expr = context.expression();
                if (expr != null)
                {
                    var val = Visit(expr);
                    var printf = module.GetNamedFunction("printf");
                    var fmt = val.TypeOf.Kind switch
                    {
                        LLVMTypeKind.LLVMIntegerTypeKind => "%lld",
                        LLVMTypeKind.LLVMDoubleTypeKind => "%lf",
                        LLVMTypeKind.LLVMPointerTypeKind => "%s",
                        _ => "%d"
                    };
                    var fmtStr = builder.BuildGlobalStringPtr(fmt, ".fmt");
                    builder.BuildCall2(printf.TypeOf, printf, new[] { fmtStr, val });
                }
                return default;
            }

            if (first == "WRITELN")
            {
                var expr = context.expression();
                var printf = module.GetNamedFunction("printf");
                if (expr == null)
                {
                    var nl = builder.BuildGlobalStringPtr("\n", ".nl");
                    builder.BuildCall2(printf.TypeOf, printf, new[] { nl });
                    return default;
                }

                var val = Visit(expr);
                var fmt = val.TypeOf.Kind switch
                {
                    LLVMTypeKind.LLVMIntegerTypeKind => "%lld",
                    LLVMTypeKind.LLVMDoubleTypeKind => "%lf",
                    LLVMTypeKind.LLVMPointerTypeKind => "%s",
                    _ => "%d"
                };
                fmt += "\n";
                var fmtStr = builder.BuildGlobalStringPtr(fmt, ".fmt");
                builder.BuildCall2(printf.TypeOf, printf, new[] { fmtStr, val });
                return default;
            }

            if (first == "READ")
            {
                var designator = context.designator();
                if (designator == null)
                    throw new Exception("READ requires a designator");
                var varName = designator.ID().GetText();
                var variable = LookupVariable(varName);
                if (variable == null)
                    throw new Exception($"Variable not found: {varName}");

                var scanf = module.GetNamedFunction("scanf");
                string fmt = variable.Type.Kind == LLVMTypeKind.LLVMDoubleTypeKind ? "%lf" : "%lld";
                var fmtStr = builder.BuildGlobalStringPtr(fmt, ".scan");
                builder.BuildCall2(scanf.TypeOf, scanf, new[] { fmtStr, variable.Value });
                return default;
            }

            return default;
        }

        private void InitializeBuiltIns()
        {
            // Declare printf: i32 @printf(i8*, ...)
            var printfType = LLVMTypeRef.CreateFunction(
                LLVMTypeRef.Int32,
                new[] { LLVMTypeRef.CreatePointer(LLVMTypeRef.Int8, 0) },
                true);
            module.AddFunction("printf", printfType);

            // Declare scanf: i32 @scanf(i8*, ...)
            var scanfType = LLVMTypeRef.CreateFunction(
                LLVMTypeRef.Int32,
                new[] { LLVMTypeRef.CreatePointer(LLVMTypeRef.Int8, 0) },
                true);
            module.AddFunction("scanf", scanfType);

            // Declare puts: i32 @puts(i8*)
            var putsType = LLVMTypeRef.CreateFunction(
                LLVMTypeRef.Int32,
                new[] { LLVMTypeRef.CreatePointer(LLVMTypeRef.Int8, 0) });
            module.AddFunction("puts", putsType);
        }

        #region Module and Declarations

        public override LLVMValueRef VisitModule([NotNull] Oberon0Parser.ModuleContext context)
        {
            scopes.Push(globalVars);
            
            // Visit declarations
            if (context.declarations() != null)
            {
                Visit(context.declarations());
            }
            
            // Store the BEGIN block (statementSequence) for later execution in main()
            if (context.statementSequence() != null)
            {
                moduleBeginBlock = context.statementSequence();
            }
            
            scopes.Pop();
            return default;
        }

        public override LLVMValueRef VisitDeclarations([NotNull] Oberon0Parser.DeclarationsContext context)
        {
            // Process TYPE declarations first
            foreach (var typeDecl in context.typeDecl())
            {
                Visit(typeDecl);
            }
            
            // Process VAR declarations
            foreach (var varDecl in context.varDecl())
            {
                Visit(varDecl);
            }
            
            // Process PROCEDURE declarations
            foreach (var procDecl in context.procDecl())
            {
                Visit(procDecl);
            }
            
            return default;
        }

        public override LLVMValueRef VisitTypeDecl([NotNull] Oberon0Parser.TypeDeclContext context)
        {
            string typeName = context.ID().GetText();
            var typeContext = context.type();
            
            // Store the type context for later resolution
            userTypeContexts[typeName] = typeContext;
            
            // Create the LLVM type
            var llvmType = GetLLVMType(typeContext);
            userTypes[typeName] = llvmType;
            
            return default;
        }

        public override LLVMValueRef VisitVarDecl([NotNull] Oberon0Parser.VarDeclContext context)
        {
            var llvmType = GetLLVMType(context.type());

            foreach (var id in context.identList().ID())
            {
                string varName = id.GetText();
                LLVMValueRef value;

                if (currentFunction == null)
                {
                    // Global variable
                    value = module.AddGlobal(llvmType, varName);
                    value.Initializer = LLVMValueRef.CreateConstNull(llvmType);

                    globalVars[varName] = new Variable
                    {
                        Name = varName,
                        Value = value,
                        Type = llvmType,
                        IsGlobal = true
                    };
                }
                else
                {
                    // Local variable
                    value = builder.BuildAlloca(llvmType, varName);

                    scopes.Peek()[varName] = new Variable
                    {
                        Name = varName,
                        Value = value,
                        Type = llvmType,
                        IsGlobal = false
                    };
                }
            }

            return default;
        }

        #endregion

        #region Procedures and Functions

        public override LLVMValueRef VisitProcDecl([NotNull] Oberon0Parser.ProcDeclContext context)
        {
            var heading = context.procHeading();
            string procName = heading.ID().GetText();

            // Determine return type
            LLVMTypeRef returnType = LLVMTypeRef.Void;
            if (heading.type() != null)
            {
                returnType = GetLLVMType(heading.type());
            }

            // Build parameter types
            var paramTypes = new List<LLVMTypeRef>();
            var paramNames = new List<string>();
            var paramIsByRef = new List<bool>();
            var paramBaseTypes = new List<LLVMTypeRef>();  // Store base type for arrays

            if (heading.formalParameters() != null)
            {
                foreach (var fp in heading.formalParameters().fpSection())
                {
                    var baseType = GetLLVMType(fp.type());
                    bool isByRef = fp.ChildCount > 0 && fp.GetChild(0).GetText() == "VAR";
                    
                    // Arrays and VAR parameters are passed by pointer
                    var paramType = baseType;
                    if (isByRef || baseType.Kind == LLVMTypeKind.LLVMArrayTypeKind)
                    {
                        paramType = LLVMTypeRef.CreatePointer(baseType, 0);
                    }

                    foreach (var id in fp.identList().ID())
                    {
                        paramTypes.Add(paramType);
                        paramNames.Add(id.GetText());
                        paramIsByRef.Add(isByRef || baseType.Kind == LLVMTypeKind.LLVMArrayTypeKind);
                        paramBaseTypes.Add(baseType);
                    }
                }
            }

            // Create function type and add to module
            var funcType = LLVMTypeRef.CreateFunction(returnType, paramTypes.ToArray());
            var func = module.AddFunction(procName, funcType);

            var function = new Function
            {
                Name = procName,
                Value = func,
                FunctionType = funcType,
                ReturnType = returnType,
                Parameters = paramNames,
                ParameterTypes = paramTypes
            };

            // Save context (including current insert block for nested procedures)
            LLVMBasicBlockRef savedInsertBlock = default;
            if (currentFunction != null)
            {
                functionStack.Push(currentFunction);
                savedInsertBlock = builder.InsertBlock;
            }
            currentFunction = function;
            functions[procName] = function;

            // Create entry block
            var entry = func.AppendBasicBlock("entry");
            builder.PositionAtEnd(entry);

            // Create new scope
            var localScope = new Dictionary<string, Variable>();
            scopes.Push(localScope);

            // Allocate and store parameters
            for (int i = 0; i < paramNames.Count; i++)
            {
                var param = func.GetParam((uint)i);
                var paramType = paramTypes[i];
                var baseType = paramBaseTypes[i];
                bool isByRef = paramIsByRef[i];

                if (isByRef)
                {
                    // For VAR parameters and arrays, the parameter is already a pointer
                    localScope[paramNames[i]] = new Variable
                    {
                        Name = paramNames[i],
                        Value = param,
                        Type = baseType,
                        IsByRef = true
                    };
                }
                else
                {
                    // Value parameter - allocate and copy
                    var alloca = builder.BuildAlloca(paramType, $"{paramNames[i]}.addr");
                    builder.BuildStore(param, alloca);

                    localScope[paramNames[i]] = new Variable
                    {
                        Name = paramNames[i],
                        Value = alloca,
                        Type = paramType
                    };
                }
            }

            // Visit procedure body
            Visit(context.procBody());

            // Add default return to any basic blocks without terminators
            if (currentFunction != null)
            {
                // Iterate through all basic blocks and add terminators where missing
                var block = currentFunction.Value.FirstBasicBlock;
                while (block.Handle != IntPtr.Zero)
                {
                    if (block.Terminator.Handle == IntPtr.Zero)
                    {
                        builder.PositionAtEnd(block);

                        if (currentFunction.ReturnType.Kind == LLVMTypeKind.LLVMVoidTypeKind)
                        {
                            builder.BuildRetVoid();
                        }
                        else
                        {
                            builder.BuildRet(LLVMValueRef.CreateConstNull(currentFunction.ReturnType));
                        }
                    }
                    block = block.Next;
                }
            }

            scopes.Pop();

            // Restore previous function context and builder position
            if (functionStack.Count > 0)
            {
                currentFunction = functionStack.Pop();
                // Restore builder position to the saved insert block of the parent function
                if (savedInsertBlock.Handle != IntPtr.Zero)
                {
                    builder.PositionAtEnd(savedInsertBlock);
                }
            }
            else
            {
                currentFunction = null;
            }

            return default;
        }

        public override LLVMValueRef VisitProcBody([NotNull] Oberon0Parser.ProcBodyContext context)
        {
            // Save current insert block before visiting nested declarations (which may define nested procedures)
            var savedBlock = builder.InsertBlock;

            // Visit declarations (including nested procedures)
            if (context.declarations() != null)
            {
                Visit(context.declarations());
            }

            // Restore insert block position after processing nested procedures
            // so that statements are emitted into the correct function
            if (savedBlock.Handle != IntPtr.Zero)
            {
                builder.PositionAtEnd(savedBlock);
            }

            // Visit statements
            if (context.statementSequence() != null)
            {
                Visit(context.statementSequence());
            }

            return default;
        }

        #endregion

        #region Statements

        public override LLVMValueRef VisitStatementSequence([NotNull] Oberon0Parser.StatementSequenceContext context)
        {
            foreach (var stmt in context.statement())
            {
                Visit(stmt);
            }
            return default;
        }

        public override LLVMValueRef VisitStatement([NotNull] Oberon0Parser.StatementContext context)
        {
            if (context.assignment() != null)
            {
                Visit(context.assignment());
            }
            else if (context.procedureCall() != null)
            {
                Visit(context.procedureCall());
            }
            else if (context.ioStatement() != null)
            {
                Visit(context.ioStatement());
            }
            else if (context.ifStatement() != null)
            {
                Visit(context.ifStatement());
            }
            else if (context.loopStatement() != null)
            {
                Visit(context.loopStatement());
            }
            else if (context.switchStatement() != null)
            {
                Visit(context.switchStatement());
            }
            else if (context.returnStatement() != null)
            {
                Visit(context.returnStatement());
            }
            // Add other statement types as needed

            return default;
        }

        public override LLVMValueRef VisitAssignment([NotNull] Oberon0Parser.AssignmentContext context)
        {
            var designator = context.designator();
            string varName = designator.ID().GetText();
            var variable = LookupVariable(varName);

            if (variable == null)
            {
                throw new Exception($"Variable not found: {varName}");
            }

            // Check for array element assignment
            var selectors = designator.selector();
            if (selectors != null && selectors.Length > 0)
            {
                return StoreArrayElement(variable, selectors, context.expression());
            }

            // Evaluate expression
            var exprValue = Visit(context.expression());

            // Check if this is an array assignment (res := mat)
            if (variable.Type.Kind == LLVMTypeKind.LLVMArrayTypeKind)
            {
                // For array assignment, we need to copy the entire array
                // Get the source array pointer
                var sourceVar = GetSourceVariable(context.expression());
                if (sourceVar != null && sourceVar.Type.Kind == LLVMTypeKind.LLVMArrayTypeKind)
                {
                    // Use memcpy for array copy
                    CopyArray(variable, sourceVar);
                    return default;
                }
            }

            // Type conversion if needed
            if (exprValue.TypeOf != variable.Type)
            {
                exprValue = ConvertType(exprValue, variable.Type);
            }

            // Store value
            builder.BuildStore(exprValue, variable.Value);
            return default;
        }

        private LLVMValueRef StoreArrayElement(Variable variable, Oberon0Parser.SelectorContext[] selectors, Oberon0Parser.ExpressionContext expr)
        {
            // Handle array element assignment: a[i, j] := value
            var indices = new List<LLVMValueRef>();
            
            // First index is always 0 for GEP into the array
            indices.Add(LLVMValueRef.CreateConstInt(LLVMTypeRef.Int64, 0));
            
            foreach (var selector in selectors)
            {
                // Check if this is an array index selector [...]
                var exprList = selector.expressionList();
                if (exprList != null)
                {
                    foreach (var indexExpr in exprList.expression())
                    {
                        var indexValue = Visit(indexExpr);
                        // Ensure index is i64
                        if (indexValue.TypeOf != LLVMTypeRef.Int64)
                        {
                            indexValue = ConvertType(indexValue, LLVMTypeRef.Int64);
                        }
                        indices.Add(indexValue);
                    }
                }
            }
            
            // Build GEP to get pointer to element
            var elementPtr = builder.BuildInBoundsGEP2(variable.Type, variable.Value, indices.ToArray(), "arrayidx");
            
            // Determine element type
            var elementType = GetArrayElementType(variable.Type, indices.Count - 1);
            
            // Evaluate expression and store
            var value = Visit(expr);
            if (value.TypeOf != elementType)
            {
                value = ConvertType(value, elementType);
            }
            
            builder.BuildStore(value, elementPtr);
            return default;
        }

        private Variable? GetSourceVariable(Oberon0Parser.ExpressionContext expr)
        {
            // Try to extract variable from simple expression
            try
            {
                var simpleExpr = expr.simpleExpression(0);
                var term = simpleExpr.term(0);
                var factor = term.factor(0);
                var designator = factor.designator();
                if (designator != null)
                {
                    return LookupVariable(designator.ID().GetText());
                }
            }
            catch { }
            return null;
        }

        private void CopyArray(Variable dest, Variable source)
        {
            // Calculate array size
            var arrayType = dest.Type;
            ulong arraySize = GetTypeSize(arrayType);
            
            // Get base pointers as i8*
            var destPtr = builder.BuildBitCast(dest.Value, LLVMTypeRef.CreatePointer(LLVMTypeRef.Int8, 0), "dest.ptr");
            var srcPtr = builder.BuildBitCast(source.Value, LLVMTypeRef.CreatePointer(LLVMTypeRef.Int8, 0), "src.ptr");
            
            // Use C library memcpy instead of LLVM intrinsic
            var memcpyType = LLVMTypeRef.CreateFunction(
                LLVMTypeRef.CreatePointer(LLVMTypeRef.Int8, 0),  // returns void*
                new[] { 
                    LLVMTypeRef.CreatePointer(LLVMTypeRef.Int8, 0),  // dest
                    LLVMTypeRef.CreatePointer(LLVMTypeRef.Int8, 0),  // src
                    LLVMTypeRef.Int64  // size
                });
            
            var memcpy = module.GetNamedFunction("memcpy");
            if (memcpy.Handle == IntPtr.Zero)
            {
                memcpy = module.AddFunction("memcpy", memcpyType);
            }
            
            builder.BuildCall2(memcpyType, memcpy, new[] {
                destPtr,
                srcPtr,
                LLVMValueRef.CreateConstInt(LLVMTypeRef.Int64, arraySize)
            });
        }

        private ulong GetTypeSize(LLVMTypeRef type)
        {
            if (type.Kind == LLVMTypeKind.LLVMIntegerTypeKind)
            {
                return type.IntWidth / 8;
            }
            else if (type.Kind == LLVMTypeKind.LLVMDoubleTypeKind)
            {
                return 8;
            }
            else if (type.Kind == LLVMTypeKind.LLVMArrayTypeKind)
            {
                return type.ArrayLength * GetTypeSize(type.ElementType);
            }
            return 8; // Default
        }

        public override LLVMValueRef VisitProcedureCall([NotNull] Oberon0Parser.ProcedureCallContext context)
        {
            string procName = context.ID().GetText();

            // Handle built-in procedures
            if (procName == "WRITE" || procName == "WRITELN")
            {
                HandleWrite(context, procName == "WRITELN");
                return default;
            }
            else if (procName == "READ")
            {
                HandleRead(context);
                return default;
            }

            // User-defined procedure/function
            if (!functions.TryGetValue(procName, out var func))
            {
                throw new Exception($"Procedure not found: {procName}");
            }

            // Evaluate arguments
            var args = new List<LLVMValueRef>();
            if (context.expressionList() != null)
            {
                var expressions = context.expressionList().expression();
                for (int i = 0; i < expressions.Length; i++)
                {
                    var expr = expressions[i];
                    var paramType = func.ParameterTypes[i];
                    
                    // Check if parameter expects a pointer (VAR parameter or array)
                    if (paramType.Kind == LLVMTypeKind.LLVMPointerTypeKind)
                    {
                        // Get the variable directly and pass its address
                        var sourceVar = GetSourceVariable(expr);
                        if (sourceVar != null)
                        {
                            args.Add(sourceVar.Value);
                            continue;
                        }
                    }
                    
                    args.Add(Visit(expr));
                }
            }

            // Call the function
            return builder.BuildCall2(func.FunctionType, func.Value, args.ToArray());
        }

        public override LLVMValueRef VisitIfStatement([NotNull] Oberon0Parser.IfStatementContext context)
        {
            if (currentFunction == null) return default;

            var func = currentFunction.Value;
            var thenBlock = func.AppendBasicBlock("if.then");
            var elseBlock = func.AppendBasicBlock("if.else");
            var endBlock = func.AppendBasicBlock("if.end");

            // Evaluate condition
            var condition = Visit(context.expression(0));
            condition = ConvertToBool(condition);
            builder.BuildCondBr(condition, thenBlock, elseBlock);

            // THEN block
            builder.PositionAtEnd(thenBlock);
            Visit(context.statementSequence(0));
            
            if (builder.InsertBlock.Terminator.Handle == IntPtr.Zero)
            {
                builder.BuildBr(endBlock);
            }

            // ELSE block
            builder.PositionAtEnd(elseBlock);
            if (context.statementSequence().Length > 1)
            {
                Visit(context.statementSequence(1));
            }
            
            if (builder.InsertBlock.Terminator.Handle == IntPtr.Zero)
            {
                builder.BuildBr(endBlock);
            }

            // Continue at end block
            builder.PositionAtEnd(endBlock);
            return default;
        }

        public override LLVMValueRef VisitReturnStatement([NotNull] Oberon0Parser.ReturnStatementContext context)
        {
            if (context.expression() != null)
            {
                var value = Visit(context.expression());

                if (currentFunction != null && value.TypeOf != currentFunction.ReturnType)
                {
                    value = ConvertType(value, currentFunction.ReturnType);
                }

                return builder.BuildRet(value);
            }
            else
            {
                return builder.BuildRetVoid();
            }
        }

        public override LLVMValueRef VisitLoopStatement([NotNull] Oberon0Parser.LoopStatementContext context)
        {
            if (currentFunction == null) return default;

            var func = currentFunction.Value;
            var keyword = context.GetChild(0).GetText();

            if (keyword == "WHILE")
            {
                // WHILE expression DO statementSequence END
                var condBlock = func.AppendBasicBlock("while.cond");
                var bodyBlock = func.AppendBasicBlock("while.body");
                var endBlock = func.AppendBasicBlock("while.end");

                // Push loop context for BREAK/CONTINUE support
                loopStack.Push(new LoopContext { CondBlock = condBlock, EndBlock = endBlock });

                // Jump to condition
                builder.BuildBr(condBlock);

                // Condition block
                builder.PositionAtEnd(condBlock);
                var cond = Visit(context.expression(0));
                cond = ConvertToBool(cond);
                builder.BuildCondBr(cond, bodyBlock, endBlock);

                // Body block
                builder.PositionAtEnd(bodyBlock);
                Visit(context.statementSequence());
                if (builder.InsertBlock.Terminator.Handle == IntPtr.Zero)
                {
                    builder.BuildBr(condBlock);
                }

                // End block
                builder.PositionAtEnd(endBlock);
                loopStack.Pop();
            }
            else if (keyword == "REPEAT")
            {
                // REPEAT statementSequence UNTIL expression
                var bodyBlock = func.AppendBasicBlock("repeat.body");
                var condBlock = func.AppendBasicBlock("repeat.cond");
                var endBlock = func.AppendBasicBlock("repeat.end");

                loopStack.Push(new LoopContext { CondBlock = condBlock, EndBlock = endBlock });

                // Jump to body
                builder.BuildBr(bodyBlock);

                // Body block
                builder.PositionAtEnd(bodyBlock);
                Visit(context.statementSequence());
                if (builder.InsertBlock.Terminator.Handle == IntPtr.Zero)
                {
                    builder.BuildBr(condBlock);
                }

                // Condition block (at the end)
                builder.PositionAtEnd(condBlock);
                var cond = Visit(context.expression(0));
                cond = ConvertToBool(cond);
                // REPEAT UNTIL: exit when condition is TRUE
                builder.BuildCondBr(cond, endBlock, bodyBlock);

                // End block
                builder.PositionAtEnd(endBlock);
                loopStack.Pop();
            }
            else if (keyword == "FOR")
            {
                // FOR ID := expression (TO | DOWNTO) expression DO statementSequence END
                var loopVarName = context.ID().GetText();
                var loopVar = LookupVariable(loopVarName);
                if (loopVar == null)
                {
                    throw new Exception($"Loop variable not found: {loopVarName}");
                }

                // Determine direction (TO or DOWNTO)
                bool isDownTo = context.GetChild(4).GetText() == "DOWNTO";

                // Initialize loop variable
                var startValue = Visit(context.expression(0));
                builder.BuildStore(startValue, loopVar.Value);

                // Get end value
                var endValue = Visit(context.expression(1));

                var condBlock = func.AppendBasicBlock("for.cond");
                var bodyBlock = func.AppendBasicBlock("for.body");
                var incBlock = func.AppendBasicBlock("for.inc");
                var endBlock = func.AppendBasicBlock("for.end");

                loopStack.Push(new LoopContext { CondBlock = incBlock, EndBlock = endBlock });

                // Jump to condition
                builder.BuildBr(condBlock);

                // Condition block
                builder.PositionAtEnd(condBlock);
                var currentValue = builder.BuildLoad2(loopVar.Type, loopVar.Value, loopVarName);
                LLVMValueRef cmp;
                if (isDownTo)
                {
                    // DOWNTO: continue while i >= end
                    cmp = builder.BuildICmp(LLVMIntPredicate.LLVMIntSGE, currentValue, endValue, "for.cmp");
                }
                else
                {
                    // TO: continue while i <= end
                    cmp = builder.BuildICmp(LLVMIntPredicate.LLVMIntSLE, currentValue, endValue, "for.cmp");
                }
                builder.BuildCondBr(cmp, bodyBlock, endBlock);

                // Body block
                builder.PositionAtEnd(bodyBlock);
                Visit(context.statementSequence());
                if (builder.InsertBlock.Terminator.Handle == IntPtr.Zero)
                {
                    builder.BuildBr(incBlock);
                }

                // Increment/Decrement block
                builder.PositionAtEnd(incBlock);
                currentValue = builder.BuildLoad2(loopVar.Type, loopVar.Value, loopVarName);
                LLVMValueRef nextValue;
                if (isDownTo)
                {
                    nextValue = builder.BuildSub(currentValue, LLVMValueRef.CreateConstInt(LLVMTypeRef.Int64, 1), "for.dec");
                }
                else
                {
                    nextValue = builder.BuildAdd(currentValue, LLVMValueRef.CreateConstInt(LLVMTypeRef.Int64, 1), "for.inc");
                }
                builder.BuildStore(nextValue, loopVar.Value);
                builder.BuildBr(condBlock);

                // End block
                builder.PositionAtEnd(endBlock);
                loopStack.Pop();
            }

            return default;
        }

        public override LLVMValueRef VisitSwitchStatement([NotNull] Oberon0Parser.SwitchStatementContext context)
        {
            if (currentFunction == null) return default;

            var func = currentFunction.Value;

            // Evaluate the CASE expression
            var switchValue = Visit(context.expression());

            // Create blocks for each case branch and the end/else block
            var caseBranches = context.caseBranch();
            var caseBlocks = new List<LLVMBasicBlockRef>();
            var caseCondBlocks = new List<LLVMBasicBlockRef>();
            
            for (int i = 0; i < caseBranches.Length; i++)
            {
                caseCondBlocks.Add(func.AppendBasicBlock($"case.cond.{i}"));
                caseBlocks.Add(func.AppendBasicBlock($"case.body.{i}"));
            }

            var elseBlock = func.AppendBasicBlock("case.else");
            var endBlock = func.AppendBasicBlock("case.end");

            // Jump to first case condition
            if (caseCondBlocks.Count > 0)
            {
                builder.BuildBr(caseCondBlocks[0]);
            }
            else
            {
                builder.BuildBr(elseBlock);
            }

            // Generate code for each case branch
            for (int i = 0; i < caseBranches.Length; i++)
            {
                var branch = caseBranches[i];
                var literals = branch.literal();

                // Condition block - check if value matches any of the literals
                builder.PositionAtEnd(caseCondBlocks[i]);

                LLVMValueRef matchCondition = default;
                foreach (var literal in literals)
                {
                    var literalValue = Visit(literal);
                    var cmp = builder.BuildICmp(LLVMIntPredicate.LLVMIntEQ, switchValue, literalValue, $"case.cmp.{i}");
                    
                    if (matchCondition.Handle == IntPtr.Zero)
                    {
                        matchCondition = cmp;
                    }
                    else
                    {
                        // OR the conditions together (for multiple literals like 1, 2, 3: ...)
                        matchCondition = builder.BuildOr(matchCondition, cmp, "case.or");
                    }
                }

                // If match, go to body; otherwise go to next case or else
                var nextBlock = (i + 1 < caseCondBlocks.Count) ? caseCondBlocks[i + 1] : elseBlock;
                builder.BuildCondBr(matchCondition, caseBlocks[i], nextBlock);

                // Body block
                builder.PositionAtEnd(caseBlocks[i]);
                Visit(branch.statementSequence());
                
                if (builder.InsertBlock.Terminator.Handle == IntPtr.Zero)
                {
                    builder.BuildBr(endBlock);
                }
            }

            // ELSE block
            builder.PositionAtEnd(elseBlock);
            if (context.statementSequence() != null)
            {
                Visit(context.statementSequence());
            }
            
            if (builder.InsertBlock.Terminator.Handle == IntPtr.Zero)
            {
                builder.BuildBr(endBlock);
            }

            // End block
            builder.PositionAtEnd(endBlock);

            return default;
        }

        #endregion

        #region Expression Evaluation

        public override LLVMValueRef VisitExpression([NotNull] Oberon0Parser.ExpressionContext context)
        {
            if (context.simpleExpression().Length == 1)
            {
                return Visit(context.simpleExpression(0));
            }
            else
            {
                // Comparison
                var left = Visit(context.simpleExpression(0));
                var right = Visit(context.simpleExpression(1));
                string op = context.GetChild(1).GetText();

                return BuildComparison(left, right, op);
            }
        }

        public override LLVMValueRef VisitSimpleExpression([NotNull] Oberon0Parser.SimpleExpressionContext context)
        {
            var result = Visit(context.term(0));

            // Handle unary +/-
            if (context.ChildCount > 0)
            {
                var firstChild = context.GetChild(0).GetText();
                if (firstChild == "-")
                {
                    result = BuildNegate(result);
                }
            }

            // Handle binary operations
            for (int i = 1; i < context.term().Length; i++)
            {
                int opIndex = i * 2 - 1;
                if (opIndex < context.ChildCount)
                {
                    string op = context.GetChild(opIndex).GetText();
                    var right = Visit(context.term(i));
                    result = BuildBinaryOp(result, right, op);
                }
            }

            return result;
        }

        public override LLVMValueRef VisitTerm([NotNull] Oberon0Parser.TermContext context)
        {
            var result = Visit(context.factor(0));

            for (int i = 1; i < context.factor().Length; i++)
            {
                int opIndex = i * 2 - 1;
                if (opIndex < context.ChildCount)
                {
                    string op = context.GetChild(opIndex).GetText();
                    var right = Visit(context.factor(i));
                    result = BuildBinaryOp(result, right, op);
                }
            }

            return result;
        }

        public override LLVMValueRef VisitFactor([NotNull] Oberon0Parser.FactorContext context)
        {
            // Literal
            if (context.literal() != null)
            {
                return Visit(context.literal());
            }

            // Parenthesized expression
            if (context.expression() != null)
            {
                return Visit(context.expression());
            }

            // NOT factor
            if (context.ChildCount > 0 && context.GetChild(0).GetText() == "NOT")
            {
                var value = Visit(context.factor());
                return BuildNot(value);
            }

            // Designator (variable or function call)
            if (context.designator() != null)
            {
                var designator = context.designator();
                string varName = designator.ID().GetText();

                // Function call
                if (context.expressionList() != null)
                {
                    return EvaluateFunctionCall(varName, context.expressionList());
                }

                // Variable access
                var variable = LookupVariable(varName);
                if (variable == null)
                {
                    throw new Exception($"Variable not found: {varName}");
                }

                // Check for array access with selectors
                var selectors = designator.selector();
                if (selectors != null && selectors.Length > 0)
                {
                    return LoadArrayElement(variable, selectors);
                }

                return builder.BuildLoad2(variable.Type, variable.Value, varName);
            }

            throw new Exception("Unknown factor type");
        }

        private LLVMValueRef LoadArrayElement(Variable variable, Oberon0Parser.SelectorContext[] selectors)
        {
            // Handle array element access: a[i, j] or a[i][j]
            var indices = new List<LLVMValueRef>();
            
            // First index is always 0 for GEP into the array
            indices.Add(LLVMValueRef.CreateConstInt(LLVMTypeRef.Int64, 0));
            
            foreach (var selector in selectors)
            {
                // Check if this is an array index selector [...]
                var exprList = selector.expressionList();
                if (exprList != null)
                {
                    foreach (var expr in exprList.expression())
                    {
                        var indexValue = Visit(expr);
                        // Ensure index is i64
                        if (indexValue.TypeOf != LLVMTypeRef.Int64)
                        {
                            indexValue = ConvertType(indexValue, LLVMTypeRef.Int64);
                        }
                        indices.Add(indexValue);
                    }
                }
            }
            
            // Build GEP to get pointer to element
            var elementPtr = builder.BuildInBoundsGEP2(variable.Type, variable.Value, indices.ToArray(), "arrayidx");
            
            // Determine element type by stripping array dimensions
            var elementType = GetArrayElementType(variable.Type, indices.Count - 1);
            
            // Load the element
            return builder.BuildLoad2(elementType, elementPtr, "elem");
        }

        private LLVMTypeRef GetArrayElementType(LLVMTypeRef arrayType, int numIndices)
        {
            var currentType = arrayType;
            for (int i = 0; i < numIndices; i++)
            {
                if (currentType.Kind == LLVMTypeKind.LLVMArrayTypeKind)
                {
                    currentType = currentType.ElementType;
                }
            }
            return currentType;
        }

        public override LLVMValueRef VisitLiteral([NotNull] Oberon0Parser.LiteralContext context)
        {
            if (context.INTEGER_LITERAL() != null)
            {
                long value = long.Parse(context.INTEGER_LITERAL().GetText(), System.Globalization.CultureInfo.InvariantCulture);
                return LLVMValueRef.CreateConstInt(LLVMTypeRef.Int64, (ulong)value, true);
            }
            else if (context.REAL_LITERAL() != null)
            {
                double value = double.Parse(context.REAL_LITERAL().GetText(), System.Globalization.CultureInfo.InvariantCulture);
                return LLVMValueRef.CreateConstReal(LLVMTypeRef.Double, value);
            }
            else if (context.STRING_LITERAL() != null)
            {
                string str = context.STRING_LITERAL().GetText();
                str = str.Substring(1, str.Length - 2); // Remove quotes
                return builder.BuildGlobalStringPtr(str, ".str");
            }
            else if (context.BOOLEAN_LITERAL() != null)
            {
                bool value = context.BOOLEAN_LITERAL().GetText() == "TRUE";
                return LLVMValueRef.CreateConstInt(LLVMTypeRef.Int1, value ? 1u : 0u);
            }
            else if (context.GetText() == "NIL")
            {
                return LLVMValueRef.CreateConstPointerNull(LLVMTypeRef.CreatePointer(LLVMTypeRef.Int8, 0));
            }

            throw new Exception("Unknown literal type");
        }

        private LLVMValueRef EvaluateFunctionCall(string funcName, Oberon0Parser.ExpressionListContext exprList)
        {
            if (!functions.TryGetValue(funcName, out var func))
            {
                throw new Exception($"Function not found: {funcName}");
            }

            var args = new List<LLVMValueRef>();
            if (exprList != null)
            {
                var expressions = exprList.expression();
                for (int i = 0; i < expressions.Length; i++)
                {
                    var expr = expressions[i];
                    var paramType = func.ParameterTypes[i];
                    
                    // Check if parameter expects a pointer (VAR parameter or array)
                    if (paramType.Kind == LLVMTypeKind.LLVMPointerTypeKind)
                    {
                        // Get the variable directly and pass its address
                        var sourceVar = GetSourceVariable(expr);
                        if (sourceVar != null)
                        {
                            args.Add(sourceVar.Value);
                            continue;
                        }
                    }
                    
                    args.Add(Visit(expr));
                }
            }

            return builder.BuildCall2(func.FunctionType, func.Value, args.ToArray());
        }

        #endregion

        #region Binary Operations

        private LLVMValueRef BuildBinaryOp(LLVMValueRef left, LLVMValueRef right, string op)
        {
            // Type promotion
            if (left.TypeOf != right.TypeOf)
            {
                if (left.TypeOf.Kind == LLVMTypeKind.LLVMDoubleTypeKind || right.TypeOf.Kind == LLVMTypeKind.LLVMDoubleTypeKind)
                {
                    left = ConvertType(left, LLVMTypeRef.Double);
                    right = ConvertType(right, LLVMTypeRef.Double);
                }
                else if (left.TypeOf.Kind == LLVMTypeKind.LLVMIntegerTypeKind && right.TypeOf.Kind == LLVMTypeKind.LLVMIntegerTypeKind)
                {
                    // Promote to the larger integer type (default to i64 for safety)
                    var targetType = LLVMTypeRef.Int64;
                    if (left.TypeOf.IntWidth > right.TypeOf.IntWidth)
                    {
                        targetType = left.TypeOf;
                    }
                    else if (right.TypeOf.IntWidth > left.TypeOf.IntWidth)
                    {
                        targetType = right.TypeOf;
                    }
                    left = ConvertType(left, targetType);
                    right = ConvertType(right, targetType);
                }
            }

            if (left.TypeOf.Kind == LLVMTypeKind.LLVMIntegerTypeKind)
            {
                return op switch
                {
                    "+" => builder.BuildAdd(left, right, "add"),
                    "-" => builder.BuildSub(left, right, "sub"),
                    "*" => builder.BuildMul(left, right, "mul"),
                    "/" or "DIV" => builder.BuildSDiv(left, right, "div"),
                    "MOD" => builder.BuildSRem(left, right, "rem"),
                    "AND" => builder.BuildAnd(left, right, "and"),
                    "OR" => builder.BuildOr(left, right, "or"),
                    _ => throw new Exception($"Unknown integer operation: {op}")
                };
            }
            else if (left.TypeOf.Kind == LLVMTypeKind.LLVMDoubleTypeKind)
            {
                return op switch
                {
                    "+" => builder.BuildFAdd(left, right, "fadd"),
                    "-" => builder.BuildFSub(left, right, "fsub"),
                    "*" => builder.BuildFMul(left, right, "fmul"),
                    "/" => builder.BuildFDiv(left, right, "fdiv"),
                    _ => throw new Exception($"Unknown real operation: {op}")
                };
            }

            throw new Exception("Unsupported type for binary operation");
        }

        private LLVMValueRef BuildComparison(LLVMValueRef left, LLVMValueRef right, string op)
        {
            if (left.TypeOf.Kind == LLVMTypeKind.LLVMIntegerTypeKind)
            {
                var predicate = op switch
                {
                    "=" => LLVMIntPredicate.LLVMIntEQ,
                    "#" => LLVMIntPredicate.LLVMIntNE,
                    "<" => LLVMIntPredicate.LLVMIntSLT,
                    "<=" => LLVMIntPredicate.LLVMIntSLE,
                    ">" => LLVMIntPredicate.LLVMIntSGT,
                    ">=" => LLVMIntPredicate.LLVMIntSGE,
                    _ => throw new Exception($"Unknown comparison: {op}")
                };
                return builder.BuildICmp(predicate, left, right, "cmp");
            }
            else if (left.TypeOf.Kind == LLVMTypeKind.LLVMDoubleTypeKind)
            {
                var predicate = op switch
                {
                    "=" => LLVMRealPredicate.LLVMRealOEQ,
                    "#" => LLVMRealPredicate.LLVMRealONE,
                    "<" => LLVMRealPredicate.LLVMRealOLT,
                    "<=" => LLVMRealPredicate.LLVMRealOLE,
                    ">" => LLVMRealPredicate.LLVMRealOGT,
                    ">=" => LLVMRealPredicate.LLVMRealOGE,
                    _ => throw new Exception($"Unknown comparison: {op}")
                };
                return builder.BuildFCmp(predicate, left, right, "fcmp");
            }

            throw new Exception("Unsupported type for comparison");
        }

        private LLVMValueRef BuildNegate(LLVMValueRef value)
        {
            if (value.TypeOf.Kind == LLVMTypeKind.LLVMIntegerTypeKind)
            {
                return builder.BuildNeg(value, "neg");
            }
            else if (value.TypeOf.Kind == LLVMTypeKind.LLVMDoubleTypeKind)
            {
                return builder.BuildFNeg(value, "fneg");
            }
            throw new Exception("Cannot negate this type");
        }

        private LLVMValueRef BuildNot(LLVMValueRef value)
        {
            value = ConvertToBool(value);
            return builder.BuildNot(value, "not");
        }

        #endregion

        #region Built-in I/O

        private void HandleWrite(Oberon0Parser.ProcedureCallContext context, bool writeln)
        {
            var printf = module.GetNamedFunction("printf");

            if (context.expressionList() == null || context.expressionList().expression().Length == 0)
            {
                if (writeln)
                {
                    var newlineStr = builder.BuildGlobalStringPtr("\n", ".nl");
                    builder.BuildCall2(printf.TypeOf, printf, new[] { newlineStr });
                }
                return;
            }

            var expr = context.expressionList().expression(0);
            var value = Visit(expr);

            string format = value.TypeOf.Kind switch
            {
                LLVMTypeKind.LLVMIntegerTypeKind => "%lld",
                LLVMTypeKind.LLVMDoubleTypeKind => "%lf",
                LLVMTypeKind.LLVMPointerTypeKind => "%s",
                _ => throw new Exception("Unsupported type for WRITE")
            };

            var fmtStr = builder.BuildGlobalStringPtr(format, ".fmt");
            builder.BuildCall2(printf.TypeOf, printf, new[] { fmtStr, value });

            if (writeln)
            {
                var newlineStr = builder.BuildGlobalStringPtr("\n", ".nl");
                builder.BuildCall2(printf.TypeOf, printf, new[] { newlineStr });
            }
        }

        private void HandleRead(Oberon0Parser.ProcedureCallContext context)
        {
            var scanf = module.GetNamedFunction("scanf");

            if (context.expressionList() == null || context.expressionList().expression().Length == 0)
            {
                throw new Exception("READ requires a variable argument");
            }

            var expr = context.expressionList().expression(0);
            var designator = expr.simpleExpression(0).term(0).factor(0).designator();

            if (designator == null)
            {
                throw new Exception("READ requires a variable");
            }

            string varName = designator.ID().GetText();
            var variable = LookupVariable(varName);

            if (variable == null)
            {
                throw new Exception($"Variable not found: {varName}");
            }

            string format = variable.Type.Kind switch
            {
                LLVMTypeKind.LLVMIntegerTypeKind => "%lld",
                LLVMTypeKind.LLVMDoubleTypeKind => "%lf",
                _ => throw new Exception("Unsupported type for READ")
            };

            var fmtStr = builder.BuildGlobalStringPtr(format, ".scan");
            builder.BuildCall2(scanf.TypeOf, scanf, new[] { fmtStr, variable.Value });
        }

        #endregion

        #region Type System

        private LLVMTypeRef GetLLVMType(Oberon0Parser.TypeContext context)
        {
            // Check for basic types first
            string typeText = context.GetText();
            
            // Handle basic types
            if (typeText == "INTEGER") return LLVMTypeRef.Int64;
            if (typeText == "REAL") return LLVMTypeRef.Double;
            if (typeText == "BOOLEAN") return LLVMTypeRef.Int1;
            if (typeText == "STRING") return LLVMTypeRef.CreatePointer(LLVMTypeRef.Int8, 0);
            
            // Check if it's an ARRAY type
            if (context.ChildCount > 0 && context.GetChild(0).GetText() == "ARRAY")
            {
                return CreateArrayType(context);
            }
            
            // Check if it's a user-defined type name (like IntMatrix)
            if (userTypes.TryGetValue(typeText, out var userType))
            {
                return userType;
            }
            
            // Check if type context is stored (for forward references)
            if (userTypeContexts.TryGetValue(typeText, out var storedContext))
            {
                var llvmType = GetLLVMType(storedContext);
                userTypes[typeText] = llvmType;
                return llvmType;
            }
            
            // Default fallback
            return LLVMTypeRef.CreatePointer(LLVMTypeRef.Int8, 0);
        }

        private LLVMTypeRef CreateArrayType(Oberon0Parser.TypeContext context)
        {
            // ARRAY expression (',' expression)* OF type
            // Get dimensions from expressions
            var dimensions = new List<uint>();
            var expressions = context.expression();
            
            foreach (var expr in expressions)
            {
                // Evaluate constant expression for array dimension
                uint dim = EvaluateConstantExpression(expr);
                dimensions.Add(dim);
            }
            
            // Get element type (the type after OF)
            var elementTypeContext = context.type();
            var elementType = GetLLVMType(elementTypeContext);
            
            // Build nested array type from innermost to outermost
            // ARRAY 2, 2 OF INTEGER -> [2 x [2 x i64]]
            var resultType = elementType;
            for (int i = dimensions.Count - 1; i >= 0; i--)
            {
                resultType = LLVMTypeRef.CreateArray(resultType, dimensions[i]);
            }
            
            return resultType;
        }

        private uint EvaluateConstantExpression(Oberon0Parser.ExpressionContext context)
        {
            // Simple constant evaluation for array dimensions
            var text = context.GetText();
            if (uint.TryParse(text, out uint value))
            {
                return value;
            }
            // Default to 1 if we can't evaluate
            return 1;
        }

        private LLVMValueRef ConvertType(LLVMValueRef value, LLVMTypeRef targetType)
        {
            if (value.TypeOf == targetType)
                return value;

            var fromKind = value.TypeOf.Kind;
            var toKind = targetType.Kind;

            if (fromKind == LLVMTypeKind.LLVMIntegerTypeKind && toKind == LLVMTypeKind.LLVMDoubleTypeKind)
            {
                return builder.BuildSIToFP(value, targetType, "sitofp");
            }
            else if (fromKind == LLVMTypeKind.LLVMDoubleTypeKind && toKind == LLVMTypeKind.LLVMIntegerTypeKind)
            {
                return builder.BuildFPToSI(value, targetType, "fptosi");
            }
            else if (fromKind == LLVMTypeKind.LLVMIntegerTypeKind && toKind == LLVMTypeKind.LLVMIntegerTypeKind)
            {
                if (value.TypeOf.IntWidth < targetType.IntWidth)
                    return builder.BuildSExt(value, targetType, "sext");  // Use sign extension for signed integers
                else if (value.TypeOf.IntWidth > targetType.IntWidth)
                    return builder.BuildTrunc(value, targetType, "trunc");
            }

            return value;
        }

        private LLVMValueRef ConvertToBool(LLVMValueRef value)
        {
            if (value.TypeOf == LLVMTypeRef.Int1)
                return value;

            if (value.TypeOf.Kind == LLVMTypeKind.LLVMIntegerTypeKind)
            {
                var zero = LLVMValueRef.CreateConstInt(value.TypeOf, 0);
                return builder.BuildICmp(LLVMIntPredicate.LLVMIntNE, value, zero, "tobool");
            }
            else if (value.TypeOf.Kind == LLVMTypeKind.LLVMDoubleTypeKind)
            {
                var zero = LLVMValueRef.CreateConstReal(value.TypeOf, 0.0);
                return builder.BuildFCmp(LLVMRealPredicate.LLVMRealONE, value, zero, "tobool");
            }

            return value;
        }

        #endregion

        #region Helper Methods

        private Variable? LookupVariable(string name)
        {
            for (int i = scopes.Count - 1; i >= 0; i--)
            {
                if (scopes.ElementAt(i).TryGetValue(name, out var variable))
                {
                    return variable;
                }
            }
            return null;
        }

        public void CreateMainFunction()
        {
            var mainType = LLVMTypeRef.CreateFunction(LLVMTypeRef.Int32, Array.Empty<LLVMTypeRef>());
            var main = module.AddFunction("main", mainType);
            var entry = main.AppendBasicBlock("entry");
            builder.PositionAtEnd(entry);

            // Set up main function context
            currentFunction = new Function
            {
                Name = "main",
                Value = main,
                FunctionType = mainType,
                ReturnType = LLVMTypeRef.Int32,
                Parameters = new List<string>()
            };

            // Create scope for main function with global variables
            scopes.Push(globalVars);

            // Execute the module's BEGIN block (if it exists)
            if (moduleBeginBlock != null)
            {
                Visit(moduleBeginBlock);
            }

            // Clean up scope
            scopes.Pop();
            currentFunction = null;

            // Return 0 from main
            builder.BuildRet(LLVMValueRef.CreateConstInt(LLVMTypeRef.Int32, 0));
        }

        public string GetIR()
        {
            return module.ToString();
        }

        public void WriteToFile(string filename)
        {
            module.WriteBitcodeToFile(filename.Replace(".ll", ".bc"));
            System.IO.File.WriteAllText(filename, GetIR());
        }

        public void Dispose()
        {
            builder.Dispose();
            module.Dispose();
        }

        #endregion
    }

    #region Supporting Classes - moved to SharedTypes.cs

    // Variable, Function, and LoopContext classes have been moved to SharedTypes.cs
    // to avoid duplication with LLVMCodeGenerator.cs

    #endregion
}
