using System;
using System.Collections.Generic;
using System.IO;
using LinearProgrammingSolver.Tables;
using LinearProgrammingSolver.Utils;
using LinearProgrammingSolver.Algorithms.Adapters;

namespace LinearProgrammingSolver.Algorithms.Core
{
    // Enum for algorithm selection
    public enum AlgorithmOption
    {
        PrimalSimplex = 1,
        RevisedPrimalSimplex = 2,
        BranchBoundSimplex = 3,
        BranchBoundKnapsack = 4,
        CuttingPlane = 5,
        NonLinearProgramming = 6,
        BackToMain = 7
    }

    public class AlgorithmManager
    {
        private readonly AlgorithmContext _context;
        private readonly AlgorithmPipeline _pipeline;
        
        // Registry of available algorithms
        private readonly Dictionary<AlgorithmOption, Func<IFullAlgorithm>> _algorithmRegistry;

        public AlgorithmManager(ProblemType problemType, Table rawTable, NLPProblem nlpProblem = null)
        {
            // Initialize context
            _context = new AlgorithmContext
            {
                ProblemType = problemType,
                RawTable = rawTable,
                NLPProblem = nlpProblem,
                OutputPath = GetAbsoluteOutputPath()
            };
            
            _pipeline = new AlgorithmPipeline(_context);
            
            // Register all available algorithms
            _algorithmRegistry = new Dictionary<AlgorithmOption, Func<IFullAlgorithm>>
            {
                { AlgorithmOption.PrimalSimplex, () => new PrimalSimplexAdapter() },
                { AlgorithmOption.BranchBoundSimplex, () => new BranchAndBoundAdapter() },
                { AlgorithmOption.BranchBoundKnapsack, () => new KnapsackAdapter() },
                { AlgorithmOption.CuttingPlane, () => new CuttingPlaneAdapter() },
                { AlgorithmOption.NonLinearProgramming, () => new NLPAdapter() },
                // Revised Primal Simplex uses same adapter as Primal for now
                { AlgorithmOption.RevisedPrimalSimplex, () => new PrimalSimplexAdapter() }
            };
        }

        // Updates the current problem context (for when new files are loaded)
        public void UpdateProblemContext(ProblemType problemType, Table rawTable = null, NLPProblem nlpProblem = null)
        {
            _context.ProblemType = problemType;
            _context.RawTable = rawTable;
            _context.NLPProblem = nlpProblem;
            
            // Reset derived tables when changing problems
            _context.CanonicalTable = null;
            _context.OptimalTable = null;
        }

        // Main algorithm selection menu handler
        public Table HandleAlgorithmSelection()
        {
            bool backToMain = false;
            Table lastResult = null;
            
            while (!backToMain)
            {
                DisplayAlgorithmMenu();
                
                if (int.TryParse(Console.ReadLine(), out int choice))
                {
                    if (choice >= 1 && choice <= 7)
                    {
                        var selectedOption = (AlgorithmOption)choice;
                        
                        if (selectedOption == AlgorithmOption.BackToMain)
                        {
                            backToMain = true;
                            continue;
                        }
                        
                        // Handle special cases for unsupported algorithms
                        // (Currently all algorithms are implemented)
                        
                        // Execute the selected algorithm
                        lastResult = ExecuteAlgorithm(selectedOption);
                        backToMain = true;
                    }
                    else
                    {
                        Console.WriteLine("Invalid option. Please try again.");
                    }
                }
                else
                {
                    Console.WriteLine("Invalid input. Please enter a number.");
                }
                
                if (!backToMain)
                {
                    Console.WriteLine("\nPress any key to continue...");
                    Console.ReadKey();
                    Console.Clear();
                }
            }
            
            return lastResult;
        }

        // Execute a specific algorithm using the new architecture
        private Table ExecuteAlgorithm(AlgorithmOption option)
        {
            if (!_algorithmRegistry.TryGetValue(option, out var algorithmFactory))
            {
                Console.WriteLine($"Algorithm {option} is not implemented yet.");
                return null;
            }

            try
            {
                var algorithm = algorithmFactory();
                
                // Validate problem type compatibility
                bool isSupported = false;
                foreach (var supportedType in algorithm.SupportedTypes)
                {
                    if (supportedType == _context.ProblemType)
                    {
                        isSupported = true;
                        break;
                    }
                }
                
                if (!isSupported)
                {
                    Console.WriteLine($"Error: {algorithm.Name} does not support {_context.ProblemType} problems.");
                    ShowProblemTypeError(algorithm);
                    return null;
                }

                // Clear output file and algorithm tables before execution
                var fileWriter = new FileWriter();
                fileWriter.ClearOutputFile(_context.OutputPath);
                
                // Clear only algorithm-generated tables, preserving input tables (t-raw, t-i)
                TableCache.ClearAlgorithmTables();
                
                // Display algorithm header
                algorithm.DisplayHeader();
                
                // Execute using pipeline (with automatic prerequisite resolution)
                var result = ExecuteWithPipeline(algorithm);
                
                if (result.Success)
                {
                    // Show results using algorithm-specific UI
                    algorithm.ShowResults(result.ResultTable, _context);
                    
                    // Export results using FileWriter based on algorithm type
                    ExportResultsUsingFileWriter(algorithm, result.ResultTable);
                    
                    // Display table cache summary
                    Console.WriteLine();
                    TableCache.DisplayTableSummary();
                    
                    return result.ResultTable;
                }
                else
                {
                    Console.WriteLine($"Algorithm execution failed: {result.Message}");
                    if (result.Exception != null)
                    {
                        Console.WriteLine($"Details: {result.Exception.Message}");
                    }
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error executing {option}: {ex.Message}");
                return null;
            }
        }

        // Handle unsupported algorithms
        private void HandleUnsupportedAlgorithm(string algorithmName)
        {
            if (_context.ProblemType != ProblemType.LinearProgramming)
            {
                Console.WriteLine($"Error: {algorithmName} requires a Linear Programming problem.");
                Console.WriteLine("Please load an LP/IP file or select the NLP algorithm instead.");
            }
            else
            {
                Console.WriteLine($"{algorithmName} Algorithm - Coming Soon!");
            }
        }

        // Show problem type compatibility error
        private void ShowProblemTypeError(IAlgorithm algorithm)
        {
            if (_context.ProblemType == ProblemType.NonLinearProgramming)
            {
                Console.WriteLine("The currently loaded file contains a Non-Linear Programming problem.");
                Console.WriteLine("Please load an LP/IP file (format: max/min ...) or select the NLP algorithm instead.");
            }
            else
            {
                Console.WriteLine("The currently loaded file contains a Linear Programming problem.");
                Console.WriteLine("Please load an NLP file (format: F(x,y) = ...) or select an LP/IP algorithm instead.");
            }
        }

        // Displays the algorithm selection menu
        private void DisplayAlgorithmMenu()
        {
            Console.WriteLine("╔══════════════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                         ALGORITHM SELECTION                                  ║");
            Console.WriteLine("╠══════════════════════════════════════════════════════════════════════════════╣");
            Console.WriteLine("║                                                                              ║");
            Console.WriteLine("║  1. Primal Simplex             - Standard simplex algorithm                  ║");
            Console.WriteLine("║  2. Revised Primal Simplex     - Matrix-based simplex method                 ║");
            Console.WriteLine("║  3. Branch & Bound Simplex     - Integer programming via simplex             ║");
            Console.WriteLine("║  4. Branch & Bound Knapsack    - Specialized knapsack algorithm              ║");
            Console.WriteLine("║  5. Cutting Plane Algorithm    - Integer programming via cutting planes      ║");
            Console.WriteLine("║  6. Non-Linear Programming     - Analytical NLP optimization (+10 bonus)     ║");
            Console.WriteLine("║  7. Back to Main Menu          - Return to main menu                         ║");
            Console.WriteLine("║                                                                              ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════════════════════╝");
            Console.WriteLine();
            Console.Write("Select an algorithm (1-7): ");
        }

        // Gets the current optimal table (for external access)
        public Table GetCurrentOptimalTable()
        {
            return _context.OptimalTable ?? TableCache.GetTable("t-optimal");
        }
        
        // Get the current context (for external access)
        public AlgorithmContext GetContext()
        {
            return _context;
        }

        // Export results using FileWriter based on algorithm type
        private void ExportResultsUsingFileWriter(IAlgorithm algorithm, Table resultTable)
        {
            var fileWriter = new FileWriter();
            
            try
            {
                if (algorithm.Name == "Non-Linear Programming")
                {
                    // Handle NLP results (non-TableCache) - let adapter handle it
                    ((IFullAlgorithm)algorithm).ExportResults(resultTable, _context);
                    return;
                }
                
                // Handle TableCache-based algorithms - FileWriter uses static TableCache
                fileWriter.WriteTableCacheToFile(algorithm.Name, _context.OutputPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error exporting results with FileWriter: {ex.Message}");
                // Fallback to algorithm-specific export
                try
                {
                    ((IFullAlgorithm)algorithm).ExportResults(resultTable, _context);
                }
                catch (Exception fallbackEx)
                {
                    Console.WriteLine($"Fallback export also failed: {fallbackEx.Message}");
                }
            }
        }

        // Execute algorithm with pipeline integration
        private AlgorithmResult ExecuteWithPipeline(IAlgorithm algorithm)
        {
            try
            {
                // Validate problem type compatibility
                bool isSupported = false;
                foreach (var supportedType in algorithm.SupportedTypes)
                {
                    if (supportedType == _context.ProblemType)
                    {
                        isSupported = true;
                        break;
                    }
                }
                
                if (!isSupported)
                {
                    return AlgorithmResult.CreateFailure($"{algorithm.Name} does not support {_context.ProblemType} problems");
                }
                
                // Ensure all prerequisites exist
                if (!_pipeline.EnsurePrerequisites(algorithm.RequiredTables))
                {
                    return AlgorithmResult.CreateFailure($"Failed to satisfy prerequisites for {algorithm.Name}");
                }
                
                // Execute the algorithm
                Console.WriteLine($"Executing {algorithm.Name}...");
                var result = algorithm.Execute(_context);
                
                if (result != null)
                {
                    // Update context with new result
                    if (result.Status == "Optimal" || result.Status == "Optimal_Integer")
                    {
                        _context.OptimalTable = result;
                    }
                    
                    return AlgorithmResult.CreateSuccess(result);
                }
                
                return AlgorithmResult.CreateFailure($"{algorithm.Name} returned null result");
            }
            catch (Exception ex)
            {
                return AlgorithmResult.CreateFailure($"Error executing {algorithm.Name}: {ex.Message}", ex);
            }
        }

        // Gets absolute path to output file relative to executable location
        private string GetAbsoluteOutputPath()
        {
            // Get the directory where the executable is located
            string executableDir = AppDomain.CurrentDomain.BaseDirectory;
            
            // Create data directory next to the executable
            string dataDir = Path.Combine(executableDir, "data");
            Directory.CreateDirectory(dataDir);
            
            string absolutePath = Path.Combine(dataDir, "output.txt");
            return absolutePath;
        }
    }
}