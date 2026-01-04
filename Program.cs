using Antlr4.Runtime;
using Compiler.CodeGeneration;
using Compiler.Semantics;

class Program
{
    static void Main(string[] args)
    {
        string testFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "TestProgrammes");
        testFolder = Path.GetFullPath(testFolder);

        string outputFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "Output");
        outputFolder = Path.GetFullPath(outputFolder);
        Directory.CreateDirectory(outputFolder);

        if (!Directory.Exists(testFolder))
        {
            Console.WriteLine($"Error: '{testFolder}' not found!");
            return;
        }

        string[] testFiles = Directory.GetFiles(testFolder, "*.ob");
        if (testFiles.Length == 0)
        {
            Console.WriteLine($"No .ob files in '{testFolder}'");
            return;
        }

        Console.WriteLine($"Processing {testFiles.Length} file(s)...\n");

        foreach (var filePath in testFiles)
        {
            string fileName = Path.GetFileName(filePath);
            string baseName = Path.GetFileNameWithoutExtension(filePath);

            Console.WriteLine();
            Console.WriteLine("====================================");
            Console.WriteLine($"File: {fileName}");
            Console.WriteLine("====================================");

            try
            {
                string code = File.ReadAllText(filePath);
                var tree = ParseCode(code, out var errors, out var parser);

                if (errors.Count > 0)
                {
                    Console.WriteLine("Syntax errors:");
                    errors.ForEach(e => Console.WriteLine($"  {e}"));
                    continue;
                }

                Console.WriteLine("Syntax valid");

                // Display parse tree
                Console.WriteLine("\n--- Parse Tree ---");
                Console.WriteLine(tree.ToStringTree(parser));
                Console.WriteLine("------------------\n");

                var semanticAnalyzer = new SemanticAnalyzer();
                semanticAnalyzer.Visit(tree);

                if (semanticAnalyzer.Errors.Any())
                {
                    Console.WriteLine("Semantic errors:");
                    semanticAnalyzer.Errors.ForEach(e => Console.WriteLine($"  {e}"));
                    continue;
                }

                Console.WriteLine("Semantics valid");

                var moduleContext = ((Oberon0Parser.FileContext)tree).module();
                string moduleName = moduleContext.ID(0).GetText();

                var codeVisitor = new LLVMCodeVisitor(moduleName);
                codeVisitor.Visit(tree);
                codeVisitor.CreateMainFunction();

                string llvmFile = Path.Combine(outputFolder, $"{baseName}.ll");
                string exeFile = Path.Combine(outputFolder, baseName + GetExeExtension());
                if (CompileToExecutable(llvmFile, exeFile))
                    Console.WriteLine($"Executable created: {exeFile}");
                else
                    Console.WriteLine("Compilation failed (clang required)");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        Console.WriteLine($"\nDone! Output: {outputFolder}");
    }

    static Oberon0Parser.FileContext ParseCode(string input, out List<string> errors, out Oberon0Parser parser)
    {
        var lexer = new Oberon0Lexer(new AntlrInputStream(input));
        parser = new Oberon0Parser(new CommonTokenStream(lexer));

        var errorList = new List<string>();
        parser.RemoveErrorListeners();
        parser.AddErrorListener(new CustomErrorListener(errorList));

        errors = errorList;
        return parser.file();
    }

    static bool CompileToExecutable(string llvmFile, string outputFile)
    {
        try
        {
            var process = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "clang",
                    Arguments = $"\"{llvmFile}\" -o \"{outputFile}\" -Wno-override-module",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            process.WaitForExit();
            return process.ExitCode == 0 && File.Exists(outputFile);
        }
        catch
        {
            return false;
        }
    }

    static string GetExeExtension() =>
        System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(
            System.Runtime.InteropServices.OSPlatform.Windows) ? ".exe" : "";
}

class CustomErrorListener : BaseErrorListener
{
    private readonly List<string> errors;

    public CustomErrorListener(List<string> errors)
    {
        this.errors = errors;
    }

    public override void SyntaxError(TextWriter output, IRecognizer recognizer, IToken offendingSymbol,
                                     int line, int charPositionInLine, string msg, RecognitionException e)
    {
        errors.Add($"Line {line}:{charPositionInLine} - {msg}");
    }
}