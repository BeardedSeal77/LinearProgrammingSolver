using System;
using System.IO;
using System.Linq;
using LinearProgrammingSolver.Tables;
using LinearProgrammingSolver.Utils;
using LinearProgrammingSolver.Algorithms.Implementations.IP;
using LinearProgrammingSolver.Algorithms.Core;

namespace LinearProgrammingSolver.Algorithms.Adapters
{
    public class CuttingPlaneAdapter : IFullAlgorithm
    {
        private CuttingPlaneAlgorithm _algorithm;

        public string Name => "Cutting Plane Algorithm";
        public string Description => "Integer programming via cutting planes";
        public ProblemType[] SupportedTypes => new[] { ProblemType.LinearProgramming };
        public string[] RequiredTables => new[] { "t-optimal" };

        public Table Execute(AlgorithmContext context)
        {
            _algorithm = new CuttingPlaneAlgorithm();
            var optimalTable = TableCache.GetTable("t-optimal");
            
            var result = _algorithm.SolveIP(optimalTable);
            
            // Don't store here - already stored in algorithm
            
            return result;
        }

        public void DisplayHeader()
        {
            Console.Clear();
            Console.WriteLine("╔══════════════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                      EXECUTING CUTTING PLANE ALGORITHM                       ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════════════════════╝");
            Console.WriteLine();
        }

        public void ShowResults(Table result, AlgorithmContext context)
        {
            if (_algorithm == null || result == null)
            {
                Console.WriteLine("No solution found");
                return;
            }

            Console.WriteLine($"Status: {result.Status}");
            Console.WriteLine($"Objective: {result.GetObjectiveValue():F3}");
            Console.WriteLine();
            
            Console.WriteLine("Variables:");
            for (int i = 0; i < result.BasicVariables.Count; i++)
            {
                var varName = result.BasicVariables[i];
                var value = result.GetElement(i + 1, result.GetColumnCount() - 1);
                Console.WriteLine($"  {varName} = {value:F3}");
            }
            Console.WriteLine();
            
            Console.WriteLine("Final tableau:");
            result.DisplayTraditional();
        }

        public void ExportResults(Table result, AlgorithmContext context)
        {
            if (_algorithm == null) return;
            
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(context.OutputPath));
                
                using (var writer = new StreamWriter(context.OutputPath))
                {
                    writer.WriteLine("=== CUTTING PLANE ALGORITHM RESULTS ===");
                    writer.WriteLine($"Executed at: {DateTime.Now}");
                    writer.WriteLine();
                    
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
                    
                    // Write cutting iterations (all tables with "cut" in the name)
                    writer.WriteLine("=== CUTTING PLANE ITERATIONS ===");
                    var cutTables = TableCache.GetAllTables()
                        .Where(t => t.TableId.Contains("cut") || t.TableId.Contains("gomory"))
                        .OrderBy(t => t.TableId);
                    
                    foreach (var table in cutTables)
                    {
                        writer.WriteLine($"Table {table.TableId} ({table.Status}):");
                        writer.WriteLine($"Objective Value: {table.GetObjectiveValue():F6}");
                        writer.WriteLine(table.ToString());
                        writer.WriteLine();
                    }
                    
                    // Write final solution
                    writer.WriteLine("=== FINAL SOLUTION ===");
                    if (result != null)
                    {
                        writer.WriteLine($"Table ID: {result.TableId}");
                        writer.WriteLine($"Status: {result.Status}");
                        writer.WriteLine($"Objective Value: {result.GetObjectiveValue():F6}");
                        writer.WriteLine("Basic variables and values:");
                        for (int i = 0; i < result.BasicVariables.Count; i++)
                        {
                            var varName = result.BasicVariables[i];
                            var value = result.GetElement(i + 1, result.GetColumnCount() - 1);
                            writer.WriteLine($"  {varName} = {value:F6}");
                        }
                        writer.WriteLine();
                        writer.WriteLine("Final Table:");
                        writer.WriteLine(result.ToString());
                        writer.WriteLine();
                    }
                    else
                    {
                        writer.WriteLine("No solution found!");
                    }
                    
                    // Algorithm summary
                    writer.WriteLine("=== ALGORITHM SUMMARY ===");
                    writer.WriteLine("Method: Iterative Gomory Cutting Plane Algorithm");
                    writer.WriteLine("Approach: Generate cutting planes to eliminate fractional solutions");
                    writer.WriteLine("Termination: Early stop on no improvement or maximum cuts reached");
                }
                
                Console.WriteLine($"Cutting Plane results exported to {context.OutputPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error exporting Cutting Plane results: {ex.Message}");
            }
        }
    }
}