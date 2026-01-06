using System.Globalization;
using Antlr4.Runtime.Misc;
using LLVMSharp.Interop;

namespace Compiler.CodeGeneration
{
    public class LLVMCodeVisitor : Oberon0BaseVisitor<LLVMValueRef>
    {
        #region Nested Types

        // Represents a variable in the symbol table
        private class Variable
        {
            public string Name { get; set; } = "";
            public LLVMValueRef Value { get; set; }
            public LLVMTypeRef Type { get; set; }
            public bool IsGlobal { get; set; }
            public bool IsByRef { get; set; }
        }

        // Represents a procedure/function declaration
        private class Function
        {
            public string Name { get; set; } = "";
            public LLVMValueRef Value { get; set; }
            public LLVMTypeRef FunctionType { get; set; }
            public LLVMTypeRef ReturnType { get; set; }
            public List<string> Parameters { get; set; } = new();
            public List<LLVMTypeRef> ParameterTypes { get; set; } = new();
        }

        #endregion

        // LLVM module and IR builder
        private readonly LLVMModuleRef module;
        private readonly LLVMBuilderRef builder;

        // Symbol tables and type registry
        private readonly Stack<Dictionary<string, Variable>> scopes = new();
        private readonly Dictionary<string, Variable> globalVars = new();
        private readonly Dictionary<string, Function> functions = new();
        private readonly Dictionary<string, LLVMTypeRef> userTypes = new();

        // Function context tracking
        private Function? currentFunction;
        private readonly Stack<Function> functionStack = new();
        private Oberon0Parser.StatementSequenceContext? moduleBeginBlock;

        // Loop control flow targets for BREAK/CONTINUE
        private readonly Stack<LLVMBasicBlockRef> loopContinueBlocks = new();
        private readonly Stack<LLVMBasicBlockRef> loopBreakBlocks = new();

        public LLVMCodeVisitor(string moduleName)
        {
            module = LLVMModuleRef.CreateWithName(moduleName);
            builder = module.Context.CreateBuilder();
            module.Target = "x86_64-pc-windows-msvc";
            InitializeBuiltIns();
        }

        // Declare printf and scanf for I/O operations
        private void InitializeBuiltIns()
        {
            var int32 = LLVMTypeRef.Int32;
            var ptrInt8 = LLVMTypeRef.CreatePointer(LLVMTypeRef.Int8, 0);

            module.AddFunction("printf", LLVMTypeRef.CreateFunction(int32, new[] { ptrInt8 }, true));
            module.AddFunction("scanf", LLVMTypeRef.CreateFunction(int32, new[] { ptrInt8 }, true));
        }

        #region Module and Declarations

        // Entry point: process declarations and save BEGIN block for main
        public override LLVMValueRef VisitModule([NotNull] Oberon0Parser.ModuleContext context)
        {
            scopes.Push(globalVars);
            Visit(context.declarations());
            moduleBeginBlock = context.statementSequence();
            scopes.Pop();
            return default;
        }

        public override LLVMValueRef VisitDeclarations([NotNull] Oberon0Parser.DeclarationsContext context)
        {
            foreach (var typeDecl in context.typeDecl()) Visit(typeDecl);
            foreach (var varDecl in context.varDecl()) Visit(varDecl);
            foreach (var procDecl in context.procDecl()) Visit(procDecl);
            return default;
        }

        // Register user-defined type
        public override LLVMValueRef VisitTypeDecl([NotNull] Oberon0Parser.TypeDeclContext context)
        {
            userTypes[context.ID().GetText()] = GetLLVMType(context.type());
            return default;
        }

        // Allocate storage for declared variables
        public override LLVMValueRef VisitVarDecl([NotNull] Oberon0Parser.VarDeclContext context)
        {
            var llvmType = GetLLVMType(context.type());
            foreach (var id in context.identList().ID())
                DeclareVariable(id.GetText(), llvmType);
            return default;
        }

        // Create global or local variable storage
        private void DeclareVariable(string name, LLVMTypeRef type)
        {
            var value = currentFunction == null
                ? CreateGlobalVar(name, type)
                : builder.BuildAlloca(type, name);

            var scope = currentFunction == null ? globalVars : scopes.Peek();
            scope[name] = new Variable { Name = name, Value = value, Type = type, IsGlobal = currentFunction == null };
        }

        private LLVMValueRef CreateGlobalVar(string name, LLVMTypeRef type)
        {
            var global = module.AddGlobal(type, name);
            global.Initializer = LLVMValueRef.CreateConstNull(type);
            return global;
        }

        #endregion

        #region Procedures

        // Generate LLVM function from procedure declaration
        public override LLVMValueRef VisitProcDecl([NotNull] Oberon0Parser.ProcDeclContext context)
        {
            var heading = context.procHeading();
            var (funcType, paramInfo) = CreateFunctionSignature(heading);
            var func = module.AddFunction(heading.ID().GetText(), funcType);

            EnterFunction(new Function
            {
                Name = heading.ID().GetText(),
                Value = func,
                FunctionType = funcType,
                ReturnType = funcType.ReturnType,
                Parameters = paramInfo.names,
                ParameterTypes = paramInfo.types
            });

            var entry = func.AppendBasicBlock("entry");
            builder.PositionAtEnd(entry);

            var localScope = new Dictionary<string, Variable>();
            scopes.Push(localScope);

            for (int i = 0; i < paramInfo.names.Count; i++)
                AddParameter(func.GetParam((uint)i), paramInfo.names[i], paramInfo.types[i],
                           paramInfo.baseTypes[i], paramInfo.isByRef[i], localScope);

            Visit(context.procBody());
            EnsureReturn();

            scopes.Pop();
            ExitFunction();
            return default;
        }

        // Build function type from procedure heading (handles VAR parameters as pointers)
        private (LLVMTypeRef funcType, (List<string> names, List<LLVMTypeRef> types, List<LLVMTypeRef> baseTypes, List<bool> isByRef))
            CreateFunctionSignature(Oberon0Parser.ProcHeadingContext heading)
        {
            var returnType = heading.type() != null ? GetLLVMType(heading.type()) : LLVMTypeRef.Void;
            var names = new List<string>();
            var types = new List<LLVMTypeRef>();
            var baseTypes = new List<LLVMTypeRef>();
            var isByRef = new List<bool>();

            if (heading.formalParameters() != null)
            {
                foreach (var fp in heading.formalParameters().fpSection())
                {
                    var baseType = GetLLVMType(fp.type());
                    var byRef = fp.ChildCount > 0 && fp.GetChild(0).GetText() == "VAR";
                    var paramType = (byRef || baseType.Kind == LLVMTypeKind.LLVMArrayTypeKind)
                        ? LLVMTypeRef.CreatePointer(baseType, 0) : baseType;

                    foreach (var id in fp.identList().ID())
                    {
                        names.Add(id.GetText());
                        types.Add(paramType);
                        baseTypes.Add(baseType);
                        isByRef.Add(byRef || baseType.Kind == LLVMTypeKind.LLVMArrayTypeKind);
                    }
                }
            }

            return (LLVMTypeRef.CreateFunction(returnType, types.ToArray()), (names, types, baseTypes, isByRef));
        }

        // Add parameter to local scope (by-ref params use pointer directly)
        private void AddParameter(LLVMValueRef param, string name, LLVMTypeRef type,
            LLVMTypeRef baseType, bool byRef, Dictionary<string, Variable> scope)
        {
            if (byRef)
            {
                scope[name] = new Variable { Name = name, Value = param, Type = baseType, IsByRef = true };
            }
            else
            {
                var alloca = builder.BuildAlloca(type, $"{name}.addr");
                builder.BuildStore(param, alloca);
                scope[name] = new Variable { Name = name, Value = alloca, Type = type };
            }
        }

        private void EnterFunction(Function func)
        {
            if (currentFunction != null) functionStack.Push(currentFunction);
            currentFunction = func;
            functions[func.Name] = func;
        }

        private void ExitFunction()
        {
            currentFunction = functionStack.Count > 0 ? functionStack.Pop() : null;
        }

        // Add implicit return to blocks without terminator
        private void EnsureReturn()
        {
            if (currentFunction == null) return;

            var block = currentFunction.Value.FirstBasicBlock;
            while (block.Handle != IntPtr.Zero)
            {
                if (block.Terminator.Handle == IntPtr.Zero)
                {
                    builder.PositionAtEnd(block);
                    if (currentFunction.ReturnType.Kind == LLVMTypeKind.LLVMVoidTypeKind)
                        builder.BuildRetVoid();
                    else
                        builder.BuildRet(LLVMValueRef.CreateConstNull(currentFunction.ReturnType));
                }
                block = block.Next;
            }
        }

        public override LLVMValueRef VisitProcBody([NotNull] Oberon0Parser.ProcBodyContext context)
        {
            var savedBlock = builder.InsertBlock;
            if (context.declarations() != null) Visit(context.declarations());
            if (savedBlock.Handle != IntPtr.Zero) builder.PositionAtEnd(savedBlock);
            if (context.statementSequence() != null) Visit(context.statementSequence());
            return default;
        }

        #endregion

        #region Statements

        public override LLVMValueRef VisitStatementSequence([NotNull] Oberon0Parser.StatementSequenceContext context)
        {
            foreach (var stmt in context.statement()) Visit(stmt);
            return default;
        }

        // Generate store instruction for assignment
        public override LLVMValueRef VisitAssignment([NotNull] Oberon0Parser.AssignmentContext context)
        {
            var designator = context.designator();
            var variable = LookupVariable(designator.ID().GetText())
                ?? throw new Exception($"Variable not found: {designator.ID().GetText()}");

            var selectors = designator.selector();
            if (selectors?.Length > 0)
                return StoreArrayElement(variable, selectors, context.expression());

            var exprValue = Visit(context.expression());

            if (variable.Type.Kind == LLVMTypeKind.LLVMArrayTypeKind)
            {
                var sourceVar = TryGetSourceVariable(context.expression());
                if (sourceVar?.Type.Kind == LLVMTypeKind.LLVMArrayTypeKind)
                {
                    CopyArray(variable, sourceVar);
                    return default;
                }
            }

            exprValue = ConvertIfNeeded(exprValue, variable.Type);
            builder.BuildStore(exprValue, variable.Value);
            return default;
        }

        // Store value to array element using GEP
        private LLVMValueRef StoreArrayElement(Variable variable, Oberon0Parser.SelectorContext[] selectors,
            Oberon0Parser.ExpressionContext expr)
        {
            var indices = BuildArrayIndices(selectors);
            var elementPtr = builder.BuildInBoundsGEP2(variable.Type, variable.Value, indices.ToArray(), "arrayidx");
            var elementType = GetArrayElementType(variable.Type, indices.Count - 1);
            var value = ConvertIfNeeded(Visit(expr), elementType);

            builder.BuildStore(value, elementPtr);
            return default;
        }

        private List<LLVMValueRef> BuildArrayIndices(Oberon0Parser.SelectorContext[] selectors)
        {
            var indices = new List<LLVMValueRef> { LLVMValueRef.CreateConstInt(LLVMTypeRef.Int64, 0) };
            foreach (var selector in selectors)
            {
                if (selector.expressionList() != null)
                {
                    foreach (var indexExpr in selector.expressionList().expression())
                    {
                        var idx = ConvertIfNeeded(Visit(indexExpr), LLVMTypeRef.Int64);
                        indices.Add(idx);
                    }
                }
            }
            return indices;
        }

        // Copy array contents with memcpy
        private void CopyArray(Variable dest, Variable source)
        {
            var arraySize = GetTypeSize(dest.Type);
            var destPtr = builder.BuildBitCast(dest.Value, LLVMTypeRef.CreatePointer(LLVMTypeRef.Int8, 0), "dest.ptr");
            var srcPtr = builder.BuildBitCast(source.Value, LLVMTypeRef.CreatePointer(LLVMTypeRef.Int8, 0), "src.ptr");

            var memcpyType = LLVMTypeRef.CreateFunction(
                LLVMTypeRef.CreatePointer(LLVMTypeRef.Int8, 0),
                new[] { LLVMTypeRef.CreatePointer(LLVMTypeRef.Int8, 0),
                       LLVMTypeRef.CreatePointer(LLVMTypeRef.Int8, 0),
                       LLVMTypeRef.Int64 });

            var memcpy = module.GetNamedFunction("memcpy");
            if (memcpy.Handle == IntPtr.Zero)
                memcpy = module.AddFunction("memcpy", memcpyType);

            builder.BuildCall2(memcpyType, memcpy, new[] {
                destPtr, srcPtr, LLVMValueRef.CreateConstInt(LLVMTypeRef.Int64, arraySize)
            });
        }

        // Generate procedure call (handles built-in I/O separately)
        public override LLVMValueRef VisitProcedureCall([NotNull] Oberon0Parser.ProcedureCallContext context)
        {
            string procName = context.ID().GetText();

            if (procName == "WRITE" || procName == "WRITELN")
            {
                HandleWrite(context, procName == "WRITELN");
                return default;
            }
            if (procName == "READ")
            {
                HandleRead(context);
                return default;
            }

            var func = functions[procName];
            var args = BuildCallArguments(context.expressionList(), func);
            return builder.BuildCall2(func.FunctionType, func.Value, args.ToArray());
        }

        private List<LLVMValueRef> BuildCallArguments(Oberon0Parser.ExpressionListContext? exprList, Function func)
        {
            var args = new List<LLVMValueRef>();
            if (exprList == null) return args;

            var expressions = exprList.expression();
            for (int i = 0; i < expressions.Length; i++)
            {
                var expr = expressions[i];
                var paramType = func.ParameterTypes[i];

                if (paramType.Kind == LLVMTypeKind.LLVMPointerTypeKind)
                {
                    var sourceVar = TryGetSourceVariable(expr);
                    if (sourceVar != null)
                    {
                        args.Add(sourceVar.Value);
                        continue;
                    }
                }
                args.Add(Visit(expr));
            }
            return args;
        }

        // Generate IF/ELSIF/ELSE control flow with basic blocks
        public override LLVMValueRef VisitIfStatement([NotNull] Oberon0Parser.IfStatementContext context)
        {
            if (currentFunction == null) return default;

            var func = currentFunction.Value;
            var endBlock = func.AppendBasicBlock("if.end");

            var expressions = context.expression();
            var statementSequences = context.statementSequence();

            int numConditions = expressions.Length;
            bool hasElse = statementSequences.Length > numConditions;

            // Create blocks for each condition check and body
            var condBlocks = new List<LLVMBasicBlockRef>();
            var thenBlocks = new List<LLVMBasicBlockRef>();

            for (int i = 0; i < numConditions; i++)
            {
                if (i > 0)
                    condBlocks.Add(func.AppendBasicBlock($"elsif.cond.{i}"));
                thenBlocks.Add(func.AppendBasicBlock(i == 0 ? "if.then" : $"elsif.then.{i}"));
            }

            var elseBlock = hasElse ? func.AppendBasicBlock("if.else") : endBlock;

            // Build the IF condition and branch
            var firstCond = ConvertToBool(Visit(expressions[0]));
            var firstFalseTarget = numConditions > 1 ? condBlocks[0] : elseBlock;
            builder.BuildCondBr(firstCond, thenBlocks[0], firstFalseTarget);

            BuildBlockWithBranch(thenBlocks[0], () => Visit(statementSequences[0]), endBlock);

            for (int i = 1; i < numConditions; i++)
            {
                builder.PositionAtEnd(condBlocks[i - 1]);
                var cond = ConvertToBool(Visit(expressions[i]));
                var falseTarget = (i + 1 < numConditions) ? condBlocks[i] : elseBlock;
                builder.BuildCondBr(cond, thenBlocks[i], falseTarget);

                BuildBlockWithBranch(thenBlocks[i], () => Visit(statementSequences[i]), endBlock);
            }

            if (hasElse)
            {
                BuildBlockWithBranch(elseBlock, () => Visit(statementSequences[^1]), endBlock);
            }

            builder.PositionAtEnd(endBlock);
            return default;
        }

        // Dispatch to appropriate loop builder
        public override LLVMValueRef VisitLoopStatement([NotNull] Oberon0Parser.LoopStatementContext context)
        {
            if (currentFunction == null) return default;
            var keyword = context.GetChild(0).GetText();

            return keyword switch
            {
                "WHILE" => BuildWhileLoop(context),
                "REPEAT" => BuildRepeatLoop(context),
                "FOR" => BuildForLoop(context),
                _ => default
            };
        }

        // WHILE: condition block -> body -> back to condition
        private LLVMValueRef BuildWhileLoop(Oberon0Parser.LoopStatementContext context)
        {
            var func = currentFunction.Value;
            var condBlock = func.AppendBasicBlock("while.cond");
            var bodyBlock = func.AppendBasicBlock("while.body");
            var endBlock = func.AppendBasicBlock("while.end");

            loopContinueBlocks.Push(condBlock);
            loopBreakBlocks.Push(endBlock);

            builder.BuildBr(condBlock);
            builder.PositionAtEnd(condBlock);

            var cond = ConvertToBool(Visit(context.expression(0)));
            builder.BuildCondBr(cond, bodyBlock, endBlock);

            BuildBlockWithBranch(bodyBlock, () => Visit(context.statementSequence()), condBlock);

            loopContinueBlocks.Pop();
            loopBreakBlocks.Pop();

            builder.PositionAtEnd(endBlock);
            return default;
        }

        // REPEAT: body -> condition (exit when true)
        private LLVMValueRef BuildRepeatLoop(Oberon0Parser.LoopStatementContext context)
        {
            var func = currentFunction.Value;
            var bodyBlock = func.AppendBasicBlock("repeat.body");
            var condBlock = func.AppendBasicBlock("repeat.cond");
            var endBlock = func.AppendBasicBlock("repeat.end");

            loopContinueBlocks.Push(condBlock);
            loopBreakBlocks.Push(endBlock);

            builder.BuildBr(bodyBlock);
            BuildBlockWithBranch(bodyBlock, () => Visit(context.statementSequence()), condBlock);

            builder.PositionAtEnd(condBlock);
            var cond = ConvertToBool(Visit(context.expression(0)));
            builder.BuildCondBr(cond, endBlock, bodyBlock);

            loopContinueBlocks.Pop();
            loopBreakBlocks.Pop();

            builder.PositionAtEnd(endBlock);
            return default;
        }

        // FOR: init -> condition -> body -> increment -> condition
        private LLVMValueRef BuildForLoop(Oberon0Parser.LoopStatementContext context)
        {
            var loopVar = LookupVariable(context.ID().GetText())
                ?? throw new Exception($"Loop variable not found: {context.ID().GetText()}");

            var isDownTo = context.GetChild(4).GetText() == "DOWNTO";
            var startValue = Visit(context.expression(0));
            var endValue = Visit(context.expression(1));

            builder.BuildStore(startValue, loopVar.Value);

            var func = currentFunction.Value;
            var condBlock = func.AppendBasicBlock("for.cond");
            var bodyBlock = func.AppendBasicBlock("for.body");
            var incBlock = func.AppendBasicBlock("for.inc");
            var endBlock = func.AppendBasicBlock("for.end");

            loopContinueBlocks.Push(incBlock);
            loopBreakBlocks.Push(endBlock);

            builder.BuildBr(condBlock);
            builder.PositionAtEnd(condBlock);

            var currentValue = builder.BuildLoad2(loopVar.Type, loopVar.Value, loopVar.Name);
            var cmp = builder.BuildICmp(isDownTo ? LLVMIntPredicate.LLVMIntSGE : LLVMIntPredicate.LLVMIntSLE,
                                       currentValue, endValue, "for.cmp");
            builder.BuildCondBr(cmp, bodyBlock, endBlock);

            BuildBlockWithBranch(bodyBlock, () => Visit(context.statementSequence()), incBlock);

            builder.PositionAtEnd(incBlock);
            currentValue = builder.BuildLoad2(loopVar.Type, loopVar.Value, loopVar.Name);
            var step = LLVMValueRef.CreateConstInt(LLVMTypeRef.Int64, 1);
            var nextValue = isDownTo
                ? builder.BuildSub(currentValue, step, "for.dec")
                : builder.BuildAdd(currentValue, step, "for.inc");
            builder.BuildStore(nextValue, loopVar.Value);
            builder.BuildBr(condBlock);

            loopContinueBlocks.Pop();
            loopBreakBlocks.Pop();

            builder.PositionAtEnd(endBlock);
            return default;
        }

        private void BuildBlockWithBranch(LLVMBasicBlockRef block, Action buildContent, LLVMBasicBlockRef targetBlock)
        {
            builder.PositionAtEnd(block);
            buildContent();
            if (builder.InsertBlock.Terminator.Handle == IntPtr.Zero)
                builder.BuildBr(targetBlock);
        }

        // Generate CASE statement as chained comparisons
        public override LLVMValueRef VisitSwitchStatement([NotNull] Oberon0Parser.SwitchStatementContext context)
        {
            if (currentFunction == null) return default;

            var func = currentFunction.Value;
            var switchValue = Visit(context.expression());
            var caseBranches = context.caseBranch();

            var (caseCondBlocks, caseBlocks) = CreateCaseBlocks(func, caseBranches.Length);
            var elseBlock = func.AppendBasicBlock("case.else");
            var endBlock = func.AppendBasicBlock("case.end");

            builder.BuildBr(caseCondBlocks.Count > 0 ? caseCondBlocks[0] : elseBlock);

            for (int i = 0; i < caseBranches.Length; i++)
                BuildCaseBranch(caseBranches[i], switchValue, caseCondBlocks, caseBlocks, i, elseBlock, endBlock);

            BuildBlockWithBranch(elseBlock, () => {
                if (context.statementSequence() != null)
                    Visit(context.statementSequence());
            }, endBlock);

            builder.PositionAtEnd(endBlock);
            return default;
        }

        private (List<LLVMBasicBlockRef> condBlocks, List<LLVMBasicBlockRef> bodyBlocks)
            CreateCaseBlocks(LLVMValueRef func, int count)
        {
            var condBlocks = new List<LLVMBasicBlockRef>();
            var bodyBlocks = new List<LLVMBasicBlockRef>();

            for (int i = 0; i < count; i++)
            {
                condBlocks.Add(func.AppendBasicBlock($"case.cond.{i}"));
                bodyBlocks.Add(func.AppendBasicBlock($"case.body.{i}"));
            }
            return (condBlocks, bodyBlocks);
        }

        private void BuildCaseBranch(Oberon0Parser.CaseBranchContext branch, LLVMValueRef switchValue,
            List<LLVMBasicBlockRef> condBlocks, List<LLVMBasicBlockRef> bodyBlocks, int index,
            LLVMBasicBlockRef elseBlock, LLVMBasicBlockRef endBlock)
        {
            builder.PositionAtEnd(condBlocks[index]);

            var matchCondition = default(LLVMValueRef);
            foreach (var literal in branch.literal())
            {
                var literalValue = Visit(literal);
                var cmp = builder.BuildICmp(LLVMIntPredicate.LLVMIntEQ, switchValue, literalValue, $"case.cmp.{index}");
                matchCondition = matchCondition.Handle == IntPtr.Zero ? cmp : builder.BuildOr(matchCondition, cmp, "case.or");
            }

            var nextBlock = index + 1 < condBlocks.Count ? condBlocks[index + 1] : elseBlock;
            builder.BuildCondBr(matchCondition, bodyBlocks[index], nextBlock);

            BuildBlockWithBranch(bodyBlocks[index], () => Visit(branch.statementSequence()), endBlock);
        }

        public override LLVMValueRef VisitReturnStatement([NotNull] Oberon0Parser.ReturnStatementContext context)
        {
            if (context.expression() == null)
                return builder.BuildRetVoid();

            var value = Visit(context.expression());
            if (currentFunction != null)
                value = ConvertIfNeeded(value, currentFunction.ReturnType);

            return builder.BuildRet(value);
        }

        // Handle BREAK/CONTINUE
        public override LLVMValueRef VisitStatement([NotNull] Oberon0Parser.StatementContext context)
        {
            if (context.ChildCount == 1 && context.GetChild(0).GetText() == "CONTINUE")
            {
                if (loopContinueBlocks.Count == 0)
                    throw new Exception("CONTINUE statement outside of loop");

                builder.BuildBr(loopContinueBlocks.Peek());
                
                if (currentFunction != null)
                {
                    var unreachableBlock = currentFunction.Value.AppendBasicBlock("continue.unreachable");
                    builder.PositionAtEnd(unreachableBlock);
                }
                return default;
            }

            if (context.ChildCount == 1 && context.GetChild(0).GetText() == "BREAK")
            {
                if (loopBreakBlocks.Count == 0)
                    throw new Exception("BREAK statement outside of loop");

                builder.BuildBr(loopBreakBlocks.Peek());
                
                if (currentFunction != null)
                {
                    var unreachableBlock = currentFunction.Value.AppendBasicBlock("break.unreachable");
                    builder.PositionAtEnd(unreachableBlock);
                }
                return default;
            }

            return base.VisitStatement(context);
        }

        public override LLVMValueRef VisitIoStatement([NotNull] Oberon0Parser.IoStatementContext context)
        {
            var first = context.GetChild(0).GetText();
            var printf = module.GetNamedFunction("printf");

            if (first == "WRITE")
            {
                var val = Visit(context.expression());
                var fmt = GetFormatString(val.TypeOf, false);
                var fmtStr = builder.BuildGlobalStringPtr(fmt, ".fmt");
                builder.BuildCall2(printf.TypeOf, printf, new[] { fmtStr, val });
            }
            else if (first == "WRITELN")
            {
                var expr = context.expression();
                if (expr == null)
                {
                    var nl = builder.BuildGlobalStringPtr("\n", ".nl");
                    builder.BuildCall2(printf.TypeOf, printf, new[] { nl });
                }
                else
                {
                    var val = Visit(expr);
                    var fmt = GetFormatString(val.TypeOf, true);
                    var fmtStr = builder.BuildGlobalStringPtr(fmt, ".fmt");
                    builder.BuildCall2(printf.TypeOf, printf, new[] { fmtStr, val });
                }
            }
            else if (first == "READ")
            {
                var designator = context.designator();
                var variable = LookupVariable(designator.ID().GetText())
                    ?? throw new Exception($"Variable not found: {designator.ID().GetText()}");

                var scanf = module.GetNamedFunction("scanf");
                var fmt = variable.Type.Kind == LLVMTypeKind.LLVMDoubleTypeKind ? "%lf" : "%lld";
                var fmtStr = builder.BuildGlobalStringPtr(fmt, ".scan");
                builder.BuildCall2(scanf.TypeOf, scanf, new[] { fmtStr, variable.Value });
            }

            return default;
        }

        private string GetFormatString(LLVMTypeRef type, bool withNewline) =>
            (type.Kind switch
            {
                LLVMTypeKind.LLVMIntegerTypeKind => "%lld",
                LLVMTypeKind.LLVMDoubleTypeKind => "%lf",
                LLVMTypeKind.LLVMPointerTypeKind => "%s",
                _ => "%d"
            }) + (withNewline ? "\n" : "");

        #endregion

        #region Expressions

        // Evaluate expression; comparison returns i1 boolean
        public override LLVMValueRef VisitExpression([NotNull] Oberon0Parser.ExpressionContext context)
        {
            if (context.simpleExpression().Length == 1)
                return Visit(context.simpleExpression(0));

            var left = Visit(context.simpleExpression(0));
            var right = Visit(context.simpleExpression(1));
            return BuildComparison(left, right, context.GetChild(1).GetText());
        }

        // Evaluate additive expression (handles leading minus)
        public override LLVMValueRef VisitSimpleExpression([NotNull] Oberon0Parser.SimpleExpressionContext context)
        {
            var result = Visit(context.term(0));

            if (context.ChildCount > 0 && context.GetChild(0).GetText() == "-")
                result = BuildNegate(result);

            for (int i = 1; i < context.term().Length; i++)
            {
                var op = context.GetChild(i * 2 - 1).GetText();
                var right = Visit(context.term(i));
                result = BuildBinaryOp(result, right, op);
            }

            return result;
        }

        // Evaluate multiplicative expression
        public override LLVMValueRef VisitTerm([NotNull] Oberon0Parser.TermContext context)
        {
            var result = Visit(context.factor(0));
            for (int i = 1; i < context.factor().Length; i++)
            {
                var op = context.GetChild(i * 2 - 1).GetText();
                result = BuildBinaryOp(result, Visit(context.factor(i)), op);
            }
            return result;
        }

        // Evaluate factor: literal, variable, function call, or parenthesized expr
        public override LLVMValueRef VisitFactor([NotNull] Oberon0Parser.FactorContext context)
        {
            if (context.literal() != null)
                return Visit(context.literal());

            if (context.expression() != null)
                return Visit(context.expression());

            if (context.ChildCount > 0 && context.GetChild(0).GetText() == "NOT")
                return BuildNot(Visit(context.factor()));

            if (context.designator() != null)
            {
                var designator = context.designator();
                var varName = designator.ID().GetText();

                if (context.expressionList() != null)
                    return EvaluateFunctionCall(varName, context.expressionList());

                var variable = LookupVariable(varName)
                    ?? throw new Exception($"Variable not found: {varName}");

                var selectors = designator.selector();
                return selectors?.Length > 0
                    ? LoadArrayElement(variable, selectors)
                    : builder.BuildLoad2(variable.Type, variable.Value, varName);
            }

            throw new Exception("Unknown factor type");
        }

        private LLVMValueRef LoadArrayElement(Variable variable, Oberon0Parser.SelectorContext[] selectors)
        {
            var indices = BuildArrayIndices(selectors);
            var elementPtr = builder.BuildInBoundsGEP2(variable.Type, variable.Value, indices.ToArray(), "arrayidx");
            var elementType = GetArrayElementType(variable.Type, indices.Count - 1);
            return builder.BuildLoad2(elementType, elementPtr, "elem");
        }

        // Create LLVM constant from literal token
        public override LLVMValueRef VisitLiteral([NotNull] Oberon0Parser.LiteralContext context)
        {
            if (context.INTEGER_LITERAL() != null)
                return LLVMValueRef.CreateConstInt(LLVMTypeRef.Int64,
                    (ulong)long.Parse(context.INTEGER_LITERAL().GetText(), CultureInfo.InvariantCulture), true);

            if (context.REAL_LITERAL() != null)
                return LLVMValueRef.CreateConstReal(LLVMTypeRef.Double,
                    double.Parse(context.REAL_LITERAL().GetText(), CultureInfo.InvariantCulture));

            if (context.STRING_LITERAL() != null)
            {
                var str = context.STRING_LITERAL().GetText();
                return builder.BuildGlobalStringPtr(str[1..^1], ".str");
            }

            if (context.BOOLEAN_LITERAL() != null)
                return LLVMValueRef.CreateConstInt(LLVMTypeRef.Int1,
                    context.BOOLEAN_LITERAL().GetText() == "TRUE" ? 1u : 0u);

            if (context.GetText() == "NIL")
                return LLVMValueRef.CreateConstPointerNull(LLVMTypeRef.CreatePointer(LLVMTypeRef.Int8, 0));

            throw new Exception("Unknown literal type");
        }

        private LLVMValueRef EvaluateFunctionCall(string funcName, Oberon0Parser.ExpressionListContext exprList)
        {
            var func = functions[funcName];
            var args = BuildCallArguments(exprList, func);
            return builder.BuildCall2(func.FunctionType, func.Value, args.ToArray());
        }

        #endregion

        #region Binary Operations

        // Generate arithmetic/logical operation with type alignment
        private LLVMValueRef BuildBinaryOp(LLVMValueRef left, LLVMValueRef right, string op)
        {
            (left, right) = AlignTypes(left, right);

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

            if (left.TypeOf.Kind == LLVMTypeKind.LLVMDoubleTypeKind)
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

        // Generate comparison instruction (ICmp for int, FCmp for float)
        private LLVMValueRef BuildComparison(LLVMValueRef left, LLVMValueRef right, string op)
        {
            (left, right) = AlignTypes(left, right);

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

            if (left.TypeOf.Kind == LLVMTypeKind.LLVMDoubleTypeKind)
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

        // Promote operands to common type (int -> double if mixed)
        private (LLVMValueRef, LLVMValueRef) AlignTypes(LLVMValueRef left, LLVMValueRef right)
        {
            if (left.TypeOf == right.TypeOf) return (left, right);

            if (left.TypeOf.Kind == LLVMTypeKind.LLVMDoubleTypeKind || right.TypeOf.Kind == LLVMTypeKind.LLVMDoubleTypeKind)
                return (ConvertIfNeeded(left, LLVMTypeRef.Double), ConvertIfNeeded(right, LLVMTypeRef.Double));

            if (left.TypeOf.Kind == LLVMTypeKind.LLVMIntegerTypeKind && right.TypeOf.Kind == LLVMTypeKind.LLVMIntegerTypeKind)
            {
                var targetType = left.TypeOf.IntWidth > right.TypeOf.IntWidth ? left.TypeOf : right.TypeOf;
                return (ConvertIfNeeded(left, targetType), ConvertIfNeeded(right, targetType));
            }

            return (left, right);
        }

        private LLVMValueRef BuildNegate(LLVMValueRef value) =>
            value.TypeOf.Kind == LLVMTypeKind.LLVMIntegerTypeKind
                ? builder.BuildNeg(value, "neg")
                : builder.BuildFNeg(value, "fneg");

        private LLVMValueRef BuildNot(LLVMValueRef value) =>
            builder.BuildNot(ConvertToBool(value), "not");

        #endregion

        #region Type System

        // Map Oberon-0 type to LLVM type
        private LLVMTypeRef GetLLVMType(Oberon0Parser.TypeContext context)
        {
            var typeText = context.GetText();

            if (typeText == "INTEGER") return LLVMTypeRef.Int64;
            if (typeText == "REAL") return LLVMTypeRef.Double;
            if (typeText == "BOOLEAN") return LLVMTypeRef.Int1;
            if (typeText == "STRING") return LLVMTypeRef.CreatePointer(LLVMTypeRef.Int8, 0);

            if (context.ChildCount > 0 && context.GetChild(0).GetText() == "ARRAY")
                return CreateArrayType(context);

            return userTypes.TryGetValue(typeText, out var userType)
                ? userType
                : LLVMTypeRef.CreatePointer(LLVMTypeRef.Int8, 0);
        }

        // Build nested array type from dimensions
        private LLVMTypeRef CreateArrayType(Oberon0Parser.TypeContext context)
        {
            var dimensions = context.expression()
                .Select(expr => EvaluateConstantExpression(expr))
                .ToList();

            var elementType = GetLLVMType(context.type());
            return dimensions.Reverse<uint>()
                .Aggregate(elementType, (current, dim) => LLVMTypeRef.CreateArray(current, dim));
        }

        private uint EvaluateConstantExpression(Oberon0Parser.ExpressionContext context) =>
            uint.TryParse(context.GetText(), out uint value) ? value : 1;

        // Convert value to target type if necessary
        private LLVMValueRef ConvertIfNeeded(LLVMValueRef value, LLVMTypeRef targetType)
        {
            if (value.TypeOf == targetType) return value;
            return ConvertType(value, targetType);
        }

        private LLVMValueRef ConvertType(LLVMValueRef value, LLVMTypeRef targetType)
        {
            var (fromKind, toKind) = (value.TypeOf.Kind, targetType.Kind);

            if (fromKind == LLVMTypeKind.LLVMIntegerTypeKind && toKind == LLVMTypeKind.LLVMDoubleTypeKind)
                return builder.BuildSIToFP(value, targetType, "sitofp");

            if (fromKind == LLVMTypeKind.LLVMDoubleTypeKind && toKind == LLVMTypeKind.LLVMIntegerTypeKind)
                return builder.BuildFPToSI(value, targetType, "fptosi");

            if (fromKind == LLVMTypeKind.LLVMIntegerTypeKind && toKind == LLVMTypeKind.LLVMIntegerTypeKind)
            {
                if (value.TypeOf.IntWidth < targetType.IntWidth)
                    return builder.BuildSExt(value, targetType, "sext");
                if (value.TypeOf.IntWidth > targetType.IntWidth)
                    return builder.BuildTrunc(value, targetType, "trunc");
            }

            return value;
        }

        // Convert value to i1 boolean for conditionals
        private LLVMValueRef ConvertToBool(LLVMValueRef value)
        {
            if (value.TypeOf == LLVMTypeRef.Int1) return value;

            if (value.TypeOf.Kind == LLVMTypeKind.LLVMIntegerTypeKind)
            {
                var zero = LLVMValueRef.CreateConstInt(value.TypeOf, 0);
                return builder.BuildICmp(LLVMIntPredicate.LLVMIntNE, value, zero, "tobool");
            }

            if (value.TypeOf.Kind == LLVMTypeKind.LLVMDoubleTypeKind)
            {
                var zero = LLVMValueRef.CreateConstReal(value.TypeOf, 0.0);
                return builder.BuildFCmp(LLVMRealPredicate.LLVMRealONE, value, zero, "tobool");
            }

            return value;
        }

        private LLVMTypeRef GetArrayElementType(LLVMTypeRef arrayType, int numIndices)
        {
            var currentType = arrayType;
            for (int i = 0; i < numIndices; i++)
            {
                if (currentType.Kind == LLVMTypeKind.LLVMArrayTypeKind)
                    currentType = currentType.ElementType;
            }
            return currentType;
        }

        private ulong GetTypeSize(LLVMTypeRef type) =>
            type.Kind switch
            {
                LLVMTypeKind.LLVMIntegerTypeKind => type.IntWidth / 8,
                LLVMTypeKind.LLVMDoubleTypeKind => 8,
                LLVMTypeKind.LLVMArrayTypeKind => type.ArrayLength * GetTypeSize(type.ElementType),
                _ => 8
            };

        #endregion

        #region Helpers

        // Search scopes from inner to outer
        private Variable? LookupVariable(string name)
        {
            for (int i = scopes.Count - 1; i >= 0; i--)
            {
                if (scopes.ElementAt(i).TryGetValue(name, out var variable))
                    return variable;
            }
            return null;
        }

        // Extract variable from expression (for by-ref args and array copy)
        private Variable? TryGetSourceVariable(Oberon0Parser.ExpressionContext expr)
        {
            try
            {
                var designator = expr.simpleExpression(0).term(0).factor(0).designator();
                return designator != null ? LookupVariable(designator.ID().GetText()) : null;
            }
            catch { return null; }
        }

        // Generate printf call for WRITE/WRITELN
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

            var value = Visit(context.expressionList().expression(0));
            var format = GetFormatString(value.TypeOf, writeln);
            var fmtStr = builder.BuildGlobalStringPtr(format, ".fmt");
            builder.BuildCall2(printf.TypeOf, printf, new[] { fmtStr, value });
        }

        // Generate scanf call for READ
        private void HandleRead(Oberon0Parser.ProcedureCallContext context)
        {
            var scanf = module.GetNamedFunction("scanf");

            if (context.expressionList() == null || context.expressionList().expression().Length == 0)
                throw new Exception("READ requires a variable argument");

            var expr = context.expressionList().expression(0);
            var designator = expr.simpleExpression(0).term(0).factor(0).designator()
                ?? throw new Exception("READ requires a variable");

            var variable = LookupVariable(designator.ID().GetText())
                ?? throw new Exception($"Variable not found: {designator.ID().GetText()}");

            var format = variable.Type.Kind == LLVMTypeKind.LLVMDoubleTypeKind ? "%lf" : "%lld";
            var fmtStr = builder.BuildGlobalStringPtr(format, ".scan");
            builder.BuildCall2(scanf.TypeOf, scanf, new[] { fmtStr, variable.Value });
        }

        // Generate main() that executes the module's BEGIN block
        public void CreateMainFunction()
        {
            var mainType = LLVMTypeRef.CreateFunction(LLVMTypeRef.Int32, Array.Empty<LLVMTypeRef>());
            var main = module.AddFunction("main", mainType);
            var entry = main.AppendBasicBlock("entry");
            builder.PositionAtEnd(entry);

            currentFunction = new Function
            {
                Name = "main",
                Value = main,
                FunctionType = mainType,
                ReturnType = LLVMTypeRef.Int32,
                Parameters = new List<string>()
            };

            scopes.Push(globalVars);
            if (moduleBeginBlock != null) Visit(moduleBeginBlock);
            scopes.Pop();

            currentFunction = null;
            builder.BuildRet(LLVMValueRef.CreateConstInt(LLVMTypeRef.Int32, 0));
        }

        public string GetIR() => module.ToString();

        public void WriteToFile(string filename)
        {
            System.IO.File.WriteAllText(filename, GetIR());
        }

        public void Dispose()
        {
            builder.Dispose();
            module.Dispose();
        }

        #endregion
    }
}