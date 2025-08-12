using LinearProgrammingSolver.Models;
using LinearProgrammingSolver.Utils;
using LinearProgrammingSolver.Tables;
using LinearProgrammingSolver.LPAlgorithms;
using LinearProgrammingSolver.IPAlgorithms;

namespace LinearProgrammingSolver
{
    class Program
    {
        static void Main(string[] args)
        {
            // Main entry point for Linear Programming Solver
            // Display welcome message and start application
            // Handle any top-level exceptions gracefully
        }
    }

    public class LinearProgrammingSolverApp
    {
        private LinearProgrammingModel _model;
        private FileReader _fileReader;
        private FileWriter _fileWriter;
        private List<Table> _allTables;
        private Table _currentTable;
        private Table _lpOptimalTable;

        // Algorithm instances
        private PrimalSimplexAlgorithm _primalSimplex;
        private RevisedSimplexAlgorithm _revisedSimplex;
        private BranchAndBoundAlgorithm _branchAndBound;
        private BranchAndBoundKnapsackAlgorithm _branchAndBoundKnapsack;
        private CuttingPlaneAlgorithm _cuttingPlane;

        public LinearProgrammingSolverApp()
        {
            // Initialize application with file I/O utilities
            // Create algorithm instances
            // Initialize table collections
        }

        public void Run()
        {
            // Start the main application loop
        }

        private void ShowMainMenu()
        {
            // Display main menu and handle user choices
            // 1. Load Model from File
            // 2. Solve LP (Primal or Revised Simplex)
            // 3. Solve IP (Branch & Bound or Cutting Plane)
            // 4. Display All Tables
            // 5. Export Results
            // 6. Exit
        }

        private void LoadModelFromFile()
        {
            // Get file path from user input
            // Use FileReader to parse input file
            // Create initial canonical form table (t-i)
        }

        private void SolveLP()
        {
            // Show LP algorithm selection menu (Primal vs Revised Simplex)
            // Solve LP starting from t-i to get t-optimal
            // Store LP optimal solution for IP algorithms
        }

        private void SolveIP()
        {
            // Show IP algorithm selection menu
            // Start from LP optimal and solve IP
            // Create branch and bound tables (t-1.1, t-1.2, etc.)
        }

        private void SolvePrimalSimplex()
        {
            // Solve using Primal Simplex: t-i → t-1 → t-2 → ... → t-optimal
        }

        private void SolveRevisedSimplex()
        {
            // Solve using Revised Simplex: t-i → t-rev-1 → t-rev-2 → ... → t-optimal
        }

        private void SolveBranchAndBound()
        {
            // Solve using Branch & Bound: t-optimal → t-1.1, t-1.2 → t-1.1.1, t-1.1.2, etc.
        }

        private void SolveBranchAndBoundKnapsack()
        {
            // Solve using Branch & Bound Knapsack algorithm
        }

        private void SolveCuttingPlane()
        {
            // Solve using Cutting Plane: t-optimal → t-cut-1 → t-cut-2 → ... → t-optimal-ip
        }

        private void DisplayAllTables()
        {
            // Display all tables created during solution process
            // Show progression: t-i → t-1 → t-optimal → t-1.1, t-1.2, etc.
        }

        private void ExportResults()
        {
            // Get output file path from user
            // Use FileWriter to export all tables and results
            // Include canonical form and all iterations
        }
    }

    public enum LPAlgorithmType
    {
        PrimalSimplex,
        RevisedSimplex
    }

    public enum IPAlgorithmType
    {
        BranchAndBoundSimplex,
        BranchAndBoundKnapsack,
        CuttingPlane
    }
}