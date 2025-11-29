using Antlr4.Runtime;
using System.Collections.Generic;
using System.IO;

public class ErrorListener : BaseErrorListener
{
    public List<string> Errors { get; } = new List<string>();
    public override void SyntaxError(TextWriter output, IRecognizer recognizer,
                                     IToken offendingSymbol, int line, int charPositionInLine,
                                     string msg, RecognitionException e)
    {
        string error = $"Line {line}:{charPositionInLine} - {msg}";
        Errors.Add(error);
        System.Console.WriteLine($"Parse error: {error}");
    }
}
