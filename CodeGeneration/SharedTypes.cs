using LLVMSharp.Interop;
using System.Collections.Generic;

namespace Compiler.CodeGeneration
{
    /// <summary>
    /// Shared data structures for LLVM code generation
    /// Used by both Listener and Visitor implementations
    /// </summary>
    
    internal class Variable
    {
        public string Name { get; set; } = "";
        public LLVMValueRef Value { get; set; }
        public LLVMTypeRef Type { get; set; }
        public bool IsGlobal { get; set; }
        public bool IsByRef { get; set; }
    }

    internal class Function
    {
        public string Name { get; set; } = "";
        public LLVMValueRef Value { get; set; }
        public LLVMTypeRef FunctionType { get; set; }
        public LLVMTypeRef ReturnType { get; set; }
        public List<string> Parameters { get; set; } = new();
        public List<LLVMTypeRef> ParameterTypes { get; set; } = new();
    }

    internal class LoopContext
    {
        public LLVMBasicBlockRef CondBlock { get; set; }
        public LLVMBasicBlockRef EndBlock { get; set; }
    }
}
