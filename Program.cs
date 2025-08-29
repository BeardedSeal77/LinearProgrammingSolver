using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using LinearProgrammingSolver.Utils;
using LinearProgrammingSolver.Tables;
using LinearProgrammingSolver.LPAlgorithms;
using LinearProgrammingSolver.IPAlgorithms;
using LinearProgrammingSolver.NLPAlgorithms;

namespace LinearProgrammingSolver
{
    // Enum for main menu options
    public enum MainMenuOption
    {
        LoadFile = 1,
        SelectAlgorithm = 2,
        SensitivityAnalysis = 3,
        ViewResults = 4,
        Exit = 5
    }

    // Enum for algorithm selection
    public enum AlgorithmOption
    {
        PrimalSimplex = 1,
        RevisedPrimalSimplex = 2,
        BranchBoundSimplex = 3,
        BranchBoundKnapsack = 4,
        CuttingPlane = 5,
        NonLinearProgramming = 6,
        BackToMain = 7
    }

    // Enum for sensitivity analysis options
    public enum SensitivityOption
    {
        NonBasicVariableRange = 1,
        NonBasicVariableChange = 2,
        BasicVariableRange = 3,
        BasicVariableChange = 4,
        VariableInNonBasicRange = 5,
        VariableInNonBasicChange = 6,
        ConstraintRHSRange = 7,
        ConstraintRHSChange = 8,
        AddNewActivity = 9,
        AddNewConstraint = 10,
        ShowShadowPrices = 11,
        DualityAnalysis = 12,
        BackToMain = 13
    }

    // Enum for file loading options
    public enum FileLoadOption
    {
        LoadDefault = 1,
        ChooseFile = 2,
        EnterPath = 3,
        BackToMain = 4
    }

    class Program
    {
        private static FileReader fileReader = new FileReader();
        private static CanonicalFormConverter canonicalConverter = new CanonicalFormConverter();
        private static Table currentRawTable = null;
        private static Table currentOptimalTable = null;
        private static string currentInputPath = "";
        private static ProblemType currentProblemType = ProblemType.LinearProgramming;

        static void Main(string[] args)
        {
            Console.WriteLine("██╗     ██╗███╗   ██╗███████╗ █████╗ ██████╗ ");
            Console.WriteLine("██║     ██║████╗  ██║██╔════╝██╔══██╗██╔══██╗");
            Console.WriteLine("██║     ██║██╔██╗ ██║█████╗  ███████║██████╔╝");
            Console.WriteLine("██║     ██║██║╚██╗██║██╔══╝  ██╔══██║██╔══██╗");
            Console.WriteLine("███████╗██║██║ ╚████║███████╗██║  ██║██║  ██║");
            Console.WriteLine("╚══════╝╚═╝╚═╝  ╚═══╝╚══════╝╚═╝  ╚═╝╚═╝  ╚═╝");
            Console.WriteLine();
            Console.WriteLine("██████╗ ██████╗  ██████╗  ██████╗ ██████╗  █████╗ ███╗   ███╗███╗   ███╗██╗███╗   ██╗ ██████╗ ");
            Console.WriteLine("██╔══██╗██╔══██╗██╔═══██╗██╔════╝ ██╔══██╗██╔══██╗████╗ ████║████╗ ████║██║████╗  ██║██╔════╝ ");
            Console.WriteLine("██████╔╝██████╔╝██║   ██║██║  ███╗██████╔╝███████║██╔████╔██║██╔████╔██║██║██╔██╗ ██║██║  ███╗");
            Console.WriteLine("██╔═══╝ ██╔══██╗██║   ██║██║   ██║██╔══██╗██╔══██║██║╚██╔╝██║██║╚██╔╝██║██║██║╚██╗██║██║   ██║");
            Console.WriteLine("██║     ██║  ██║╚██████╔╝╚██████╔╝██║  ██║██║  ██║██║ ╚═╝ ██║██║ ╚═╝ ██║██║██║ ╚████║╚██████╔╝");
            Console.WriteLine("╚═╝     ╚═╝  ╚═╝ ╚═════╝  ╚═════╝ ╚═╝  ╚═╝╚═╝  ╚═╝╚═╝     ╚═╝╚═╝     ╚═╝╚═╝╚═╝  ╚═══╝ ╚═════╝ ");
            Console.WriteLine();
            Console.WriteLine("███████╗ ██████╗ ██╗    ██╗   ██╗███████╗██████╗ ");
            Console.WriteLine("██╔════╝██╔═══██╗██║    ██║   ██║██╔════╝██╔══██╗");
            Console.WriteLine("███████╗██║   ██║██║    ██║   ██║█████╗  ██████╔╝");
            Console.WriteLine("╚════██║██║   ██║██║    ╚██╗ ██╔╝██╔══╝  ██╔══██╗");
            Console.WriteLine("███████║╚██████╔╝███████╗╚████╔╝ ███████╗██║  ██║");
            Console.WriteLine("╚══════╝ ╚═════╝ ╚══════╝ ╚═══╝  ╚══════╝╚═╝  ╚═╝");
            Console.WriteLine();
            Console.WriteLine("═══════════════════════════════════════════════════════════════════════════════");
            Console.WriteLine("        LPR 381 Project - Menu-Driven LP/IP/NLP Solver");
            Console.WriteLine("═══════════════════════════════════════════════════════════════════════════════");
            Console.WriteLine();
            
            // Main application loop
            RunMainMenu();
        }

        static void RunMainMenu()
        {
            bool exitRequested = false;

            while (!exitRequested)
            {
                try
                {
                    DisplayMainMenu();
                    
                    if (int.TryParse(Console.ReadLine(), out int choice))
                    {
                        var selectedOption = (MainMenuOption)choice;
                        
                        switch (selectedOption)
                        {
                            case MainMenuOption.LoadFile:
                                HandleLoadFile();
                                break;
                            case MainMenuOption.SelectAlgorithm:
                                HandleAlgorithmSelection();
                                break;
                            case MainMenuOption.SensitivityAnalysis:
                                HandleSensitivityAnalysis();
                                break;
                            case MainMenuOption.ViewResults:
                                HandleViewResults();
                                break;
                            case MainMenuOption.Exit:
                                exitRequested = true;
                                break;
                            default:
                                Console.WriteLine("Invalid option. Please try again.");
                                break;
                        }
                    }
                    else
                    {
                        Console.WriteLine("Invalid input. Please enter a number.");
                    }
                    
                    if (!exitRequested)
                    {
                        Console.WriteLine("\nPress any key to continue...");
                        try 
                        {
                            Console.ReadKey();
                        }
                        catch
                        {
                            Console.ReadLine(); // Fallback for redirected input
                        }
                        Console.Clear();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    Console.WriteLine("\nPress any key to continue...");
                    try 
                    {
                        Console.ReadKey();
                    }
                    catch
                    {
                        Console.ReadLine(); // Fallback for redirected input
                    }
                    Console.Clear();
                }
            }
            
            Console.WriteLine("\nThank you for using Linear Programming Solver!");
            Console.WriteLine("Program terminated.");
        }

        static void DisplayMainMenu()
        {
            Console.WriteLine("╔══════════════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                              MAIN MENU                                       ║");
            Console.WriteLine("╠══════════════════════════════════════════════════════════════════════════════╣");
            Console.WriteLine("║                                                                              ║");
            Console.WriteLine("║  1. Load Input File          - Load LP/IP or NLP model from text file        ║");
            Console.WriteLine("║  2. Select Algorithm          - Choose solving algorithm                     ║");
            Console.WriteLine("║  3. Sensitivity Analysis      - Perform post-solution analysis               ║");
            Console.WriteLine("║  4. View Results              - Display solution and tables                  ║");
            Console.WriteLine("║  5. Exit                      - Close the program                            ║");
            Console.WriteLine("║                                                                              ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════════════════════╝");
            Console.WriteLine();
            
            // Display current file status
            if (!string.IsNullOrEmpty(currentInputPath))
            {
                Console.WriteLine($"Current file: {Path.GetFileName(currentInputPath)}");
            }
            else
            {
                Console.WriteLine("No file loaded");
            }
            
            Console.WriteLine();
            Console.Write("Select an option (1-5): ");
        }

        static void HandleLoadFile()
        {
            Console.Clear();
            
            bool backToMain = false;
            
            while (!backToMain)
            {
                DisplayFileLoadMenu();
                
                if (int.TryParse(Console.ReadLine(), out int choice))
                {
                    if (choice >= 1 && choice <= 4)
                    {
                        var selectedOption = (FileLoadOption)choice;
                        
                        switch (selectedOption)
                        {
                            case FileLoadOption.LoadDefault:
                                LoadDefaultFile();
                                backToMain = true;
                                break;
                            case FileLoadOption.ChooseFile:
                                ChooseFileFromExplorer();
                                backToMain = true;
                                break;
                            case FileLoadOption.EnterPath:
                                EnterFilePathManually();
                                backToMain = true;
                                break;
                            case FileLoadOption.BackToMain:
                                backToMain = true;
                                break;
                        }
                    }
                    else
                    {
                        Console.WriteLine("Invalid option. Please try again.");
                    }
                }
                else
                {
                    Console.WriteLine("Invalid input. Please enter a number.");
                }
                
                if (!backToMain)
                {
                    Console.WriteLine("\nPress any key to continue...");
                    Console.ReadKey();
                    Console.Clear();
                }
            }
        }

        static void DisplayFileLoadMenu()
        {
            Console.WriteLine("╔══════════════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                            LOAD INPUT FILE                                   ║");
            Console.WriteLine("╠══════════════════════════════════════════════════════════════════════════════╣");
            Console.WriteLine("║                                                                              ║");
            Console.WriteLine("║  1. Load Default File        - Load data/input.txt (LP/IP)                   ║");
            Console.WriteLine("║  2. Choose File              - Browse with file explorer (LP/IP/NLP)         ║");
            Console.WriteLine("║  3. Enter File Path          - Type file path manually (LP/IP/NLP)           ║");
            Console.WriteLine("║  4. Back to Main Menu        - Return to main menu                           ║");
            Console.WriteLine("║                                                                              ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════════════════════╝");
            Console.WriteLine();
            Console.Write("Select an option (1-4): ");
        }

        static void LoadDefaultFile()
        {
            Console.Clear();
            Console.WriteLine("Loading default file (data/input.txt)...\n");
            
            try
            {
                string inputPath = FindDefaultInputFile();
                ProcessInputFile(inputPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading default file: {ex.Message}");
            }
        }

        static void ChooseFileFromExplorer()
        {
            Console.Clear();
            Console.WriteLine("Attempting to open file explorer dialog...\n");
            
            try
            {
                // Initialize Windows Forms application for console app
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                
                // Create and configure the file dialog
                using (var openFileDialog = new OpenFileDialog())
                {
                    openFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
                    openFileDialog.Title = "Select Input File - Linear Programming Solver";
                    openFileDialog.InitialDirectory = Path.Combine(Directory.GetCurrentDirectory(), "data");
                    openFileDialog.RestoreDirectory = true;
                    openFileDialog.CheckFileExists = true;
                    openFileDialog.CheckPathExists = true;
                    openFileDialog.Multiselect = false;
                    
                    Console.WriteLine("File dialog launching...");
                    Console.WriteLine("If you don't see the dialog window:");
                    Console.WriteLine("- Check behind other windows or on other monitors");
                    Console.WriteLine("- Press Alt+Tab to cycle through open windows");
                    Console.WriteLine("- Look for the dialog in your taskbar");
                    Console.WriteLine();
                    Console.WriteLine("Press any key to cancel and return to the file menu if the dialog doesn't appear...");
                    Console.WriteLine();
                    
                    // Start the dialog in a separate task
                    var dialogTask = Task.Run(() => openFileDialog.ShowDialog());
                    
                    // Wait for either the dialog to complete or user to press a key
                    while (!dialogTask.IsCompleted)
                    {
                        if (Console.KeyAvailable)
                        {
                            Console.ReadKey(true); // Consume the key press
                            Console.WriteLine("Dialog cancelled by user. Returning to file menu...");
                            return;
                        }
                        Thread.Sleep(100); // Check every 100ms
                    }
                    
                    DialogResult result = dialogTask.Result;
                    
                    if (result == DialogResult.OK)
                    {
                        string selectedFile = openFileDialog.FileName;
                        Console.WriteLine($"File selected: {selectedFile}\n");
                        ProcessInputFile(selectedFile);
                    }
                    else
                    {
                        Console.WriteLine("File selection cancelled.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error with file dialog: {ex.Message}");
                Console.WriteLine("\nThe file explorer dialog could not be opened.");
                Console.WriteLine("This might be due to:");
                Console.WriteLine("- Running in a restricted environment");
                Console.WriteLine("- Missing Windows Forms components");
                Console.WriteLine("- System permission issues");
                Console.WriteLine();
                Console.WriteLine("Please use option 3 'Enter File Path' to manually specify your file location.");
            }
        }

        static void EnterFilePathManually()
        {
            Console.Clear();
            Console.WriteLine("Enter file path manually:\n");
            Console.Write("Please enter the full path to your input file: ");
            
            string userPath = Console.ReadLine();
            
            if (!string.IsNullOrWhiteSpace(userPath))
            {
                try
                {
                    ProcessInputFile(userPath);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error loading file: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine("No file path entered. Operation cancelled.");
            }
        }

        static string FindDefaultInputFile()
        {
            // Use robust path resolution that works from any working directory
            string currentDir = Directory.GetCurrentDirectory();
            string projectDir = currentDir;
            
            // If running from debugger, find the project directory
            while (!File.Exists(Path.Combine(projectDir, "data", "input.txt")) && 
                   Directory.GetParent(projectDir) != null)
            {
                projectDir = Directory.GetParent(projectDir).FullName;
            }
            
            string inputPath = Path.Combine(projectDir, "data", "input.txt");
            
            if (!File.Exists(inputPath))
            {
                throw new FileNotFoundException($"Default input file not found at: {inputPath}");
            }
            
            return inputPath;
        }

        static void ProcessInputFile(string inputPath)
        {
            if (!File.Exists(inputPath))
            {
                throw new FileNotFoundException($"File not found: {inputPath}");
            }
            
            // Clear any existing tables from previous runs
            TableCache.ClearAllTables();
            
            currentInputPath = inputPath;
            Console.WriteLine($"Processing: {inputPath}");
            Console.WriteLine();
            
            // Step 1: FileReader detects file type and parses accordingly
            var (problemType, data) = fileReader.ParseFile(inputPath);
            currentProblemType = problemType;  // Store current problem type
            
            if (problemType == ProblemType.LinearProgramming)
            {
                // Handle LP/IP problems
                var (matrix, rowLabels, columnLabels, optimizationType, constraintOperators) = 
                    ((double[,], List<string>, List<string>, OptimizationType, Dictionary<string, ConstraintOperator>))data;
                
                // Step 2: Program.cs constructs Table object
                currentRawTable = new Table("t-raw", matrix, rowLabels, columnLabels, optimizationType, "Raw", constraintOperators);
                
                // Step 3: Program.cs stores Table in cache
                TableCache.StoreTable(currentRawTable);
                Console.WriteLine("✓ Raw table created and cached");
                
                // Step 4: Convert to canonical form
                var canonicalTable = canonicalConverter.ConvertToCanonicalForm(currentRawTable);
                TableCache.StoreTable(canonicalTable);
                Console.WriteLine("✓ Canonical table created and cached");
                
                Console.WriteLine();
                Console.WriteLine("Linear Programming file loaded successfully!");
                Console.WriteLine($"Problem type: {optimizationType}");
                Console.WriteLine($"Variables: {currentRawTable.GetVariableCount()}");
                Console.WriteLine($"Constraints: {currentRawTable.GetRowCount() - 1}");
            }
            else if (problemType == ProblemType.NonLinearProgramming)
            {
                // Handle NLP problems
                var nlpProblem = (NLPProblem)data;
                
                Console.WriteLine("✓ NLP problem parsed");
                Console.WriteLine();
                Console.WriteLine("Non-Linear Programming file loaded successfully!");
                Console.WriteLine($"Function: {nlpProblem.Function}");
                Console.WriteLine($"Starting point: ({nlpProblem.StartingPoint.x}, {nlpProblem.StartingPoint.y})");
                
                // Store NLP problem for algorithm selection
                // TODO: Add NLP storage mechanism similar to Table cache
            }
        }

        static void HandleAlgorithmSelection()
        {
            Console.Clear();
            
            if (string.IsNullOrEmpty(currentInputPath))
            {
                Console.WriteLine("Error: No file loaded. Please load a file first.");
                return;
            }
            
            bool backToMain = false;
            
            while (!backToMain)
            {
                DisplayAlgorithmMenu();
                
                if (int.TryParse(Console.ReadLine(), out int choice))
                {
                    if (choice >= 1 && choice <= 7)
                    {
                        var selectedOption = (AlgorithmOption)choice;
                        
                        switch (selectedOption)
                        {
                            case AlgorithmOption.PrimalSimplex:
                                ExecutePrimalSimplex();
                                backToMain = true;
                                break;
                            case AlgorithmOption.RevisedPrimalSimplex:
                                if (currentProblemType != ProblemType.LinearProgramming)
                                {
                                    Console.WriteLine("Error: Revised Primal Simplex requires a Linear Programming problem.");
                                    Console.WriteLine("Please load an LP/IP file or select the NLP algorithm instead.");
                                }
                                else
                                {
                                    Console.WriteLine("Revised Primal Simplex Algorithm - Coming Soon!");
                                }
                                break;
                            case AlgorithmOption.BranchBoundSimplex:
                                ExecuteBranchAndBound();
                                backToMain = true;
                                break;
                            case AlgorithmOption.BranchBoundKnapsack:
                                if (currentProblemType != ProblemType.LinearProgramming)
                                {
                                    Console.WriteLine("Error: Branch & Bound Knapsack requires a Linear Programming problem.");
                                    Console.WriteLine("Please load an LP/IP file or select the NLP algorithm instead.");
                                }
                                else
                                {
                                    Console.WriteLine("Branch & Bound Knapsack Algorithm - Coming Soon!");
                                }
                                break;
                            case AlgorithmOption.CuttingPlane:
                                if (currentProblemType != ProblemType.LinearProgramming)
                                {
                                    Console.WriteLine("Error: Cutting Plane algorithm requires a Linear Programming problem.");
                                    Console.WriteLine("Please load an LP/IP file or select the NLP algorithm instead.");
                                }
                                else
                                {
                                    Console.WriteLine("Cutting Plane Algorithm - Coming Soon!");
                                }
                                break;
                            case AlgorithmOption.NonLinearProgramming:
                                ExecuteNonLinearProgramming();
                                break;
                            case AlgorithmOption.BackToMain:
                                backToMain = true;
                                break;
                        }
                    }
                    else
                    {
                        Console.WriteLine("Invalid option. Please try again.");
                    }
                }
                else
                {
                    Console.WriteLine("Invalid input. Please enter a number.");
                }
                
                if (!backToMain)
                {
                    Console.WriteLine("\nPress any key to continue...");
                    Console.ReadKey();
                    Console.Clear();
                }
            }
        }

        static void DisplayAlgorithmMenu()
        {
            Console.WriteLine("╔══════════════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                         ALGORITHM SELECTION                                  ║");
            Console.WriteLine("╠══════════════════════════════════════════════════════════════════════════════╣");
            Console.WriteLine("║                                                                              ║");
            Console.WriteLine("║  1. Primal Simplex             - Standard simplex algorithm                  ║");
            Console.WriteLine("║  2. Revised Primal Simplex     - Matrix-based simplex method                 ║");
            Console.WriteLine("║  3. Branch & Bound Simplex     - Integer programming via simplex             ║");
            Console.WriteLine("║  4. Branch & Bound Knapsack    - Specialized knapsack algorithm              ║");
            Console.WriteLine("║  5. Cutting Plane Algorithm    - Integer programming via cutting planes      ║");
            Console.WriteLine("║  6. Non-Linear Programming     - Analytical NLP optimization (+10 bonus)     ║");
            Console.WriteLine("║  7. Back to Main Menu          - Return to main menu                         ║");
            Console.WriteLine("║                                                                              ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════════════════════╝");
            Console.WriteLine();
            Console.Write("Select an algorithm (1-7): ");
        }

        static void ExecuteBranchAndBound()
        {
            Console.Clear();
            Console.WriteLine("╔══════════════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                      EXECUTING BRANCH & BOUND SIMPLEX                        ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════════════════════╝");
            Console.WriteLine();
            
            // Validate problem type
            if (currentProblemType != ProblemType.LinearProgramming)
            {
                Console.WriteLine("Error: Branch & Bound algorithm requires a Linear Programming problem.");
                Console.WriteLine("The currently loaded file contains a Non-Linear Programming problem.");
                Console.WriteLine("Please load an LP/IP file (format: max/min ...) or select the NLP algorithm instead.");
                return;
            }
            
            try
            {
                // Check if canonical table exists
                var canonical = TableCache.GetTable("t-i");
                if (canonical == null)
                {
                    Console.WriteLine("Error: No canonical table found. Please load a file first.");
                    return;
                }
                
                // Check if optimal table exists
                var optimalTable = TableCache.GetTable("t-optimal");
                
                if (optimalTable == null)
                {
                    Console.WriteLine("No optimal LP solution found in cache. Running Primal Simplex first...");
                    Console.WriteLine();
                    
                    // Automatically run Primal Simplex
                    var primalSimplex = new PrimalSimplexAlgorithm();
                    optimalTable = primalSimplex.SolveLP(canonical);
                    
                    if (optimalTable == null || !optimalTable.IsOptimal())
                    {
                        Console.WriteLine($"Error: LP relaxation could not be solved optimally. Status: {optimalTable?.Status ?? "null"}");
                        return;
                    }
                    
                    Console.WriteLine($"✓ LP relaxation solved with objective value: {optimalTable.GetObjectiveValue():F3}");
                    Console.WriteLine();
                }
                else
                {
                    Console.WriteLine($"Found existing optimal LP solution with objective value: {optimalTable.GetObjectiveValue():F3}");
                    Console.WriteLine();
                }
                
                // Start Branch & Bound
                Console.WriteLine("Starting Branch & Bound Integer Programming...");
                Console.WriteLine();
                
                var branchAndBound = new BranchAndBoundAlgorithm();
                var integerSolution = branchAndBound.SolveIP(optimalTable);
                
                // Display processing results
                Console.WriteLine("=== BRANCH & BOUND PROCESSING LOG ===" );
                var processingOrder = branchAndBound.GetProcessingOrder();
                foreach (var logEntry in processingOrder)
                {
                    Console.WriteLine(logEntry);
                }
                
                // Display fathoming reasons
                Console.WriteLine("\n=== FATHOMING REASONS ===");
                var fathomReasons = branchAndBound.GetFathomReasons();
                foreach (var kvp in fathomReasons)
                {
                    Console.WriteLine($"{kvp.Key}: {kvp.Value}");
                }
                
                // Display best integer solution
                Console.WriteLine("\n=== BEST INTEGER SOLUTION ===");
                if (integerSolution != null)
                {
                    Console.WriteLine($"✓ Integer solution found!");
                    Console.WriteLine($"Table ID: {integerSolution.TableId}");
                    Console.WriteLine($"Objective Value: {integerSolution.GetObjectiveValue():F3}");
                    Console.WriteLine("Basic variables and values:");
                    for (int i = 0; i < integerSolution.BasicVariables.Count; i++)
                    {
                        var varName = integerSolution.BasicVariables[i];
                        var value = integerSolution.GetElement(i + 1, integerSolution.GetColumnCount() - 1);
                        Console.WriteLine($"  {varName} = {value:F3}");
                    }
                    
                    currentOptimalTable = integerSolution; // Update for sensitivity analysis
                }
                else
                {
                    Console.WriteLine("No integer solution found!");
                }
                
                // Display summary
                var allSubproblems = branchAndBound.GetAllSubproblems();
                Console.WriteLine($"\n=== SUMMARY ===");
                Console.WriteLine($"Total subproblems generated: {allSubproblems.Count}");
                Console.WriteLine($"Processing steps: {processingOrder.Count}");
                Console.WriteLine($"Fathomed nodes: {fathomReasons.Count}");
                
                // Export to output.txt (project requirement)
                ExportBranchAndBoundResults(branchAndBound);
                
                Console.WriteLine();
                Console.WriteLine("✓ Branch & Bound results exported to data/output.txt");
                
                // Display table summary
                Console.WriteLine();
                TableCache.DisplayTableSummary();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error executing Branch & Bound: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }

        static void ExecutePrimalSimplex()
        {
            Console.Clear();
            Console.WriteLine("╔══════════════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                        EXECUTING PRIMAL SIMPLEX                              ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════════════════════╝");
            Console.WriteLine();
            
            // Validate problem type
            if (currentProblemType != ProblemType.LinearProgramming)
            {
                Console.WriteLine("Error: Primal Simplex algorithm requires a Linear Programming problem.");
                Console.WriteLine("The currently loaded file contains a Non-Linear Programming problem.");
                Console.WriteLine("Please load an LP/IP file (format: max/min ...) or select the NLP algorithm instead.");
                return;
            }
            
            try
            {
                var simplexSolver = new PrimalSimplexAlgorithm();
                var initialTable = TableCache.GetTable("t-i");
                
                if (initialTable == null)
                {
                    Console.WriteLine("Error: Canonical table (t-i) not found in TableCache.");
                    return;
                }
                
                Console.WriteLine("Starting Primal Simplex Algorithm...");
                Console.WriteLine();
                
                currentOptimalTable = simplexSolver.SolveLP(initialTable);
                
                Console.WriteLine("✓ Primal Simplex Algorithm completed successfully!");
                Console.WriteLine();
                
                // Display basic results
                Console.WriteLine($"Final Status: {currentOptimalTable.Status}");
                
                if (currentOptimalTable.Status == "Optimal")
                {
                    Console.WriteLine("Optimal solution found!");
                }
                else if (currentOptimalTable.Status == "Infeasible")
                {
                    Console.WriteLine("Problem is infeasible - no solution exists.");
                }
                else if (currentOptimalTable.Status == "Unbounded")
                {
                    Console.WriteLine("Problem is unbounded - objective can be improved indefinitely.");
                }
                
                // Export to output.txt (project requirement)
                ExportResults();
                
                Console.WriteLine();
                Console.WriteLine("Solution exported to data/output.txt");
                
                // Display table summary
                Console.WriteLine();
                TableCache.DisplayTableSummary();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error executing Primal Simplex: {ex.Message}");
            }
        }

        static void HandleSensitivityAnalysis()
        {
            Console.Clear();
            
            if (currentOptimalTable == null)
            {
                Console.WriteLine("Error: No optimal solution available. Please solve the problem first using an algorithm.");
                return;
            }
            
            if (currentOptimalTable.Status != "Optimal")
            {
                Console.WriteLine($"Error: Sensitivity analysis requires an optimal solution. Current status: {currentOptimalTable.Status}");
                return;
            }
            
            bool backToMain = false;
            
            while (!backToMain)
            {
                DisplaySensitivityMenu();
                
                if (int.TryParse(Console.ReadLine(), out int choice))
                {
                    if (choice >= 1 && choice <= 13)
                    {
                        var selectedOption = (SensitivityOption)choice;
                        
                        switch (selectedOption)
                        {
                            case SensitivityOption.NonBasicVariableRange:
                                Console.WriteLine("Non-Basic Variable Range Analysis - Coming Soon!");
                                break;
                            case SensitivityOption.NonBasicVariableChange:
                                Console.WriteLine("Non-Basic Variable Change Analysis - Coming Soon!");
                                break;
                            case SensitivityOption.BasicVariableRange:
                                Console.WriteLine("Basic Variable Range Analysis - Coming Soon!");
                                break;
                            case SensitivityOption.BasicVariableChange:
                                Console.WriteLine("Basic Variable Change Analysis - Coming Soon!");
                                break;
                            case SensitivityOption.VariableInNonBasicRange:
                                Console.WriteLine("Variable in Non-Basic Column Range Analysis - Coming Soon!");
                                break;
                            case SensitivityOption.VariableInNonBasicChange:
                                Console.WriteLine("Variable in Non-Basic Column Change Analysis - Coming Soon!");
                                break;
                            case SensitivityOption.ConstraintRHSRange:
                                Console.WriteLine("Constraint RHS Range Analysis - Coming Soon!");
                                break;
                            case SensitivityOption.ConstraintRHSChange:
                                Console.WriteLine("Constraint RHS Change Analysis - Coming Soon!");
                                break;
                            case SensitivityOption.AddNewActivity:
                                Console.WriteLine("Add New Activity - Coming Soon!");
                                break;
                            case SensitivityOption.AddNewConstraint:
                                Console.WriteLine("Add New Constraint - Coming Soon!");
                                break;
                            case SensitivityOption.ShowShadowPrices:
                                Console.WriteLine("Shadow Prices Analysis - Coming Soon!");
                                break;
                            case SensitivityOption.DualityAnalysis:
                                Console.WriteLine("Duality Analysis - Coming Soon!");
                                break;
                            case SensitivityOption.BackToMain:
                                backToMain = true;
                                break;
                        }
                    }
                    else
                    {
                        Console.WriteLine("Invalid option. Please try again.");
                    }
                }
                else
                {
                    Console.WriteLine("Invalid input. Please enter a number.");
                }
                
                if (!backToMain)
                {
                    Console.WriteLine("\nPress any key to continue...");
                    Console.ReadKey();
                    Console.Clear();
                }
            }
        }

        static void DisplaySensitivityMenu()
        {
            Console.WriteLine("╔══════════════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                       SENSITIVITY ANALYSIS                                   ║");
            Console.WriteLine("╠══════════════════════════════════════════════════════════════════════════════╣");
            Console.WriteLine("║                                                                              ║");
            Console.WriteLine("║  Variable Analysis:                                                          ║");
            Console.WriteLine("║    1. Non-Basic Variable Range     - Display range analysis                  ║");
            Console.WriteLine("║    2. Non-Basic Variable Change    - Apply and display changes               ║");
            Console.WriteLine("║    3. Basic Variable Range         - Display range analysis                  ║");
            Console.WriteLine("║    4. Basic Variable Change        - Apply and display changes               ║");
            Console.WriteLine("║    5. Variable in Non-Basic Range  - Column range analysis                   ║");
            Console.WriteLine("║    6. Variable in Non-Basic Change - Column change analysis                  ║");
            Console.WriteLine("║                                                                              ║");
            Console.WriteLine("║  Constraint Analysis:                                                        ║");
            Console.WriteLine("║    7. Constraint RHS Range         - Right-hand-side range analysis          ║");
            Console.WriteLine("║    8. Constraint RHS Change        - Right-hand-side change analysis         ║");
            Console.WriteLine("║                                                                              ║");
            Console.WriteLine("║  Solution Modifications:                                                     ║");
            Console.WriteLine("║    9. Add New Activity             - Add new variable to solution            ║");
            Console.WriteLine("║   10. Add New Constraint          - Add new constraint to solution           ║");
            Console.WriteLine("║   11. Show Shadow Prices          - Display shadow price analysis            ║");
            Console.WriteLine("║                                                                              ║");
            Console.WriteLine("║  Duality Analysis:                                                           ║");
            Console.WriteLine("║   12. Duality Analysis            - Dual model analysis and verification     ║");
            Console.WriteLine("║                                                                              ║");
            Console.WriteLine("║   13. Back to Main Menu           - Return to main menu                      ║");
            Console.WriteLine("║                                                                              ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════════════════════╝");
            Console.WriteLine();
            Console.Write("Select an analysis option (1-13): ");
        }

        static void HandleViewResults()
        {
            Console.Clear();
            Console.WriteLine("╔══════════════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                            VIEW RESULTS                                      ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════════════════════╝");
            Console.WriteLine();
            
            if (TableCache.GetTableCount() == 0)
            {
                Console.WriteLine("No results available. Please load a file and run an algorithm first.");
                return;
            }
            
            Console.WriteLine("1. Table Summary");
            Console.WriteLine("2. Detailed Tables View");
            Console.WriteLine("3. Final Solution Only");
            Console.WriteLine();
            Console.Write("Select view option (1-3): ");
            
            if (int.TryParse(Console.ReadLine(), out int choice))
            {
                Console.WriteLine();
                
                switch (choice)
                {
                    case 1:
                        TableCache.DisplayTableSummary();
                        break;
                    case 2:
                        TableCache.DisplayAllTablesDetailed();
                        break;
                    case 3:
                        if (currentOptimalTable != null)
                        {
                            Console.WriteLine("Final Solution:");
                            Console.WriteLine(new string('=', 50));
                            currentOptimalTable.DisplayTraditional();
                        }
                        else
                        {
                            Console.WriteLine("No optimal solution available.");
                        }
                        break;
                    default:
                        Console.WriteLine("Invalid option.");
                        break;
                }
            }
            else
            {
                Console.WriteLine("Invalid input.");
            }
        }

        static void ExportBranchAndBoundResults(BranchAndBoundAlgorithm branchAndBound)
        {
            try
            {
                // Ensure data directory exists
                string dataDir = "data";
                if (!Directory.Exists(dataDir))
                {
                    Directory.CreateDirectory(dataDir);
                }
                
                string outputPath = Path.Combine(dataDir, "output.txt");
                using (StreamWriter writer = new StreamWriter(outputPath))
                {
                    // Write canonical form
                    var canonicalTable = TableCache.GetTable("t-i");
                    if (canonicalTable != null)
                    {
                        writer.WriteLine("=== CANONICAL FORM ===");
                        writer.WriteLine(canonicalTable.ToString());
                        writer.WriteLine();
                    }
                    
                    // Write LP optimal solution
                    var lpOptimal = TableCache.GetTable("t-optimal");
                    if (lpOptimal != null)
                    {
                        writer.WriteLine("=== LP RELAXATION OPTIMAL SOLUTION ===");
                        writer.WriteLine($"Table ID: {lpOptimal.TableId}");
                        writer.WriteLine($"Objective Value: {lpOptimal.GetObjectiveValue():F3}");
                        writer.WriteLine(lpOptimal.ToString());
                        writer.WriteLine();
                    }
                    
                    // Write processing order
                    writer.WriteLine("=== BRANCH & BOUND PROCESSING LOG ===");
                    var processingOrder = branchAndBound.GetProcessingOrder();
                    foreach (var logEntry in processingOrder)
                    {
                        writer.WriteLine(logEntry);
                    }
                    writer.WriteLine();
                    
                    // Write all subproblem tables
                    writer.WriteLine("=== ALL SUBPROBLEM TABLES ===");
                    foreach (var table in TableCache.GetAllTables().Where(t => 
                        t.TableId.Contains("-A") || t.TableId.Contains("-B") || 
                        t.Status == "Iteration" || t.Status == "Optimal" || 
                        t.Status == "Infeasible" || t.Status.StartsWith("Fathomed")))
                    {
                        writer.WriteLine($"Table {table.TableId} ({table.Status}):");
                        writer.WriteLine($"Objective Value: {table.GetObjectiveValue():F3}");
                        writer.WriteLine(table.ToString());
                        writer.WriteLine();
                    }
                    
                    // Write fathoming reasons
                    writer.WriteLine("=== FATHOMING REASONS ===");
                    var fathomReasons = branchAndBound.GetFathomReasons();
                    foreach (var kvp in fathomReasons)
                    {
                        writer.WriteLine($"{kvp.Key}: {kvp.Value}");
                    }
                    writer.WriteLine();
                    
                    // Write best integer solution
                    var bestSolution = branchAndBound.GetBestIntegerSolution();
                    writer.WriteLine("=== BEST INTEGER SOLUTION ===");
                    if (bestSolution != null)
                    {
                        writer.WriteLine($"Table ID: {bestSolution.TableId}");
                        writer.WriteLine($"Objective Value: {bestSolution.GetObjectiveValue():F3}");
                        writer.WriteLine("Basic variables and values:");
                        for (int i = 0; i < bestSolution.BasicVariables.Count; i++)
                        {
                            var varName = bestSolution.BasicVariables[i];
                            var value = bestSolution.GetElement(i + 1, bestSolution.GetColumnCount() - 1);
                            writer.WriteLine($"  {varName} = {value:F3}");
                        }
                        writer.WriteLine();
                        writer.WriteLine("Final Table:");
                        writer.WriteLine(bestSolution.ToString());
                    }
                    else
                    {
                        writer.WriteLine("No integer solution found!");
                    }
                    
                    // Write summary
                    var allSubproblems = branchAndBound.GetAllSubproblems();
                    writer.WriteLine();
                    writer.WriteLine("=== SUMMARY ===");
                    writer.WriteLine($"Total subproblems generated: {allSubproblems.Count}");
                    writer.WriteLine($"Processing steps: {processingOrder.Count}");
                    writer.WriteLine($"Fathomed nodes: {fathomReasons.Count}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error exporting Branch & Bound results: {ex.Message}");
            }
        }

        static void ExecuteNonLinearProgramming()
        {
            Console.Clear();
            Console.WriteLine("╔══════════════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                   EXECUTING NON-LINEAR PROGRAMMING                           ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════════════════════╝");
            Console.WriteLine();
            
            try
            {
                // Validate problem type
                if (currentProblemType != ProblemType.NonLinearProgramming)
                {
                    Console.WriteLine("Error: Non-Linear Programming algorithm requires an NLP problem.");
                    Console.WriteLine("The currently loaded file contains a Linear Programming problem.");
                    Console.WriteLine("Please load an NLP file (format: F(x,y) = ...) or select an LP/IP algorithm instead.");
                    return;
                }
                
                // Check if we have an NLP problem loaded
                if (string.IsNullOrEmpty(currentInputPath))
                {
                    Console.WriteLine("Error: No file loaded. Please load an NLP file first.");
                    return;
                }
                
                // Re-parse the file to get NLP data
                var (problemType, data) = fileReader.ParseFile(currentInputPath);
                
                var nlpProblem = (NLPProblem)data;
                
                Console.WriteLine("Starting Non-Linear Programming optimization...");
                Console.WriteLine();
                
                // Create and execute NLP algorithm
                var nlpAlgorithm = new NLPAlgorithm();
                var solution = nlpAlgorithm.SolveNLP(nlpProblem);
                
                Console.WriteLine();
                Console.WriteLine("✓ NLP optimization completed successfully!");
                Console.WriteLine();
                
                // Display comprehensive results
                nlpAlgorithm.DisplayResults(solution);
                
                // Export NLP results to output.txt
                ExportNLPResults(solution);
                
                Console.WriteLine();
                Console.WriteLine("✓ NLP results exported to data/output.txt");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error executing Non-Linear Programming: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }

        static void ExportNLPResults(NLPProblem solution)
        {
            try
            {
                // Ensure data directory exists
                string dataDir = "data";
                if (!Directory.Exists(dataDir))
                {
                    Directory.CreateDirectory(dataDir);
                }
                
                string outputPath = Path.Combine(dataDir, "output.txt");
                using (StreamWriter writer = new StreamWriter(outputPath))
                {
                    writer.WriteLine("╔══════════════════════════════════════════════════════════════════════════════╗");
                    writer.WriteLine("║                        NLP OPTIMIZATION RESULTS                              ║");
                    writer.WriteLine("╚══════════════════════════════════════════════════════════════════════════════╝");
                    writer.WriteLine();
                    
                    writer.WriteLine($"Original Function: f(x,y) = {solution.Function}");
                    writer.WriteLine($"Starting Point: ({solution.StartingPoint.x}, {solution.StartingPoint.y})");
                    writer.WriteLine();
                    
                    writer.WriteLine("=== OPTIMAL SOLUTION ===");
                    writer.WriteLine($"Critical Point: ({solution.OptimalPoint.x:F6}, {solution.OptimalPoint.y:F6})");
                    writer.WriteLine($"Function Value: f({solution.OptimalPoint.x:F3}, {solution.OptimalPoint.y:F3}) = {solution.OptimalValue:F6}");
                    writer.WriteLine($"Point Type: {solution.PointType}");
                    writer.WriteLine();
                    
                    writer.WriteLine("=== MATHEMATICAL VERIFICATION ===");
                    writer.WriteLine($"∂f/∂x = {solution.Dx:F6} (should be ≈ 0)");
                    writer.WriteLine($"∂f/∂y = {solution.Dy:F6} (should be ≈ 0)");
                    writer.WriteLine();
                    
                    writer.WriteLine("Hessian Matrix:");
                    writer.WriteLine($"H = [{solution.HessianMatrix[0,0]:F3}, {solution.HessianMatrix[0,1]:F3}]");
                    writer.WriteLine($"    [{solution.HessianMatrix[1,0]:F3}, {solution.HessianMatrix[1,1]:F3}]");
                    writer.WriteLine();
                    writer.WriteLine($"Hessian Determinant |H| = {solution.HessianDeterminant:F6}");
                    
                    string interpretation = solution.PointType switch
                    {
                        CriticalPointType.ConvexLocalMinimum => "|H| > 0 and ∂²f/∂x² > 0 → Local Minimum",
                        CriticalPointType.ConcaveLocalMaximum => "|H| > 0 and ∂²f/∂x² < 0 → Local Maximum", 
                        CriticalPointType.SaddlePoint => "|H| < 0 → Saddle Point",
                        CriticalPointType.Inconclusive => "|H| = 0 → Test Inconclusive",
                        _ => "Unknown classification"
                    };
                    
                    writer.WriteLine($"Second Derivative Test: {interpretation}");
                    writer.WriteLine();
                    
                    writer.WriteLine("=== CALCULUS DETAILS ===");
                    writer.WriteLine($"Second derivatives:");
                    writer.WriteLine($"  ∂²f/∂x² = {solution.Dxx:F6}");
                    writer.WriteLine($"  ∂²f/∂x∂y = {solution.Dxy:F6}");
                    writer.WriteLine($"  ∂²f/∂y∂x = {solution.Dyx:F6}");
                    writer.WriteLine($"  ∂²f/∂y² = {solution.Dyy:F6}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error exporting NLP results: {ex.Message}");
            }
        }

        static void ExportResults()
        {
            try
            {
                // Ensure data directory exists
                string dataDir = "data";
                if (!Directory.Exists(dataDir))
                {
                    Directory.CreateDirectory(dataDir);
                }
                
                string outputPath = Path.Combine(dataDir, "output.txt");
                using (StreamWriter writer = new StreamWriter(outputPath))
                {
                    var initialTable = TableCache.GetTable("t-i");
                    if (initialTable != null)
                    {
                        writer.WriteLine("Canonical Form:");
                        writer.WriteLine(initialTable.ToString());
                    }
                    
                    foreach (var table in TableCache.GetAllTables().Where(t => 
                        t.Status == "Iteration" || t.Status == "Optimal" || 
                        t.Status == "Infeasible" || t.Status == "Unbounded"))
                    {
                        writer.WriteLine($"\nTable {table.TableId} ({table.Status}):");
                        writer.WriteLine(table.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error exporting results: {ex.Message}");
            }
        }

    }
}