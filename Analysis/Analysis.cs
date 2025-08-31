using System;
using LinearProgrammingSolver.Tables;
using LinearProgrammingSolver.Algorithms.Implementations.LP;
using LinearProgrammingSolver.Utils;

namespace LinearProgrammingSolver.Analysis
{
    public enum AnalysisOption
    {
        DualityAnalysis = 1,
        SensitivityAnalysis = 2,
        BackToMain = 3
    }

    public class Analysis
    {
        public static void RunAnalysisMenu()
        {
            bool backToMain = false;
            
            while (!backToMain)
            {
                DisplayAnalysisMenu();
                
                if (int.TryParse(Console.ReadLine(), out int choice))
                {
                    if (choice >= 1 && choice <= 3)
                    {
                        var selectedOption = (AnalysisOption)choice;
                        
                        switch (selectedOption)
                        {
                            case AnalysisOption.DualityAnalysis:
                                RunDualityAnalysis();
                                break;
                            case AnalysisOption.SensitivityAnalysis:
                                RunSensitivityAnalysis();
                                break;
                            case AnalysisOption.BackToMain:
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

        private static void DisplayAnalysisMenu()
        {
            Console.WriteLine("╔══════════════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                          ANALYSIS MENU                                       ║");
            Console.WriteLine("╠══════════════════════════════════════════════════════════════════════════════╣");
            Console.WriteLine("║                                                                              ║");
            Console.WriteLine("║  1. Duality Analysis          - Construct and solve dual problem             ║");
            Console.WriteLine("║  2. Sensitivity Analysis       - Shadow prices, ranges, reduced costs       ║");
            Console.WriteLine("║  3. Back to Main Menu          - Return to main menu                         ║");
            Console.WriteLine("║                                                                              ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════════════════════╝");
            Console.WriteLine();
            Console.Write("Select an analysis option (1-3): ");
        }

        private static void RunDualityAnalysis()
        {
            try { Console.Clear(); } catch { /* Ignore clear failures */ }
            
            Console.WriteLine("╔══════════════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                         DUALITY ANALYSIS                                     ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════════════════════╝");
            Console.WriteLine();
            
            // Clear output file before analysis
            try
            {
                var fileWriter = new FileWriter();
                fileWriter.ClearOutputFile("data/output.txt");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Could not clear output file: {ex.Message}");
            }
            
            // Ensure we have all required tables
            var requiredTables = EnsureRequiredTables("Duality");
            if (!requiredTables.success)
            {
                Console.WriteLine($"Error: {requiredTables.message}");
                return;
            }
            
            Duality.RunDualityAnalysis(requiredTables.canonicalTable, requiredTables.optimalTable);
        }

        private static void RunSensitivityAnalysis()
        {
            try { Console.Clear(); } catch { /* Ignore clear failures */ }
            
            Console.WriteLine("╔══════════════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                       SENSITIVITY ANALYSIS                                   ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════════════════════╝");
            Console.WriteLine();
            
            // Clear output file before analysis
            try
            {
                var fileWriter = new FileWriter();
                fileWriter.ClearOutputFile("data/output.txt");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Could not clear output file: {ex.Message}");
            }
            
            // Ensure we have all required tables
            var requiredTables = EnsureRequiredTables("Sensitivity");
            if (!requiredTables.success)
            {
                Console.WriteLine($"Error: {requiredTables.message}");
                return;
            }
            
            Sensitivity.RunSensitivityAnalysis(requiredTables.optimalTable);
        }


        private static (bool success, Table canonicalTable, Table optimalTable, string message) EnsureRequiredTables(string analysisType = "general")
        {
            Console.WriteLine($"Checking required tables for {analysisType} analysis...");
            
            // Analysis-specific requirements check
            if (analysisType == "Duality")
            {
                Console.WriteLine("Required tables: Raw table → Canonical table → Optimal table");
            }
            else if (analysisType == "Sensitivity")
            {
                Console.WriteLine("Required tables: Raw table → Canonical table → Optimal table");
            }
            
            // Check if we have a raw table (prerequisite for everything)
            var rawTable = TableCache.GetTable("t-raw");
            if (rawTable == null)
            {
                return (false, null, null, "No input file loaded. Please load a file first using option 1 (Load Input File).");
            }
            
            Console.WriteLine("Raw table found");
            
            // Check for canonical table
            var canonicalTable = TableCache.GetTable("t-i");
            if (canonicalTable == null)
            {
                Console.WriteLine("! Canonical table not found. Converting raw table to canonical form...");
                try
                {
                    var converter = new CanonicalFormConverter();
                    canonicalTable = converter.ConvertToCanonicalForm(rawTable);
                    if (canonicalTable == null)
                    {
                        return (false, null, null, "Failed to convert raw table to canonical form.");
                    }
                    Console.WriteLine("Canonical table created successfully");
                }
                catch (Exception ex)
                {
                    return (false, null, null, $"Error converting to canonical form: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine("Canonical table found");
            }
            
            // Check for optimal table
            var optimalTable = TableCache.GetTable("t-optimal");
            if (optimalTable == null)
            {
                Console.WriteLine("! Optimal table not found. Running Primal Simplex algorithm...");
                try
                {
                    var primalSimplex = new PrimalSimplexAlgorithm();
                    optimalTable = primalSimplex.SolveLP(canonicalTable);
                    
                    if (optimalTable == null)
                    {
                        return (false, null, null, "Primal Simplex algorithm failed to produce a solution.");
                    }
                    
                    Console.WriteLine($"Primal Simplex completed with status: {optimalTable.Status}");
                }
                catch (Exception ex)
                {
                    return (false, null, null, $"Error running Primal Simplex: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine("Optimal table found");
            }
            
            // Verify optimal table has correct status
            if (optimalTable.Status != "Optimal")
            {
                return (false, null, null, $"Analysis requires an optimal solution. Current status: {optimalTable.Status}");
            }
            
            // Analysis-specific validation
            if (analysisType == "Duality")
            {
                // Additional validation for duality analysis
                if (canonicalTable.OptimizationType.ToString() != "Maximization")
                {
                    Console.WriteLine("! Note: Duality analysis works best with maximization problems");
                }
                Console.WriteLine("Duality analysis prerequisites satisfied: Canonical form + Optimal solution");
            }
            else if (analysisType == "Sensitivity")
            {
                // Additional validation for sensitivity analysis
                if (optimalTable.BasicVariables == null || optimalTable.BasicVariables.Count == 0)
                {
                    return (false, null, null, "Sensitivity analysis requires basic variables information in optimal table");
                }
                Console.WriteLine("Sensitivity analysis prerequisites satisfied: Optimal solution with basic variables");
            }
            
            Console.WriteLine("All required tables available for analysis");
            Console.WriteLine();
            
            return (true, canonicalTable, optimalTable, "Success");
        }
    }
}