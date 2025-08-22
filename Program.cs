using LinearProgrammingSolver.Utils;
using LinearProgrammingSolver.Tables;
// using LinearProgrammingSolver.LPAlgorithms;
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
                
                // Convert to canonical form (same star pattern)
                var canonicalTable = canonicalConverter.ConvertToCanonicalForm(rawTable);
                TableCache.StoreTable(canonicalTable);
                Console.WriteLine("✓ Canonical table created and cached");
                
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