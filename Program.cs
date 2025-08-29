using System;
using LinearProgrammingSolver.Utils;
using LinearProgrammingSolver.Tables;
using LinearProgrammingSolver.Algorithms;

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
        private static FileManager fileManager = new FileManager();
        private static AlgorithmManager algorithmManager = null;
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
                        try { Console.Clear(); } catch { /* Ignore clear failures */ }
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
                    try { Console.Clear(); } catch { /* Ignore clear failures */ }
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
                Console.WriteLine($"Current file: {System.IO.Path.GetFileName(currentInputPath)}");
                Console.WriteLine($"Problem type: {currentProblemType}");
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
            var (problemType, rawTable, filePath, nlpProblem) = fileManager.HandleLoadFile();
            
            if (!string.IsNullOrEmpty(filePath))
            {
                currentInputPath = filePath;
                currentProblemType = problemType;
                
                // Create or update algorithm manager
                algorithmManager = new AlgorithmManager(problemType, rawTable, nlpProblem);
                
                Console.WriteLine("\nFile loaded successfully!");
                Console.WriteLine($"Problem type: {problemType}");
            }
        }

        static void HandleAlgorithmSelection()
        {
            try { Console.Clear(); } catch { /* Ignore clear failures */ }
            
            if (algorithmManager == null)
            {
                Console.WriteLine("Error: No file loaded. Please load a file first.");
                return;
            }
            
            algorithmManager.HandleAlgorithmSelection();
        }

        static void HandleSensitivityAnalysis()
        {
            try { Console.Clear(); } catch { /* Ignore clear failures */ }
            
            if (algorithmManager == null)
            {
                Console.WriteLine("Error: No file loaded. Please load a file first.");
                return;
            }
            
            var currentOptimalTable = algorithmManager.GetCurrentOptimalTable();
            
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
                    try { Console.Clear(); } catch { /* Ignore clear failures */ }
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
            try { Console.Clear(); } catch { /* Ignore clear failures */ }
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
                        if (algorithmManager != null)
                        {
                            var currentOptimalTable = algorithmManager.GetCurrentOptimalTable();
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
    }
}