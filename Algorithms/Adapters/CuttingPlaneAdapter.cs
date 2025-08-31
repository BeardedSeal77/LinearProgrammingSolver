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
            // Export is now handled by FileWriter in AlgorithmManager
            // This method serves as a fallback only
            Console.WriteLine("Export handled by FileWriter in AlgorithmManager");
        }
    }
}