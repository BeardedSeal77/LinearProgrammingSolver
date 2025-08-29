using System;
using LinearProgrammingSolver.Utils;
using LinearProgrammingSolver.Tables;
using LinearProgrammingSolver.LPAlgorithms;
using LinearProgrammingSolver.IPAlgorithms;

namespace LinearProgrammingSolver
{
    public class Test
    {
        public static void RunTest()
        {
            Console.WriteLine("=== AUTOMATED BRANCH & BOUND TEST ===\n");
            
            try
            {
                // Step 1: Load the input file
                Console.WriteLine("1. Loading data/input.txt...");
                var fileReader = new FileReader();
                var (matrix, rowLabels, columnLabels, optimizationType, constraintOperators) = fileReader.ParseFile("data/input.txt");
                
                var rawTable = new Table("t-raw", matrix, rowLabels, columnLabels, optimizationType, "Raw", constraintOperators);
                TableCache.StoreTable(rawTable);
                Console.WriteLine("✓ Raw table loaded and cached\n");
                
                // Step 2: Convert to canonical form
                Console.WriteLine("2. Converting to canonical form...");
                var canonicalConverter = new CanonicalFormConverter();
                var canonicalTable = canonicalConverter.ConvertToCanonicalForm(rawTable);
                TableCache.StoreTable(canonicalTable);
                Console.WriteLine("✓ Canonical table created and cached\n");
                
                // Step 3: Run Primal Simplex to get LP optimal
                Console.WriteLine("3. Solving LP relaxation with Primal Simplex...");
                var primalSimplex = new PrimalSimplexAlgorithm();
                var optimalTable = primalSimplex.SolveLP(canonicalTable);
                
                if (optimalTable != null && optimalTable.IsOptimal())
                {
                    Console.WriteLine($"✓ LP Optimal found with objective value: {optimalTable.GetObjectiveValue()}");
                    Console.WriteLine("Basic variables and values:");
                    for (int i = 0; i < optimalTable.BasicVariables.Count; i++)
                    {
                        var varName = optimalTable.BasicVariables[i];
                        var value = optimalTable.GetElement(i + 1, optimalTable.GetColumnCount() - 1);
                        Console.WriteLine($"  {varName} = {value}");
                    }
                    
                    // Step 4: Run Complete Branch & Bound
                    Console.WriteLine("\n4. Starting Complete Branch & Bound with LIFO stack...");
                    var branchAndBound = new BranchAndBoundAlgorithm();
                    
                    var bestIntegerSolution = branchAndBound.SolveIP(optimalTable);
                    
                    // Display processing results
                    Console.WriteLine("\n=== BRANCH & BOUND PROCESSING LOG ===");
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
                    if (bestIntegerSolution != null)
                    {
                        Console.WriteLine($"Table ID: {bestIntegerSolution.TableId}");
                        Console.WriteLine($"Objective Value: {bestIntegerSolution.GetObjectiveValue():F3}");
                        Console.WriteLine("Basic variables and values:");
                        for (int i = 0; i < bestIntegerSolution.BasicVariables.Count; i++)
                        {
                            var varName = bestIntegerSolution.BasicVariables[i];
                            var value = bestIntegerSolution.GetElement(i + 1, bestIntegerSolution.GetColumnCount() - 1);
                            Console.WriteLine($"  {varName} = {value:F3}");
                        }
                        Console.WriteLine("\nFinal Integer Solution Table:");
                        bestIntegerSolution.DisplayTraditional();
                    }
                    else
                    {
                        Console.WriteLine("No integer solution found!");
                    }
                    
                    // Display all generated subproblems count
                    var allSubproblems = branchAndBound.GetAllSubproblems();
                    Console.WriteLine($"\n=== SUMMARY ===");
                    Console.WriteLine($"Total subproblems generated: {allSubproblems.Count}");
                    Console.WriteLine($"Processing steps: {processingOrder.Count}");
                    Console.WriteLine($"Fathomed nodes: {fathomReasons.Count}");
                }
                else
                {
                    Console.WriteLine("LP relaxation could not be solved optimally.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in test: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
            
            Console.WriteLine("\n=== TEST COMPLETED ===");
        }
    }
}