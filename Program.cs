using System;
using System.IO;
using System.Linq;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using Compiler.Semantics;
using Compiler.CodeGeneration;

class Program
{
    // Application entry: process all test Oberon files in the test directory
    static void Main(string[] args)
    {
        // Locate the folder with test code files
        string TestProgrammesFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "TestProgrammes");
        TestProgrammesFolder = Path.GetFullPath(TestProgrammesFolder);

        // Create output folder for compiled executables
        string OutputFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "Output");
        OutputFolder = Path.GetFullPath(OutputFolder);
        if (!Directory.Exists(OutputFolder))
            Directory.CreateDirectory(OutputFolder);

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
            string baseName = Path.GetFileNameWithoutExtension(filePath);

            Console.WriteLine($"\n{'=',70}");
            Console.WriteLine($"Processing file: {fileName}");
            Console.WriteLine($"{'=',70}\n");

            try
            {
                string code = File.ReadAllText(filePath);
                Console.WriteLine("--- Source Code ---\n" + code);

                var result = ParseCode(code);
                Console.WriteLine($"\n✓ Syntax valid: {result.IsValid}");

                if (!result.IsValid)
                {
                    Console.WriteLine("✗ Parse failed (syntax errors found)");
                    foreach (var err in result.Errors)
                        Console.WriteLine($"  ERROR: {err}");
                    continue;
                }

                Console.WriteLine($"✓ Variables found: {string.Join(", ", result.Variables)}");

                // Run semantic analysis on the parse tree
                var walker = new ParseTreeWalker();
                var semanticAnalyzer = new SemanticAnalyzer();
                walker.Walk(semanticAnalyzer, result.Tree);

                if (semanticAnalyzer.Errors.Any())
                {
                    Console.WriteLine("\n✗ Semantic errors found:");
                    foreach (var err in semanticAnalyzer.Errors)
                        Console.WriteLine($"  ERROR: {err}");
                    continue;
                }

                Console.WriteLine("✓ Semantic analysis successful");

                // Generate LLVM IR
                Console.WriteLine("\n--- Generating LLVM IR ---");
                var moduleContext = ((Oberon0Parser.FileContext)result.Tree).module();
                string moduleName = moduleContext.ID(0).GetText();

                var codeGenerator = new LLVMCodeGenerator(moduleName);
                walker.Walk(codeGenerator, result.Tree);
                codeGenerator.CreateMainFunction(moduleContext);

                // Save LLVM IR to file
                string llvmFile = Path.Combine(OutputFolder, $"{baseName}.ll");
                codeGenerator.WriteToFile(llvmFile);
                Console.WriteLine($"✓ LLVM IR written to: {llvmFile}");

                // Display generated IR
                Console.WriteLine("\n--- Generated LLVM IR (preview) ---");
                string ir = codeGenerator.GetIR();
                var irLines = ir.Split('\n').Take(30);
                foreach (var line in irLines)
                    Console.WriteLine(line);
                if (ir.Split('\n').Length > 30)
                    Console.WriteLine("... (truncated) ...");

                // Compile to executable using clang
                Console.WriteLine("\n--- Compiling to executable ---");
                string exeFile = Path.Combine(OutputFolder, baseName);
                if (CompileToExecutable(llvmFile, exeFile))
                {
                    Console.WriteLine($"✓ Executable created: {exeFile}");
                    Console.WriteLine($"\nRun with: {exeFile}");
                }
                else
                {
                    Console.WriteLine("✗ Compilation failed (is clang installed?)");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n✗ Error: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }

            Console.WriteLine($"\n{'-',70}\n");
        }

        Console.WriteLine("\n✓ All files processed!");
        Console.WriteLine($"\nCompiled outputs are in: {OutputFolder}");
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

    // Compile LLVM IR to native executable using clang
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
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                Console.WriteLine($"Clang error: {error}");
                return false;
            }

            return File.Exists(outputFile);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Compilation error: {ex.Message}");
            return false;
        }
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