using System;
using System.IO;
using System.Linq;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using Compiler.Semantics;

class Program
{
    // Application entry: process all test Oberon files in the test directory
    static void Main(string[] args)
    {
        // Locate the folder with test code files
        string TestProgrammesFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "TestProgrammes");
        TestProgrammesFolder = Path.GetFullPath(TestProgrammesFolder);

        // Create the folder if it doesn't exist
        if (!Directory.Exists(TestProgrammesFolder))
        {
            Console.WriteLine($"Error: Folder '{TestProgrammesFolder}' not found!");
            Directory.CreateDirectory(TestProgrammesFolder);
            Console.WriteLine("Please add test files and run again.");
            return;
        }

        // Get test files (*.ob); fallback: all files if none found
        string[] testFiles = Directory.GetFiles(TestProgrammesFolder, "*.ob").ToArray();
        if (testFiles.Length == 0)
            testFiles = Directory.GetFiles(TestProgrammesFolder);
        if (testFiles.Length == 0)
        {
            Console.WriteLine($"No test files found in '{TestProgrammesFolder}' folder!");
            return;
        }

        Console.WriteLine($"Found {testFiles.Length} test file(s)\n");

        // Parse and analyze all test files in the directory
        foreach (var filePath in testFiles)
        {
            string fileName = Path.GetFileName(filePath);
            Console.WriteLine($"Testing file: {fileName}");
            try
            {
                string code = File.ReadAllText(filePath);
                Console.WriteLine("\n--- Test code ---\n" + code);

                var result = ParseCode(code);
                Console.WriteLine($"\nSyntax valid: {result.IsValid}");

                if (result.IsValid)
                    Console.WriteLine("Parse successful");
                else
                    Console.WriteLine("Parse failed (syntax errors found)");

                Console.WriteLine($"Variables: {string.Join(", ", result.Variables)}");
                Console.WriteLine($"\nParse tree:\n{result.ParseTree}");

                // Run semantic analysis on the parse tree
                var walker = new ParseTreeWalker();
                var semanticAnalyzer = new SemanticAnalyzer();
                walker.Walk(semanticAnalyzer, result.Tree);

                if (semanticAnalyzer.Errors.Any())
                {
                    Console.WriteLine("Semantic errors found:");
                    foreach (var err in semanticAnalyzer.Errors)
                        Console.WriteLine("  " + err);
                }
                else
                {
                    Console.WriteLine("Semantic analysis successful.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nError reading or parsing file: {ex.Message}");
            }

            Console.WriteLine($"\n{'-',60}\n");
        }

        Console.WriteLine("\nAll tests completed!");
    }

    // Parses Oberon code, tracks variables, builds a parse tree
    static ParseResult ParseCode(string input)
    {
        var inputStream = new AntlrInputStream(input);
        var lexer = new Oberon0Lexer(inputStream);
        var tokenStream = new CommonTokenStream(lexer);
        var parser = new Oberon0Parser(tokenStream);

        parser.RemoveErrorListeners();
        var errorListener = new ErrorListener();
        parser.AddErrorListener(errorListener);

        var variableTracker = new VariableTracker();
        var listener = new Oberon0VariableListener(variableTracker);

        var tree = parser.file();
        var walker = new ParseTreeWalker();
        walker.Walk(listener, tree);

        return new ParseResult
        {
            IsValid = parser.NumberOfSyntaxErrors == 0,
            Variables = variableTracker.GetVariables(),
            ParseTree = tree.ToStringTree(parser),
            Errors = errorListener.Errors,
            Tree = tree
        };
    }
}

// Structure for holding parse results and analysis artifacts
class ParseResult
{
    public bool IsValid { get; set; }
    public System.Collections.Generic.List<string> Variables { get; set; }
    public string ParseTree { get; set; }
    public System.Collections.Generic.List<string> Errors { get; set; }
    public ParserRuleContext Tree { get; set; } // Used for semantic analysis
}
