public class Oberon0VariableListener : Oberon0BaseListener
{
    private readonly VariableTracker tracker;

    public Oberon0VariableListener(VariableTracker tracker)
    {
        this.tracker = tracker;
    }

    public override void EnterVarDecl(Oberon0Parser.VarDeclContext context)
    {
        var idList = context.identList();
        foreach (var idToken in idList.ID())
        {
            tracker.AddVariable(idToken.GetText());
        }
    }

    public override void EnterConstDecl(Oberon0Parser.ConstDeclContext context)
    {
        tracker.AddVariable(context.ID().GetText());
    }
}
