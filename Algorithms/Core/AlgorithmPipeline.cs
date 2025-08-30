using System;
using System.Collections.Generic;
using System.Linq;
using LinearProgrammingSolver.Tables;
using LinearProgrammingSolver.Utils;
using LinearProgrammingSolver.Algorithms.Implementations.LP;

namespace LinearProgrammingSolver.Algorithms.Core
{
    public class AlgorithmPipeline
    {
        private readonly AlgorithmContext _context;
        
        public AlgorithmPipeline(AlgorithmContext context)
        {
            _context = context;
        }

        // Execute algorithm with automatic dependency resolution
        public AlgorithmResult ExecuteAlgorithm<T>() where T : IAlgorithm, new()
        {
            var algorithm = new T();
            
            try
            {
                // Validate problem type compatibility
                if (!algorithm.SupportedTypes.Contains(_context.ProblemType))
                {
                    return AlgorithmResult.CreateFailure($"{algorithm.Name} does not support {_context.ProblemType} problems");
                }
                
                // Ensure all prerequisites exist
                if (!EnsurePrerequisites(algorithm.RequiredTables))
                {
                    return AlgorithmResult.CreateFailure($"Failed to satisfy prerequisites for {algorithm.Name}");
                }
                
                // Execute the algorithm
                Console.WriteLine($"Executing {algorithm.Name}...");
                var result = algorithm.Execute(_context);
                
                if (result != null)
                {
                    // Update context with new result
                    if (result.Status == "Optimal")
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

        // Smart prerequisite resolution
        public bool EnsurePrerequisites(string[] requiredTables)
        {
            foreach (string tableId in requiredTables)
            {
                if (!HasTable(tableId))
                {
                    Console.WriteLine($"Missing prerequisite '{tableId}' - attempting to generate...");
                    
                    if (!GenerateMissingTable(tableId))
                    {
                        Console.WriteLine($"Failed to generate prerequisite '{tableId}'");
                        return false;
                    }
                    
                    Console.WriteLine($"Generated prerequisite '{tableId}'");
                }
            }
            return true;
        }

        // Check if table exists
        private bool HasTable(string tableId)
        {
            return TableCache.GetTable(tableId) != null;
        }

        // Intelligent table generation based on dependencies
        private bool GenerateMissingTable(string tableId)
        {
            try
            {
                switch (tableId)
                {
                    case "t-i": // Canonical form
                        return GenerateCanonicalForm();
                        
                    case "t-optimal": // Optimal solution
                        return GenerateOptimalSolution();
                        
                    default:
                        Console.WriteLine($"Unknown table ID for generation: {tableId}");
                        return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error generating {tableId}: {ex.Message}");
                return false;
            }
        }

        // Generate canonical form from raw table
        private bool GenerateCanonicalForm()
        {
            var rawTable = TableCache.GetTable("t-raw") ?? _context.RawTable;
            if (rawTable == null)
            {
                Console.WriteLine("Error: No raw table available for canonical form generation");
                return false;
            }

            var converter = new CanonicalFormConverter();
            var canonicalTable = converter.ConvertToCanonicalForm(rawTable);
            
            if (canonicalTable == null)
            {
                Console.WriteLine("Failed to convert to canonical form");
                return false;
            }

            canonicalTable.TableId = "t-i";
            canonicalTable.Status = "Canonical";
            TableCache.StoreTable(canonicalTable);
            _context.CanonicalTable = canonicalTable;
            
            return true;
        }

        // Generate optimal solution using primal simplex
        private bool GenerateOptimalSolution()
        {
            // Ensure canonical form exists first
            if (!HasTable("t-i") && !GenerateCanonicalForm())
            {
                return false;
            }

            var canonicalTable = TableCache.GetTable("t-i");
            var primalSimplex = new PrimalSimplexAlgorithm();
            
            Console.WriteLine("Solving LP with Primal Simplex to generate optimal solution...");
            var optimalTable = primalSimplex.SolveLP(canonicalTable);

            if (optimalTable == null || !optimalTable.IsOptimal())
            {
                Console.WriteLine($"Failed to solve LP optimally. Status: {optimalTable?.Status ?? "null"}");
                return false;
            }

            optimalTable.TableId = "t-optimal";
            optimalTable.Status = "Optimal";
            TableCache.StoreTable(optimalTable);
            _context.OptimalTable = optimalTable;
            
            Console.WriteLine($"LP solved with objective value: {optimalTable.GetObjectiveValue():F3}");
            return true;
        }

        // Get current context (for external access)
        public AlgorithmContext GetContext()
        {
            return _context;
        }
    }
}