using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using LinearProgrammingSolver.Utils;
using LinearProgrammingSolver.Tables;
using LinearProgrammingSolver.LPAlgorithms;
// using LinearProgrammingSolver.IPAlgorithms;

namespace LinearProgrammingSolver
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== Linear Programming Solver ===");
            Console.WriteLine();
            
            try
            {
                // Clear any existing tables from previous runs
                TableCache.ClearAllTables();
                
                // Star/Tree Topology: Program.cs coordinates all object creation
                var fileReader = new FileReader();
                var canonicalConverter = new CanonicalFormConverter();
                
                // Use robust path resolution that works from any working directory
                string currentDir = Directory.GetCurrentDirectory();
                string projectDir = currentDir;
                
                // If running from debugger, find the project directory
                while (!File.Exists(Path.Combine(projectDir, "data", "input.txt")) && 
                       Directory.GetParent(projectDir) != null)
                {
                    projectDir = Directory.GetParent(projectDir).FullName;
                }
                
                string inputPath = Path.Combine(projectDir, "data", "input.txt");
                Console.WriteLine($"Processing: {inputPath}");
                
                // Step 1: FileReader ONLY parses (no table construction)
                var (matrix, rowLabels, columnLabels, optimizationType, constraintOperators) = fileReader.ParseFile(inputPath);
                
                // Step 2: Program.cs constructs Table object
                var rawTable = new Table("t-raw", matrix, rowLabels, columnLabels, optimizationType, "Raw", constraintOperators);
                
                // Step 3: Program.cs stores Table in cache
                TableCache.StoreTable(rawTable);
                Console.WriteLine("✓ Raw table created and cached");
                
                // Step 4: Convert to canonical form (same star pattern)
                var canonicalTable = canonicalConverter.ConvertToCanonicalForm(rawTable);
                TableCache.StoreTable(canonicalTable); //table type "t-i"
                Console.WriteLine("✓ Canonical table created and cached");

                // Step 5: Primal Simplex
                //The only problem I have is that PrimalSimplexALgorithm is underlined in red, as well as "using LinearProgrammingSolver.LPAlgorithms;" when uncommented at the top of the file.

                var simplexSolver = new PrimalSimplexAlgorithm();
                var initialTable = TableCache.GetTable("t-i");  
                if (initialTable == null)
                {
                    Console.WriteLine("Error: Canonical table (t-i) not found in TableCache.");
                    return;
                }
                var optimalTable = simplexSolver.SolveLP(initialTable);
                Console.WriteLine("✓ Simplex algorithm completed");

                // Export to output.txt (project requirement, F3 rounding)
                using (StreamWriter writer = new StreamWriter("output.txt"))
                {
                    writer.WriteLine("Canonical Form:");
                    writer.WriteLine(initialTable.ToString());
                    foreach (var table in TableCache.GetAllTables().Where(t => t.Status == "Iteration" || t.Status == "Optimal" || t.Status == "Infeasible" || t.Status == "Unbounded"))
                    {
                        writer.WriteLine($"\nTable {table.TableId} ({table.Status}):");
                        writer.WriteLine(table.ToString());
                    }
                }
                Console.WriteLine("Output exported to output.txt");

               
                // Display final summary of all cached tables
                Console.WriteLine();
                TableCache.DisplayTableSummary();
                
                // Uncomment the line below to see detailed view of all tables:
                TableCache.DisplayAllTablesDetailed();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            
            Console.WriteLine("\nProgram completed.");
        }
    }
}