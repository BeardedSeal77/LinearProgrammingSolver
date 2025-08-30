using System;
using LinearProgrammingSolver.Tables;
using LinearProgrammingSolver.Utils;

namespace LinearProgrammingSolver.Algorithms.Core
{
    // Context passed to algorithms containing all necessary data
    public class AlgorithmContext
    {
        public Table RawTable { get; set; }
        public Table CanonicalTable { get; set; }
        public Table OptimalTable { get; set; }
        public NLPProblem NLPProblem { get; set; }
        public ProblemType ProblemType { get; set; }
        public string OutputPath { get; set; } = "data/output.txt";
    }

    // Core algorithm interface
    public interface IAlgorithm
    {
        string Name { get; }
        string Description { get; }
        ProblemType[] SupportedTypes { get; }
        string[] RequiredTables { get; }
        
        Table Execute(AlgorithmContext context);
    }

    // UI responsibilities for each algorithm
    public interface IAlgorithmUI
    {
        void DisplayHeader();
        void ShowResults(Table result, AlgorithmContext context);
        void ExportResults(Table result, AlgorithmContext context);
    }

    // Combined interface for algorithms that handle their own UI
    public interface IFullAlgorithm : IAlgorithm, IAlgorithmUI
    {
    }

    // Algorithm execution result
    public class AlgorithmResult
    {
        public Table ResultTable { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
        public Exception Exception { get; set; }

        public static AlgorithmResult CreateSuccess(Table table, string message = "Algorithm completed successfully")
        {
            return new AlgorithmResult { Success = true, ResultTable = table, Message = message };
        }

        public static AlgorithmResult CreateFailure(string message, Exception ex = null)
        {
            return new AlgorithmResult { Success = false, Message = message, Exception = ex };
        }
    }
}