using Antlr4.Runtime.Misc;

namespace Compiler.Semantics
{
    /// <summary>
    /// Variable tracker using Visitor pattern
    /// Collects variable and constant names from declarations
    /// </summary>
    public class Oberon0VariableVisitor : Oberon0BaseVisitor<object>
    {
        private readonly VariableTracker tracker;

        public Oberon0VariableVisitor(VariableTracker tracker)
        {
            this.tracker = tracker;
        }

        public override object VisitVarDecl([NotNull] Oberon0Parser.VarDeclContext context)
        {
            var idList = context.identList();
            foreach (var idToken in idList.ID())
            {
                tracker.AddVariable(idToken.GetText());
            }
            return null;
        }

        public override object VisitConstDecl([NotNull] Oberon0Parser.ConstDeclContext context)
        {
            tracker.AddVariable(context.ID().GetText());
            return null;
        }
    }
}
