using System;
using LinearProgrammingSolver.Utils;
using LinearProgrammingSolver.Tables;
using LinearProgrammingSolver.LPAlgorithms;
using LinearProgrammingSolver.IPAlgorithms;

namespace LinearProgrammingSolver
{
    public class MainTest
    {
        public static void RunMainTest()
        {
            Console.WriteLine("=== TESTING MAIN MENU BRANCH & BOUND INTEGRATION ===\n");
            
            try
            {
                // Clear cache and load file (simulating option 1)
                TableCache.ClearAllTables();
                Console.WriteLine("1. Loading data/input.txt...");
                var fileReader = new FileReader();
                var (matrix, rowLabels, columnLabels, optimizationType, constraintOperators) = fileReader.ParseFile("data/input.txt");
                
                var rawTable = new Table("t-raw", matrix, rowLabels, columnLabels, optimizationType, "Raw", constraintOperators);
                TableCache.StoreTable(rawTable);
                
                var canonicalConverter = new CanonicalFormConverter();
                var canonicalTable = canonicalConverter.ConvertToCanonicalForm(rawTable);
                TableCache.StoreTable(canonicalTable);
                Console.WriteLine("✓ File loaded and canonical form created\n");
                
                // Simulate selecting Branch & Bound (option 2, then option 3)
                Console.WriteLine("2. Testing Branch & Bound selection with no existing optimal...");
                
                // Verify t-optimal doesn't exist yet
                var existingOptimal = TableCache.GetTable("t-optimal");
                Console.WriteLine($"Existing t-optimal in cache: {(existingOptimal != null ? "YES" : "NO")}");
                
                // Test the ExecuteBranchAndBound logic
                Console.WriteLine("\n3. Executing Branch & Bound (should auto-run Primal first)...");
                TestExecuteBranchAndBound();
                
                Console.WriteLine("\n=== TEST COMPLETED SUCCESSFULLY ===");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in main test: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }
        
        private static void TestExecuteBranchAndBound()
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
                
                // Automatically run Primal Simplex
                var primalSimplex = new PrimalSimplexAlgorithm();
                optimalTable = primalSimplex.SolveLP(canonical);
                
                if (optimalTable == null || !optimalTable.IsOptimal())
                {
                    Console.WriteLine($"Error: LP relaxation could not be solved optimally. Status: {optimalTable?.Status ?? "null"}");
                    return;
                }
                
                Console.WriteLine($"✓ LP relaxation solved with objective value: {optimalTable.GetObjectiveValue():F3}");
            }
            else
            {
                Console.WriteLine($"Found existing optimal LP solution with objective value: {optimalTable.GetObjectiveValue():F3}");
            }
            
            // Start Branch & Bound
            Console.WriteLine("Starting Branch & Bound Integer Programming...");
            
            var branchAndBound = new BranchAndBoundAlgorithm();
            var integerSolution = branchAndBound.SolveIP(optimalTable);
            
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
            }
            else
            {
                Console.WriteLine("No integer solution found!");
            }
            
            // Display summary
            var allSubproblems = branchAndBound.GetAllSubproblems();
            var processingOrder = branchAndBound.GetProcessingOrder();
            var fathomReasons = branchAndBound.GetFathomReasons();
            Console.WriteLine($"\n=== SUMMARY ===");
            Console.WriteLine($"Total subproblems generated: {allSubproblems.Count}");
            Console.WriteLine($"Processing steps: {processingOrder.Count}");
            Console.WriteLine($"Fathomed nodes: {fathomReasons.Count}");
        }
    }
}