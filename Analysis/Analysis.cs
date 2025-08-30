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
        ShadowPrices = 3,
        BackToMain = 4
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
                    if (choice >= 1 && choice <= 4)
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
                            case AnalysisOption.ShadowPrices:
                                RunShadowPriceAnalysis();
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
            Console.WriteLine("║  2. Sensitivity Analysis       - Variable and constraint sensitivity         ║");
            Console.WriteLine("║  3. Shadow Prices              - Extract shadow prices from optimal tableau   ║");
            Console.WriteLine("║  4. Back to Main Menu          - Return to main menu                         ║");
            Console.WriteLine("║                                                                              ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════════════════════╝");
            Console.WriteLine();
            Console.Write("Select an analysis option (1-4): ");
        }

        private static void RunDualityAnalysis()
        {
            try { Console.Clear(); } catch { /* Ignore clear failures */ }
            
            Console.WriteLine("╔══════════════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                         DUALITY ANALYSIS                                     ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════════════════════╝");
            Console.WriteLine();
            
            // Ensure we have all required tables
            var requiredTables = EnsureRequiredTables();
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
            
            // Ensure we have all required tables
            var requiredTables = EnsureRequiredTables();
            if (!requiredTables.success)
            {
                Console.WriteLine($"Error: {requiredTables.message}");
                return;
            }
            
            Sensitivity.RunSensitivityAnalysis(requiredTables.optimalTable);
        }

        private static void RunShadowPriceAnalysis()
        {
            try { Console.Clear(); } catch { /* Ignore clear failures */ }
            
            Console.WriteLine("╔══════════════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                        SHADOW PRICE ANALYSIS                                 ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════════════════════╝");
            Console.WriteLine();
            
            // Ensure we have all required tables
            var requiredTables = EnsureRequiredTables();
            if (!requiredTables.success)
            {
                Console.WriteLine($"Error: {requiredTables.message}");
                return;
            }
            
            ExtractAndDisplayShadowPrices(requiredTables.optimalTable);
        }

        private static void ExtractAndDisplayShadowPrices(Table optimalTable)
        {
            Console.WriteLine("Shadow Prices (Dual Variable Values):");
            Console.WriteLine("=====================================");
            
            int objRow = 0;
            var columnLabels = optimalTable.ColumnLabels;
            
            Console.WriteLine("Variable".PadRight(15) + "Shadow Price");
            Console.WriteLine(new string('-', 30));
            
            for (int j = 0; j < optimalTable.GetColumnCount() - 1; j++)
            {
                string varName = columnLabels[j];
                double shadowPrice = 0.0;
                
                if (varName.StartsWith("s") || varName.StartsWith("e"))
                {
                    shadowPrice = optimalTable.GetElement(objRow, j);
                    Console.WriteLine($"{varName.PadRight(15)}{shadowPrice:F6}");
                }
            }
            
            Console.WriteLine();
            Console.WriteLine("Note: Shadow prices represent the marginal value of relaxing constraints.");
            Console.WriteLine("      Non-zero values indicate binding constraints.");
        }

        /// <summary>
        /// Ensures all required tables exist for analysis. If missing, runs the appropriate pipeline.
        /// Returns (success, canonicalTable, optimalTable, message).
        /// </summary>
        private static (bool success, Table canonicalTable, Table optimalTable, string message) EnsureRequiredTables()
        {
            Console.WriteLine("Checking required tables for analysis...");
            
            // Check if we have a raw table (prerequisite for everything)
            var rawTable = TableCache.GetTable("t-raw");
            if (rawTable == null)
            {
                return (false, null, null, "No input file loaded. Please load a file first using option 1 (Load Input File).");
            }
            
            Console.WriteLine("✓ Raw table found");
            
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
                    Console.WriteLine("✓ Canonical table created successfully");
                }
                catch (Exception ex)
                {
                    return (false, null, null, $"Error converting to canonical form: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine("✓ Canonical table found");
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
                    
                    Console.WriteLine($"✓ Primal Simplex completed with status: {optimalTable.Status}");
                }
                catch (Exception ex)
                {
                    return (false, null, null, $"Error running Primal Simplex: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine("✓ Optimal table found");
            }
            
            // Verify optimal table has correct status
            if (optimalTable.Status != "Optimal")
            {
                return (false, null, null, $"Analysis requires an optimal solution. Current status: {optimalTable.Status}");
            }
            
            Console.WriteLine("✓ All required tables available for analysis");
            Console.WriteLine();
            
            return (true, canonicalTable, optimalTable, "Success");
        }
    }
}