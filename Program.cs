using System;
using LinearProgrammingSolver.Utils;
using LinearProgrammingSolver.Tables;
using LinearProgrammingSolver.Algorithms;
using LinearProgrammingSolver.Analysis;

namespace LinearProgrammingSolver
{
    // Enum for main menu options
    public enum MainMenuOption
    {
        LoadFile = 1,
        SelectAlgorithm = 2,
        Analysis = 3,
        ViewResults = 4,
        Exit = 5
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
                            case MainMenuOption.Analysis:
                                HandleAnalysis();
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
            Console.WriteLine("║  3. Analysis                  - Duality, sensitivity, and shadow prices      ║");
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

        static void HandleAnalysis()
        {
            try { Console.Clear(); } catch { /* Ignore clear failures */ }
            
            // Let the Analysis system handle its own validation and pipeline execution
            Analysis.Analysis.RunAnalysisMenu();
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