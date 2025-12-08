using System.Collections.Generic;
using Antlr4.Runtime;

/// <summary>
/// Shared data structure for parse results
/// Used by both Listener and Visitor implementations
/// </summary>
internal class ParseResult
{
    public bool IsValid { get; set; }
    public List<string> Variables { get; set; }
    public string ParseTree { get; set; }
    public List<string> Errors { get; set; }
    public ParserRuleContext Tree { get; set; }
}
