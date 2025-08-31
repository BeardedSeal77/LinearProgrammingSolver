using System;
using System.IO;
using System.Linq;
using LinearProgrammingSolver.Tables;
using LinearProgrammingSolver.Utils;
using LinearProgrammingSolver.Algorithms.Implementations.LP;
using LinearProgrammingSolver.Algorithms.Core;

namespace LinearProgrammingSolver.Algorithms.Adapters
{
    public class PrimalSimplexAdapter : IFullAlgorithm
    {
        public string Name => "Primal Simplex";
        public string Description => "Standard simplex algorithm for linear programming";
        public ProblemType[] SupportedTypes => new[] { ProblemType.LinearProgramming };
        public string[] RequiredTables => new[] { "t-i" };

        public Table Execute(AlgorithmContext context)
        {
            var simplexSolver = new PrimalSimplexAlgorithm();
            var canonicalTable = TableCache.GetTable("t-i");
            
            Console.WriteLine("Starting Primal Simplex Algorithm...");
            Console.WriteLine();
            
            var result = simplexSolver.SolveLP(canonicalTable);
            
            if (result != null)
            {
                result.TableId = "t-optimal";
                TableCache.StoreTable(result);
            }
            
            return result;
        }

        public void DisplayHeader()
        {
            Console.Clear();
            Console.WriteLine("╔══════════════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                        EXECUTING PRIMAL SIMPLEX                              ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════════════════════╝");
            Console.WriteLine();
        }

        public void ShowResults(Table result, AlgorithmContext context)
        {
            if (result == null)
            {
                Console.WriteLine("Algorithm failed to produce a result.");
                return;
            }

            Console.WriteLine("Primal Simplex Algorithm completed successfully!");
            Console.WriteLine();
            
            Console.WriteLine($"Final Status: {result.Status}");
            
            switch (result.Status)
            {
                case "Optimal":
                    Console.WriteLine("Optimal solution found!");
                    Console.WriteLine($"Objective Value: {result.GetObjectiveValue():F6}");
                    
                    Console.WriteLine("\nBasic Variables:");
                    for (int i = 0; i < result.BasicVariables.Count; i++)
                    {
                        var varName = result.BasicVariables[i];
                        var value = result.GetElement(i + 1, result.GetColumnCount() - 1);
                        Console.WriteLine($"  {varName} = {value:F6}");
                    }
                    break;
                    
                case "Infeasible":
                    Console.WriteLine("Problem is infeasible - no solution exists.");
                    break;
                    
                case "Unbounded":
                    Console.WriteLine("Problem is unbounded - objective can be improved indefinitely.");
                    break;
            }
            
            Console.WriteLine();
            Console.WriteLine("Final Tableau:");
            result.DisplayTraditional();
        }

        public void ExportResults(Table result, AlgorithmContext context)
        {
            // Export is now handled by FileWriter in AlgorithmManager
            // This method serves as a fallback only
            Console.WriteLine("Export handled by FileWriter in AlgorithmManager");
        }
    }
}