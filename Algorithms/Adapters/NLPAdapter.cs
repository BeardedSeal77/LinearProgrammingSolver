using System;
using System.IO;
using LinearProgrammingSolver.Tables;
using LinearProgrammingSolver.Utils;
using LinearProgrammingSolver.Algorithms.Implementations.NLP;
using LinearProgrammingSolver.Algorithms.Core;

namespace LinearProgrammingSolver.Algorithms.Adapters
{
    public class NLPAdapter : IFullAlgorithm
    {
        private NLPProblem _result;

        public string Name => "Non-Linear Programming";
        public string Description => "Analytical NLP optimization (+10 bonus)";
        public ProblemType[] SupportedTypes => new[] { ProblemType.NonLinearProgramming };
        public string[] RequiredTables => new string[0]; // No table prerequisites for NLP

        public Table Execute(AlgorithmContext context)
        {
            if (context.NLPProblem == null)
            {
                throw new InvalidOperationException("No NLP problem data available");
            }

            Console.WriteLine("NLP Problem Details:");
            Console.WriteLine($"Function: {context.NLPProblem.Function}");
            Console.WriteLine($"Starting Point: ({context.NLPProblem.StartingPoint.x}, {context.NLPProblem.StartingPoint.y})");
            Console.WriteLine();
            
            var nlpAlgorithm = new NLPAlgorithm();
            _result = nlpAlgorithm.SolveNLP(context.NLPProblem);
            
            // NLP doesn't return a Table, so we return null but store the result internally
            // The ShowResults method will handle displaying the NLP-specific results
            return null;
        }

        public void DisplayHeader()
        {
            Console.Clear();
            Console.WriteLine("╔══════════════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                   EXECUTING NON-LINEAR PROGRAMMING                           ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════════════════════╝");
            Console.WriteLine();
        }

        public void ShowResults(Table result, AlgorithmContext context)
        {
            if (_result == null)
            {
                Console.WriteLine("NLP algorithm failed to produce results.");
                return;
            }

            Console.WriteLine();
            Console.WriteLine("=== NLP OPTIMIZATION COMPLETE ===");
            
            // Display the NLP-specific results
            var nlpAlgorithm = new NLPAlgorithm();
            nlpAlgorithm.DisplayResults(_result);
        }

        public void ExportResults(Table result, AlgorithmContext context)
        {
            // NLP export is handled by FileWriter in AlgorithmManager
            // This method provides NLP-specific export functionality
            if (_result == null || context.NLPProblem == null)
            {
                Console.WriteLine("No NLP results to export.");
                return;
            }
            
            try
            {
                var fileWriter = new FileWriter();
                fileWriter.WriteNLPResults(context.NLPProblem, _result, context.OutputPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error exporting NLP results with FileWriter: {ex.Message}");
                // Fallback export for NLP
                try
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(context.OutputPath));
                    
                    using (var writer = new StreamWriter(context.OutputPath))
                    {
                        writer.WriteLine("=== NON-LINEAR PROGRAMMING RESULTS (FALLBACK) ===");
                        writer.WriteLine($"Executed at: {DateTime.Now}");
                        writer.WriteLine();
                        writer.WriteLine($"Critical Point: ({_result.OptimalPoint.x:F6}, {_result.OptimalPoint.y:F6})");
                        writer.WriteLine($"Function Value: {_result.OptimalValue:F6}");
                    }
                    Console.WriteLine($"NLP results exported to {context.OutputPath} (fallback mode)");
                }
                catch (Exception fallbackEx)
                {
                    Console.WriteLine($"Fallback NLP export also failed: {fallbackEx.Message}");
                }
            }
        }
    }
}