using System.Collections.Generic;

public class VariableTracker
{
    private readonly HashSet<string> variables = new();

    public void AddVariable(string variable)
    {
        variables.Add(variable);
    }

    public List<string> GetVariables()
    {
        return new List<string>(variables);
    }
}
