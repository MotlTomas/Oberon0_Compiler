using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using Compiler.CodeGeneration;
using Compiler.Semantics;
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

class Program
{
    // Application entry using Visitor pattern
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

                // Run semantic analysis using VISITOR pattern
                var semanticVisitor = new SemanticVisitor();
                semanticVisitor.Visit(result.Tree);

                if (semanticVisitor.Errors.Any())
                {
                    Console.WriteLine("\n✗ Semantic errors found:");
                    foreach (var err in semanticVisitor.Errors)
                        Console.WriteLine($"  ERROR: {err}");
                    continue;
                }

                Console.WriteLine("✓ Semantic analysis successful");

                // Generate LLVM IR using VISITOR pattern
                Console.WriteLine("\n--- Generating LLVM IR ---");
                var moduleContext = ((Oberon0Parser.FileContext)result.Tree).module();
                string moduleName = moduleContext.ID(0).GetText();

                var codeVisitor = new LLVMCodeVisitor(moduleName);
                codeVisitor.Visit(result.Tree);
                codeVisitor.CreateMainFunction();

                // Save LLVM IR to file
                string llvmFile = Path.Combine(OutputFolder, $"{baseName}.ll");
                codeVisitor.WriteToFile(llvmFile);
                Console.WriteLine($"✓ LLVM IR written to: {llvmFile}");

                // Display generated IR
                Console.WriteLine("\n--- Generated LLVM IR (preview) ---");
                string ir = codeVisitor.GetIR();
                var irLines = ir.Split('\n').Take(30);
                foreach (var line in irLines)
                    Console.WriteLine(line);
                if (ir.Split('\n').Length > 30)
                    Console.WriteLine("... (truncated) ...");

                // Compile to executable using clang
                Console.WriteLine("\n--- Compiling to executable ---");
                string exeExtension = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? ".exe" : "";
                string exeFile = Path.Combine(OutputFolder, baseName + exeExtension);
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

    // Parses Oberon code using Visitor pattern
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
        var visitor = new Oberon0VariableVisitor(variableTracker);

        var tree = parser.file();
        
        // Use visitor pattern for tree traversal
        visitor.Visit(tree);

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

            // On Windows, we need to set up the library paths for x64 linking
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // Find Visual Studio and Windows SDK paths
                string vsPath = FindVisualStudioPath();
                string winSdkPath = FindWindowsSdkPath();
                
                if (!string.IsNullOrEmpty(vsPath) && !string.IsNullOrEmpty(winSdkPath))
                {
                    string libPath = $"{vsPath};{winSdkPath}\\ucrt\\x64;{winSdkPath}\\um\\x64";
                    process.StartInfo.EnvironmentVariables["LIB"] = libPath;
                }
            }

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

    // Find Visual Studio MSVC library path
    static string FindVisualStudioPath()
    {
        try
        {
            string basePath = @"C:\Program Files\Microsoft Visual Studio\2022";
            string[] editions = { "Community", "Professional", "Enterprise" };
            
            foreach (var edition in editions)
            {
                string vcPath = Path.Combine(basePath, edition, "VC", "Tools", "MSVC");
                if (Directory.Exists(vcPath))
                {
                    var versions = Directory.GetDirectories(vcPath);
                    if (versions.Length > 0)
                    {
                        // Get the latest version
                        var latestVersion = versions.OrderByDescending(v => v).First();
                        string libPath = Path.Combine(latestVersion, "lib", "x64");
                        if (Directory.Exists(libPath))
                        {
                            return libPath;
                        }
                    }
                }
            }
        }
        catch { }
        return "";
    }

    // Find Windows SDK library path
    static string FindWindowsSdkPath()
    {
        try
        {
            string basePath = @"C:\Program Files (x86)\Windows Kits\10\Lib";
            if (Directory.Exists(basePath))
            {
                var versions = Directory.GetDirectories(basePath);
                if (versions.Length > 0)
                {
                    // Get the latest version
                    var latestVersion = versions.OrderByDescending(v => v).First();
                    return latestVersion;
                }
            }
        }
        catch { }
        return "";
    }
}