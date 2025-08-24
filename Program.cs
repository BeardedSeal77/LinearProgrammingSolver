using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using LinearProgrammingSolver.Utils;
using LinearProgrammingSolver.Tables;
using LinearProgrammingSolver.LPAlgorithms;
// using LinearProgrammingSolver.IPAlgorithms;

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
        BackToMain = 6
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

    class Program
    {
        private static FileReader fileReader = new FileReader();
        private static CanonicalFormConverter canonicalConverter = new CanonicalFormConverter();
        private static Table currentRawTable = null;
        private static Table currentOptimalTable = null;
        private static string currentInputPath = "";

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
            Console.WriteLine("        LPR 381 Project - Menu-Driven LP/IP Solver by Edward Cullinan");
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
            Console.WriteLine("║                              MAIN MENU                                      ║");
            Console.WriteLine("╠══════════════════════════════════════════════════════════════════════════════╣");
            Console.WriteLine("║                                                                              ║");
            Console.WriteLine("║  1. Load Input File          - Load LP/IP model from text file             ║");
            Console.WriteLine("║  2. Select Algorithm          - Choose solving algorithm                     ║");
            Console.WriteLine("║  3. Sensitivity Analysis      - Perform post-solution analysis              ║");
            Console.WriteLine("║  4. View Results              - Display solution and tables                 ║");
            Console.WriteLine("║  5. Exit                      - Close the program                           ║");
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
            Console.WriteLine("╔══════════════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                            LOAD INPUT FILE                                    ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════════════════════╝");
            Console.WriteLine();
            
            try
            {
                // Clear any existing tables from previous runs
                TableCache.ClearAllTables();
                
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
                    Console.WriteLine($"Input file not found at: {inputPath}");
                    Console.Write("Please enter the full path to your input file: ");
                    string userPath = Console.ReadLine();
                    
                    if (!string.IsNullOrEmpty(userPath) && File.Exists(userPath))
                    {
                        inputPath = userPath;
                    }
                    else
                    {
                        Console.WriteLine("Invalid file path. Operation cancelled.");
                        return;
                    }
                }
                
                currentInputPath = inputPath;
                Console.WriteLine($"Processing: {inputPath}");
                Console.WriteLine();
                
                // Step 1: FileReader ONLY parses (no table construction)
                var (matrix, rowLabels, columnLabels, optimizationType, constraintOperators) = fileReader.ParseFile(inputPath);
                
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
                Console.WriteLine("File loaded successfully!");
                Console.WriteLine($"Problem type: {optimizationType}");
                Console.WriteLine($"Variables: {currentRawTable.GetVariableCount()}");
                Console.WriteLine($"Constraints: {currentRawTable.GetRowCount() - 1}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading file: {ex.Message}");
                currentInputPath = "";
                currentRawTable = null;
            }
        }

        static void HandleAlgorithmSelection()
        {
            Console.Clear();
            
            if (currentRawTable == null)
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
                    if (choice >= 1 && choice <= 6)
                    {
                        var selectedOption = (AlgorithmOption)choice;
                        
                        switch (selectedOption)
                        {
                            case AlgorithmOption.PrimalSimplex:
                                ExecutePrimalSimplex();
                                backToMain = true;
                                break;
                            case AlgorithmOption.RevisedPrimalSimplex:
                                Console.WriteLine("Revised Primal Simplex Algorithm - Coming Soon!");
                                break;
                            case AlgorithmOption.BranchBoundSimplex:
                                Console.WriteLine("Branch & Bound Simplex Algorithm - Coming Soon!");
                                break;
                            case AlgorithmOption.BranchBoundKnapsack:
                                Console.WriteLine("Branch & Bound Knapsack Algorithm - Coming Soon!");
                                break;
                            case AlgorithmOption.CuttingPlane:
                                Console.WriteLine("Cutting Plane Algorithm - Coming Soon!");
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
            Console.WriteLine("║                         ALGORITHM SELECTION                                    ║");
            Console.WriteLine("╠══════════════════════════════════════════════════════════════════════════════╣");
            Console.WriteLine("║                                                                              ║");
            Console.WriteLine("║  1. Primal Simplex             - Standard simplex algorithm                  ║");
            Console.WriteLine("║  2. Revised Primal Simplex     - Matrix-based simplex method                 ║");
            Console.WriteLine("║  3. Branch & Bound Simplex     - Integer programming via simplex             ║");
            Console.WriteLine("║  4. Branch & Bound Knapsack    - Specialized knapsack algorithm              ║");
            Console.WriteLine("║  5. Cutting Plane Algorithm    - Integer programming via cutting planes      ║");
            Console.WriteLine("║  6. Back to Main Menu          - Return to main menu                         ║");
            Console.WriteLine("║                                                                              ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════════════════════╝");
            Console.WriteLine();
            Console.Write("Select an algorithm (1-6): ");
        }

        static void ExecutePrimalSimplex()
        {
            Console.Clear();
            Console.WriteLine("╔══════════════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                        EXECUTING PRIMAL SIMPLEX                               ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════════════════════╝");
            Console.WriteLine();
            
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
                Console.WriteLine("Solution exported to output.txt");
                
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
            Console.WriteLine("║  Variable Analysis:                                                           ║");
            Console.WriteLine("║    1. Non-Basic Variable Range     - Display range analysis                   ║");
            Console.WriteLine("║    2. Non-Basic Variable Change    - Apply and display changes               ║");
            Console.WriteLine("║    3. Basic Variable Range         - Display range analysis                   ║");
            Console.WriteLine("║    4. Basic Variable Change        - Apply and display changes               ║");
            Console.WriteLine("║    5. Variable in Non-Basic Range  - Column range analysis                   ║");
            Console.WriteLine("║    6. Variable in Non-Basic Change - Column change analysis                  ║");
            Console.WriteLine("║                                                                              ║");
            Console.WriteLine("║  Constraint Analysis:                                                         ║");
            Console.WriteLine("║    7. Constraint RHS Range         - Right-hand-side range analysis          ║");
            Console.WriteLine("║    8. Constraint RHS Change        - Right-hand-side change analysis         ║");
            Console.WriteLine("║                                                                              ║");
            Console.WriteLine("║  Solution Modifications:                                                      ║");
            Console.WriteLine("║    9. Add New Activity             - Add new variable to solution             ║");
            Console.WriteLine("║   10. Add New Constraint          - Add new constraint to solution           ║");
            Console.WriteLine("║   11. Show Shadow Prices          - Display shadow price analysis            ║");
            Console.WriteLine("║                                                                              ║");
            Console.WriteLine("║  Duality Analysis:                                                            ║");
            Console.WriteLine("║   12. Duality Analysis            - Dual model analysis and verification      ║");
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
            Console.WriteLine("║                            VIEW RESULTS                                       ║");
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

        static void ExportResults()
        {
            try
            {
                using (StreamWriter writer = new StreamWriter("output.txt"))
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