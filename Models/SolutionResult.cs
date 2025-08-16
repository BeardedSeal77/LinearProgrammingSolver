// using LinearProgrammingSolver.Iterations; // Commented out - missing namespace

namespace LinearProgrammingSolver.Models
{
    public class SolutionResult
    {
        public LinearProgrammingModel SolvedModel { get; set; }
        public SolutionStatus Status { get; set; }
        public double OptimalValue { get; set; }
        public List<double> OptimalSolution { get; set; }
        // public List<IIteration> Iterations { get; set; } // Commented out - missing interface
        public TimeSpan SolutionTime { get; set; }
        public string AlgorithmUsed { get; set; }
        public string ErrorMessage { get; set; }
        public Dictionary<string, object> AdditionalInfo { get; set; }

        public SolutionResult()
        {
            // Initialize result with empty collections and default values
        }

        public bool IsSuccessful => Status == SolutionStatus.Optimal;

        public void Display()
        {
            // Display solution result to console
            // Show status, optimal value, solution vector, timing
        }

        public string GetSummary()
        {
            // Return one-line summary of solution result
        }
    }

    public class SensitivityResult
    {
        public bool IsSuccessful { get; set; }
        public string AnalysisType { get; set; }
        public double LowerBound { get; set; }
        public double UpperBound { get; set; }
        public double CurrentValue { get; set; }
        public double NewOptimalValue { get; set; }
        public List<double> NewSolution { get; set; }
        public string ErrorMessage { get; set; }
        public Dictionary<string, double> Changes { get; set; }

        public SensitivityResult()
        {
            // Initialize sensitivity result with empty collections
        }

        public void Display()
        {
            // Display sensitivity analysis results to console
            // Show ranges, new optimal values, solution changes
        }
    }

    public class DualityResult
    {
        public LinearProgrammingModel PrimalModel { get; set; }
        public LinearProgrammingModel DualModel { get; set; }
        public SolutionResult PrimalSolution { get; set; }
        public SolutionResult DualSolution { get; set; }
        public DualityType DualityType { get; set; }
        public double DualityGap { get; set; }
        public bool IsStrongDuality { get; set; }

        public DualityResult()
        {
            // Initialize duality result with default values
        }

        public void Display()
        {
            // Display duality analysis results to console
            // Show duality type, gap, primal/dual optimal values
        }
    }

    public enum DualityType
    {
        None,
        Strong,
        Weak
    }
}