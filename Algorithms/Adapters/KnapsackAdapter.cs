using System;
using System.IO;
using System.Linq;
using LinearProgrammingSolver.Tables;
using LinearProgrammingSolver.Utils;
using LinearProgrammingSolver.Algorithms.Implementations.IP;
using LinearProgrammingSolver.Algorithms.Core;

namespace LinearProgrammingSolver.Algorithms.Adapters
{
    public class KnapsackAdapter : IFullAlgorithm
    {
        private KnapsackAlgorithm _algorithm;

        public string Name => "Knapsack Branch & Bound";
        public string Description => "0-1 Knapsack problem solved via branch and bound";
        public ProblemType[] SupportedTypes => new[] { ProblemType.LinearProgramming };
        public string[] RequiredTables => new[] { "t-raw" }; // Only needs raw input

        public Table Execute(AlgorithmContext context)
        {
            _algorithm = new KnapsackAlgorithm();
            
            // Use the raw table directly for knapsack
            var rawTable = context.RawTable ?? TableCache.GetTable("t-raw");
            if (rawTable == null)
            {
                Console.WriteLine("Error: No raw table available for knapsack problem");
                return null;
            }

            Console.WriteLine("Using raw problem formulation for knapsack solving...");
            Console.WriteLine();

            var result = _algorithm.SolveKnapsack(rawTable);
            
            if (result != null)
            {
                result.TableId = "knapsack-final";
                TableCache.StoreTable(result);
            }

            return result;
        }

        public void DisplayHeader()
        {
            Console.Clear();
            Console.WriteLine("╔══════════════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                      EXECUTING KNAPSACK BRANCH & BOUND                       ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════════════════════╝");
            Console.WriteLine();
        }

        public void ShowResults(Table result, AlgorithmContext context)
        {
            if (_algorithm == null)
            {
                Console.WriteLine("Algorithm was not executed properly.");
                return;
            }

            Console.WriteLine();
            Console.WriteLine("=== KNAPSACK BRANCH & BOUND RESULTS ===");

            // Display processing log
            Console.WriteLine();
            Console.WriteLine("=== PROCESSING ORDER ===");
            var processingOrder = _algorithm.GetProcessingOrder();
            foreach (var logEntry in processingOrder)
            {
                Console.WriteLine(logEntry);
            }

            // Display fathoming reasons
            Console.WriteLine();
            Console.WriteLine("=== FATHOMING REASONS ===");
            var fathomReasons = _algorithm.GetFathomReasons();
            foreach (var kvp in fathomReasons)
            {
                Console.WriteLine($"{kvp.Key}: {kvp.Value}");
            }

            // Display best solution
            Console.WriteLine();
            Console.WriteLine("=== OPTIMAL KNAPSACK SOLUTION ===");
            if (result != null)
            {
                Console.WriteLine("Knapsack problem solved successfully!");
                Console.WriteLine($"Table ID: {result.TableId}");
                Console.WriteLine($"Status: {result.Status}");
                Console.WriteLine($"Optimal Value: {result.GetElement(0, 0):F3}");
                
                Console.WriteLine();
                Console.WriteLine("Solution details displayed during algorithm execution above.");
            }
            else
            {
                Console.WriteLine("No feasible solution found!");
            }

            // Display summary
            var allSubproblems = _algorithm.GetAllSubproblems();
            Console.WriteLine();
            Console.WriteLine("=== SUMMARY ===");
            Console.WriteLine($"Total subproblems generated: {allSubproblems.Count}");
            Console.WriteLine($"Processing steps: {processingOrder.Count}");
            Console.WriteLine($"Fathomed nodes: {fathomReasons.Count}");
            Console.WriteLine("Knapsack Branch & Bound completed.");
        }

        public void ExportResults(Table result, AlgorithmContext context)
        {
            // Export is now handled by FileWriter in AlgorithmManager
            // This method serves as a fallback only
            Console.WriteLine("Export handled by FileWriter in AlgorithmManager");
        }
    }
}