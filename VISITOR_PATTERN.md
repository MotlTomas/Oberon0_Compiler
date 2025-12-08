# Visitor Pattern Implementation

This project uses the **ANTLR4 Visitor pattern** for abstract syntax tree traversal.

## What is Visitor Pattern?

The Visitor pattern allows you to:
- **Return values** from visit methods (unlike Listener pattern which uses void methods)
- **Control traversal explicitly** by calling `Visit()` when needed
- **Compose computations** by passing return values between visits

## Key Components

### 1. Semantic Analysis (`SemanticVisitor.cs`)
Extends `Oberon0BaseVisitor<object>` to perform:
- Symbol table building
- Type checking
- Scope management
- Error collection

### 2. Variable Tracking (`Oberon0VariableVisitor.cs`)
Extends `Oberon0BaseVisitor<object>` to collect:
- Variable declarations
- Constant declarations

### 3. Code Generation (`LLVMCodeVisitor.cs`)
Extends `Oberon0BaseVisitor<LLVMValueRef>` to:
- Generate LLVM IR instructions
- Return LLVM values for composition
- Build expressions recursively

## Visitor Pattern vs Listener Pattern

| Aspect | Visitor | Listener |
|--------|---------|----------|
| Return Values | ? Yes (generic type) | ? No (void) |
| Traversal Control | ? Explicit via `Visit()` | ? Automatic via walker |
| Use Case | Code generation, expressions | Side effects, data collection |
| Our Choice | ? **Used in this project** | ? Not used |

## Example Usage

### Visiting a Variable Declaration
```csharp
public override object VisitVarDecl(Oberon0Parser.VarDeclContext context)
{
    string typeName = context.type().GetText();
    foreach (var id in context.identList().ID())
    {
        // Process variable...
        scopes[^1].Add(new Symbol(id.GetText(), SymbolKind.Variable, typeName));
    }
    return null; // Visitor must return something
}
```

### Visiting an Expression (with return value)
```csharp
public override LLVMValueRef VisitExpression(Oberon0Parser.ExpressionContext context)
{
    // Visit children and get their computed values
    var left = Visit(context.simpleExpression(0));
    var right = Visit(context.simpleExpression(1));
    
    // Compose the results
    return BuildComparison(left, right, op);
}
```

### In Main Program
```csharp
// Create visitor
var semanticVisitor = new SemanticVisitor();

// Explicitly visit the tree
semanticVisitor.Visit(tree);

// Check for errors
if (semanticVisitor.Errors.Any()) { /* handle errors */ }
```

## Why Visitor for This Compiler?

1. **Expression Evaluation**: LLVM code generation requires returning values
2. **Type Safety**: Generic return type `LLVMValueRef` ensures type safety
3. **Composability**: Easy to compose LLVM instructions
4. **Control**: Precise control over when to visit children nodes

## File Structure

```
Semantics/
??? SemanticVisitor.cs          - Visitor<object> for semantic checks
??? Oberon0VariableVisitor.cs   - Visitor<object> for variable tracking

CodeGeneration/
??? LLVMCodeVisitor.cs          - Visitor<LLVMValueRef> for code generation
??? SharedTypes.cs              - Shared data structures

Program.cs                       - Uses visitors for compilation pipeline
```

## Tips

1. **Always return something**: Even if you don't use the return value, return `null` or `default`
2. **Explicit visits**: Call `Visit(node)` when you want to process children
3. **Control flow**: You decide when and how to visit children
4. **Return types**: Choose appropriate generic type for your visitor (`object`, `LLVMValueRef`, etc.)
