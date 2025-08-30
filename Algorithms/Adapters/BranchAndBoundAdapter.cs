using System;
using System.IO;
using System.Linq;
using LinearProgrammingSolver.Tables;
using LinearProgrammingSolver.Utils;
using LinearProgrammingSolver.Algorithms.Implementations.IP;
using LinearProgrammingSolver.Algorithms.Core;

namespace LinearProgrammingSolver.Algorithms.Adapters
{
    public class BranchAndBoundAdapter : IFullAlgorithm
    {
        private BranchAndBoundAlgorithm _algorithm;

        public string Name => "Branch & Bound Simplex";
        public string Description => "Integer programming via branch and bound with simplex";
        public ProblemType[] SupportedTypes => new[] { ProblemType.LinearProgramming };
        public string[] RequiredTables => new[] { "t-i", "t-optimal" };

        public Table Execute(AlgorithmContext context)
        {
            _algorithm = new BranchAndBoundAlgorithm();
            var optimalTable = TableCache.GetTable("t-optimal");
            
            Console.WriteLine($"Using optimal LP solution with objective value: {optimalTable.GetObjectiveValue():F3}");
            Console.WriteLine();
            
            Console.WriteLine("Starting Branch & Bound Integer Programming...");
            Console.WriteLine();
            
            var result = _algorithm.SolveIP(optimalTable);
            
            if (result != null)
            {
                result.TableId = "bb-final";
                TableCache.StoreTable(result);
            }
            
            return result;
        }

        public void DisplayHeader()
        {
            Console.Clear();
            Console.WriteLine("╔══════════════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                      EXECUTING BRANCH & BOUND SIMPLEX                        ║");
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

            // Display processing log
            Console.WriteLine("=== BRANCH & BOUND PROCESSING LOG ===");
            var processingOrder = _algorithm.GetProcessingOrder();
            foreach (var logEntry in processingOrder)
            {
                Console.WriteLine(logEntry);
            }
            
            // Display fathoming reasons
            Console.WriteLine("\n=== FATHOMING REASONS ===");
            var fathomReasons = _algorithm.GetFathomReasons();
            foreach (var kvp in fathomReasons)
            {
                Console.WriteLine($"{kvp.Key}: {kvp.Value}");
            }
            
            // Display best integer solution
            Console.WriteLine("\n=== BEST INTEGER SOLUTION ===");
            if (result != null)
            {
                Console.WriteLine("Integer solution found!");
                Console.WriteLine($"Table ID: {result.TableId}");
                Console.WriteLine($"Objective Value: {result.GetObjectiveValue():F3}");
                Console.WriteLine("Basic variables and values:");
                for (int i = 0; i < result.BasicVariables.Count; i++)
                {
                    var varName = result.BasicVariables[i];
                    var value = result.GetElement(i + 1, result.GetColumnCount() - 1);
                    Console.WriteLine($"  {varName} = {value:F3}");
                }
                
                Console.WriteLine("\nFinal Tableau:");
                result.DisplayTraditional();
            }
            else
            {
                Console.WriteLine("No integer solution found!");
            }
            
            // Display summary
            var allSubproblems = _algorithm.GetAllSubproblems();
            Console.WriteLine($"\n=== SUMMARY ===");
            Console.WriteLine($"Total subproblems generated: {allSubproblems.Count}");
            Console.WriteLine($"Processing steps: {processingOrder.Count}");
            Console.WriteLine($"Fathomed nodes: {fathomReasons.Count}");
        }

        public void ExportResults(Table result, AlgorithmContext context)
        {
            if (_algorithm == null) return;
            
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(context.OutputPath));
                
                using (var writer = new StreamWriter(context.OutputPath))
                {
                    writer.WriteLine("=== BRANCH & BOUND SIMPLEX RESULTS ===");
                    writer.WriteLine($"Executed at: {DateTime.Now}");
                    writer.WriteLine();
                    
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
                    var processingOrder = _algorithm.GetProcessingOrder();
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
                    var fathomReasons = _algorithm.GetFathomReasons();
                    foreach (var kvp in fathomReasons)
                    {
                        writer.WriteLine($"{kvp.Key}: {kvp.Value}");
                    }
                    writer.WriteLine();
                    
                    // Write best integer solution
                    var bestSolution = _algorithm.GetBestIntegerSolution();
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
                    var allSubproblems = _algorithm.GetAllSubproblems();
                    writer.WriteLine();
                    writer.WriteLine("=== SUMMARY ===");
                    writer.WriteLine($"Total subproblems generated: {allSubproblems.Count}");
                    writer.WriteLine($"Processing steps: {processingOrder.Count}");
                    writer.WriteLine($"Fathomed nodes: {fathomReasons.Count}");
                }
                
                Console.WriteLine($"Branch & Bound results exported to {context.OutputPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error exporting Branch & Bound results: {ex.Message}");
            }
        }
    }
}