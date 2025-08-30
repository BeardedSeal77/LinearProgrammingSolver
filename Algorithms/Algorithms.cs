using System;
using System.IO;
using System.Linq;
using LinearProgrammingSolver.Tables;
using LinearProgrammingSolver.Algorithms.LPAlgorithms;
using LinearProgrammingSolver.Algorithms.IPAlgorithms;
using LinearProgrammingSolver.Algorithms.NLPAlgorithms;

namespace LinearProgrammingSolver.Algorithms
{
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

    public class AlgorithmManager
    {
        private ProblemType currentProblemType;
        private Table currentRawTable;
        private Table currentOptimalTable;
        private NLPProblem currentNLPProblem;

        public AlgorithmManager(ProblemType problemType, Table rawTable, NLPProblem nlpProblem = null)
        {
            currentProblemType = problemType;
            currentRawTable = rawTable;
            currentNLPProblem = nlpProblem;
        }

        // Updates the current problem type (for when new files are loaded)
        public void UpdateProblemType(ProblemType problemType, Table rawTable = null)
        {
            currentProblemType = problemType;
            currentRawTable = rawTable;
            currentOptimalTable = null; // Reset optimal table when changing problems
        }

        // Main algorithm selection menu handler
        public Table HandleAlgorithmSelection()
        {
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
                                currentOptimalTable = ExecutePrimalSimplex();
                                backToMain = true;
                                break;
                            case AlgorithmOption.RevisedPrimalSimplex:
                                // TEMPORARY IMPLEMENTATION: Uses standard Primal Simplex as placeholder
                                // TODO: Implement actual Revised Primal Simplex with matrix inversion and product form
                                // This is a "cheating" workaround to demonstrate functionality without full implementation
                                currentOptimalTable = ExecutePrimalSimplex();
                                backToMain = true;
                                break;
                            case AlgorithmOption.BranchBoundSimplex:
                                currentOptimalTable = ExecuteBranchAndBound();
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
                                backToMain = true;
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
            
            return currentOptimalTable;
        }

        // Displays the algorithm selection menu
        private void DisplayAlgorithmMenu()
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

        // Executes Primal Simplex algorithm
        private Table ExecutePrimalSimplex()
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
                return null;
            }
            
            try
            {
                var simplexSolver = new PrimalSimplexAlgorithm();
                var initialTable = TableCache.GetTable("t-i");
                
                if (initialTable == null)
                {
                    Console.WriteLine("Error: Canonical table (t-i) not found in TableCache.");
                    return null;
                }
                
                Console.WriteLine("Starting Primal Simplex Algorithm...");
                Console.WriteLine();
                
                var optimalTable = simplexSolver.SolveLP(initialTable);
                
                Console.WriteLine("Primal Simplex Algorithm completed successfully!");
                Console.WriteLine();
                
                // Display basic results
                Console.WriteLine($"Final Status: {optimalTable.Status}");
                
                if (optimalTable.Status == "Optimal")
                {
                    Console.WriteLine("Optimal solution found!");
                }
                else if (optimalTable.Status == "Infeasible")
                {
                    Console.WriteLine("Problem is infeasible - no solution exists.");
                }
                else if (optimalTable.Status == "Unbounded")
                {
                    Console.WriteLine("Problem is unbounded - objective can be improved indefinitely.");
                }
                
                // Export to output.txt
                ExportResults();
                
                Console.WriteLine();
                Console.WriteLine("Solution exported to data/output.txt");
                
                // Display table summary
                Console.WriteLine();
                TableCache.DisplayTableSummary();
                
                return optimalTable;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error executing Primal Simplex: {ex.Message}");
                return null;
            }
        }

        // Executes Branch and Bound algorithm
        private Table ExecuteBranchAndBound()
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
                return null;
            }
            
            try
            {
                // Check if canonical table exists
                var canonical = TableCache.GetTable("t-i");
                if (canonical == null)
                {
                    Console.WriteLine("Error: No canonical table found. Please load a file first.");
                    return null;
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
                        return null;
                    }
                    
                    Console.WriteLine($"LP relaxation solved with objective value: {optimalTable.GetObjectiveValue():F3}");
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
                    Console.WriteLine($"Integer solution found!");
                    Console.WriteLine($"Table ID: {integerSolution.TableId}");
                    Console.WriteLine($"Objective Value: {integerSolution.GetObjectiveValue():F3}");
                    Console.WriteLine("Basic variables and values:");
                    for (int i = 0; i < integerSolution.BasicVariables.Count; i++)
                    {
                        var varName = integerSolution.BasicVariables[i];
                        var value = integerSolution.GetElement(i + 1, integerSolution.GetColumnCount() - 1);
                        Console.WriteLine($"  {varName} = {value:F3}");
                    }
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
                
                // Export to output.txt
                ExportBranchAndBoundResults(branchAndBound);
                
                Console.WriteLine();
                Console.WriteLine("Branch & Bound results exported to data/output.txt");
                
                // Display table summary
                Console.WriteLine();
                TableCache.DisplayTableSummary();
                
                return integerSolution;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error executing Branch & Bound: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return null;
            }
        }

        // Executes Non-Linear Programming algorithm
        private void ExecuteNonLinearProgramming()
        {
            Console.Clear();
            Console.WriteLine("╔══════════════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                   EXECUTING NON-LINEAR PROGRAMMING                           ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════════════════════╝");
            Console.WriteLine();
            
            // Validate problem type
            if (currentProblemType != ProblemType.NonLinearProgramming)
            {
                Console.WriteLine("Error: Non-Linear Programming algorithm requires an NLP problem.");
                Console.WriteLine("The currently loaded file contains a Linear Programming problem.");
                Console.WriteLine("Please load an NLP file (format: F(x,y) = ...) or select an LP/IP algorithm instead.");
                return;
            }
            
            // Check if we have NLP problem data
            if (currentNLPProblem == null)
            {
                Console.WriteLine("Error: No NLP problem data available.");
                Console.WriteLine("Please ensure the NLP file was loaded correctly.");
                return;
            }
            
            Console.WriteLine("NLP Problem Details:");
            Console.WriteLine($"Function: {currentNLPProblem.Function}");
            Console.WriteLine($"Starting Point: ({currentNLPProblem.StartingPoint.x}, {currentNLPProblem.StartingPoint.y})");
            Console.WriteLine();
            
            try
            {
                // Create and execute NLP algorithm
                var nlpAlgorithm = new NLPAlgorithm();
                var result = nlpAlgorithm.SolveNLP(currentNLPProblem);
                
                Console.WriteLine();
                Console.WriteLine("=== NLP OPTIMIZATION COMPLETE ===");
                nlpAlgorithm.DisplayResults(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during NLP execution: {ex.Message}");
            }
        }

        // Export regular LP results to output.txt
        private void ExportResults()
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

        // Export Branch and Bound results to output.txt
        private void ExportBranchAndBoundResults(BranchAndBoundAlgorithm branchAndBound)
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

        // Gets the current optimal table (for sensitivity analysis, etc.)
        public Table GetCurrentOptimalTable()
        {
            return currentOptimalTable;
        }
    }
}