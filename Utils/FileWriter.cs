using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.Json;
using LinearProgrammingSolver.Tables;
// using LinearProgrammingSolver.Iterations; // Commented out - missing namespace

namespace LinearProgrammingSolver.Utils
{
    public class FileWriter
 {
        // Clears the output file before writing new results
        public void ClearOutputFile(string outputPath)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
                File.WriteAllText(outputPath, string.Empty);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Could not clear output file {outputPath}: {ex.Message}");
            }
        }

        // Main entry: writes the full solution result to a text file.
        // Includes canonical form, all iterations, and final solution.
        // Note: This method is kept for compatibility but may not be used in current architecture
        public void WriteResultToFile(string algorithmName, string outputPath)
        {
            // Write complete solution result to output text file
            using (var writer = new StreamWriter(outputPath))
            {
                var tables = TableCache.GetAllTables()
                    .OrderBy(t => t.CreatedTime)
                    .ThenBy(t => GetTableOrder(t.TableId))
                    .ToList();

                if (!tables.Any())
                {
                    writer.WriteLine("No tables available in TableCache.");
                    return;
                }

                // Write canonical form
                WriteCanonicalForm(algorithmName, writer);

                // Write all iterations
                var iterations = tables.Where(t => t.TableId.StartsWith("t-") && !t.TableId.Equals("t-optimal")).ToList();
                WriteIterations(iterations, writer);

                // Write all other tables (excluding canonical and iterations)
                var otherTables = tables.Where(t => !t.TableId.Equals("t-canonical") && !t.TableId.StartsWith("t-")).ToList();
                foreach (var table in otherTables)
                {
                    writer.WriteLine($"\n=== Table: {table.TableId} ===");
                    writer.WriteLine($"Status: {table.Status}");
                    writer.WriteLine($"Optimization: {table.OptimizationType}");

                    if (table.BasicVariables?.Count > 0)
                    {
                        writer.WriteLine($"Basic Variables: [{string.Join(", ", table.BasicVariables)}]");
                    }

                    writer.WriteLine("\nTableau Matrix:");
                    writer.Write("      ");
                    foreach (var colLabel in table.ColumnLabels)
                    {
                        if (colLabel == "RHS")
                        {
                            writer.Write($"{"Op",4}{"RHS",8}");
                        }
                        else
                        {
                            writer.Write($"{colLabel,8}");
                        }
                    }
                    writer.WriteLine();
                    writer.Write("      ");
                    for (int j = 0; j < table.ColumnLabels.Count; j++)
                    {
                        if (table.ColumnLabels[j] == "RHS")
                        {
                            writer.Write("------------");
                        }
                        else
                        {
                            writer.Write("--------");
                        }
                    }
                    writer.WriteLine();

                    for (int i = 0; i < table.GetRowCount(); i++)
                    {
                        string rowLabel = i < table.RowLabels.Count ? table.RowLabels[i] : $"R{i}";
                        writer.Write($"{rowLabel,5} |");
                        for (int j = 0; j < table.GetColumnCount(); j++)
                        {
                            if (j == table.GetColumnCount() - 1) // RHS column
                            {
                                string operatorSymbol = "=";  // Default operator symbol
                                writer.Write($"{operatorSymbol,4}");
                                writer.Write($"{FormatDecimal(table.GetElement(i, j)),8}");
                            }
                            else
                            {
                                writer.Write($"{FormatDecimal(table.GetElement(i, j)),8}");
                            }
                        }
                        writer.WriteLine();
                    }

                    writer.WriteLine($"Objective Value: {FormatDecimal(table.GetObjectiveValue())}");
                    writer.WriteLine(new string('-', 80));
                }

                // Write solution summary at the end
                WriteSolutionSummary(writer);
            }
        }

        // Simple method: replicates "Detailed Tables View" to file
        public void WriteTableCacheToFile(string algorithmName, string outputPath)
        {
            try
            {
                using (var writer = new StreamWriter(outputPath, append: false))
                {
                    writer.WriteLine($"=== {algorithmName.ToUpper()} RESULTS ===");
                    writer.WriteLine($"Executed at: {DateTime.Now}");
                    writer.WriteLine();
                    writer.WriteLine("=== COMPLETE TABLE CACHE CONTENTS ===");
                    
                    var tables = TableCache.GetAllTables()
                        .OrderBy(t => t.CreatedTime)
                        .ThenBy(t => GetTableOrder(t.TableId))
                        .ToList();

                    if (!tables.Any())
                    {
                        writer.WriteLine("No tables cached.");
                        return;
                    }

                    writer.WriteLine($"Total Tables in Cache: {tables.Count}");
                    writer.WriteLine();

                    // Write each table exactly like DisplayAllTablesDetailed does
                    for (int i = 0; i < tables.Count; i++)
                    {
                        var table = tables[i];
                        
                        // Capture the table's DisplayTraditional output to string
                        var tableOutput = CaptureTableDisplay(table);
                        writer.Write(tableOutput);
                        
                        // Add separator between tables (except after the last one)
                        if (i < tables.Count - 1)
                        {
                            writer.WriteLine();
                            writer.WriteLine(new string('=', 80));
                            writer.WriteLine();
                        }
                    }

                    writer.WriteLine();
                    writer.WriteLine("=== END OF TABLE CACHE ===");
                }
                
                Console.WriteLine($"Results exported to {outputPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error exporting results: {ex.Message}");
            }
        }

        // Helper method to capture table display output as string
        private string CaptureTableDisplay(Table table)
        {
            // Use StringWriter to capture console output
            using (var stringWriter = new StringWriter())
            {
                var originalOut = Console.Out;
                try
                {
                    Console.SetOut(stringWriter);
                    table.DisplayTraditional();
                    return stringWriter.ToString();
                }
                catch (Exception ex)
                {
                    return $"Error displaying table {table.TableId}: {ex.Message}\n";
                }
                finally
                {
                    Console.SetOut(originalOut);
                }
            }
        }

        // Method for NLP results (non-TableCache algorithms)
        public void WriteNLPResults(NLPProblem problem, NLPProblem result, string outputPath)
        {
            try
            {
                using (var writer = new StreamWriter(outputPath, append: false))
                {
                    writer.WriteLine("=== NON-LINEAR PROGRAMMING RESULTS ===");
                    writer.WriteLine($"Executed at: {DateTime.Now}");
                    writer.WriteLine();
                    
                    writer.WriteLine("=== PROBLEM DEFINITION ===");
                    writer.WriteLine($"Function: {problem.Function}");
                    writer.WriteLine($"Starting Point: ({problem.StartingPoint.x}, {problem.StartingPoint.y})");
                    writer.WriteLine();
                    
                    if (result != null)
                    {
                        writer.WriteLine("=== SOLUTION ===");
                        writer.WriteLine($"Critical Point: ({result.OptimalPoint.x:F6}, {result.OptimalPoint.y:F6})");
                        writer.WriteLine($"Function Value: {result.OptimalValue:F6}");
                        writer.WriteLine($"Classification: {result.PointType}");
                        writer.WriteLine();
                        
                        writer.WriteLine("=== ANALYSIS ===");
                        writer.WriteLine($"Gradient at optimal: ({result.Dx:F6}, {result.Dy:F6})");
                        
                        if (result.HessianMatrix != null)
                        {
                            writer.WriteLine("Hessian matrix:");
                            writer.WriteLine($"  [{result.Dxx:F6}  {result.Dxy:F6}]");
                            writer.WriteLine($"  [{result.Dyx:F6}  {result.Dyy:F6}]");
                            writer.WriteLine($"Determinant: {result.HessianDeterminant:F6}");
                            writer.WriteLine();
                        }
                        
                        writer.WriteLine("=== ALGORITHM SUMMARY ===");
                        writer.WriteLine("Method: Analytical optimization using calculus");
                        writer.WriteLine("Approach: Find critical points via gradient analysis");
                        writer.WriteLine("Classification: Second derivative test for optimization type");
                    }
                    else
                    {
                        writer.WriteLine("=== ERROR ===");
                        writer.WriteLine("NLP algorithm failed to produce results.");
                    }
                }
                
                Console.WriteLine($"NLP results exported to {outputPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error exporting NLP results: {ex.Message}");
            }
        }

        // Method for Analysis results
        public void WriteAnalysisResults(string analysisType, string content, string outputPath)
        {
            try
            {
                using (var writer = new StreamWriter(outputPath, append: false))
                {
                    writer.WriteLine($"=== {analysisType.ToUpper()} RESULTS ===");
                    writer.WriteLine($"Executed at: {DateTime.Now}");
                    writer.WriteLine();
                    writer.WriteLine(content);
                }
                
                Console.WriteLine($"{analysisType} results exported to {outputPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error exporting {analysisType} results: {ex.Message}");
            }
        }

        private void WriteTableToStream(Table table, StreamWriter writer)
        {
            writer.WriteLine($"=== {table.TableId} ===");
            writer.WriteLine($"Status: {table.Status}");
            writer.WriteLine($"Optimization: {table.OptimizationType}");

            if (table.BasicVariables?.Count > 0)
            {
                writer.WriteLine($"Basic Variables: [{string.Join(", ", table.BasicVariables)}]");
            }

            writer.WriteLine();
            writer.WriteLine("Tableau Matrix:");
            writer.Write("      ");
            foreach (var colLabel in table.ColumnLabels)
            {
                if (colLabel == "RHS")
                {
                    writer.Write($"{"Op",4}{"RHS",8}");
                }
                else
                {
                    writer.Write($"{colLabel,8}");
                }
            }
            writer.WriteLine();
            writer.Write("      ");
            for (int j = 0; j < table.ColumnLabels.Count; j++)
            {
                if (table.ColumnLabels[j] == "RHS")
                {
                    writer.Write("------------");
                }
                else
                {
                    writer.Write("--------");
                }
            }
            writer.WriteLine();

            for (int i = 0; i < table.GetRowCount(); i++)
            {
                string rowLabel = i < table.RowLabels.Count ? table.RowLabels[i] : $"R{i}";
                writer.Write($"{rowLabel,5} |");
                for (int j = 0; j < table.GetColumnCount(); j++)
                {
                    if (j == table.GetColumnCount() - 1) // RHS column
                    {
                        string operatorSymbol = "=";  // Default operator symbol
                        writer.Write($"{operatorSymbol,4}");
                        writer.Write($"{FormatDecimal(table.GetElement(i, j)),8}");
                    }
                    else
                    {
                        writer.Write($"{FormatDecimal(table.GetElement(i, j)),8}");
                    }
                }
                writer.WriteLine();
            }

            writer.WriteLine($"Objective Value: {FormatDecimal(table.GetObjectiveValue())}");
            writer.WriteLine(new string('-', 80));
            writer.WriteLine();
        }

        private void WriteSolutionSummaryToStream(Table optimalTable, StreamWriter writer)
        {
            writer.WriteLine("=== SOLUTION SUMMARY ===");
            writer.WriteLine($"Optimal Value: {FormatDecimal(optimalTable.GetObjectiveValue())}");
            var varValues = new Dictionary<string, double>();
            int rhsCol = optimalTable.GetColumnCount() - 1;
            for (int i = 0; i < optimalTable.GetRowCount() - 1; i++)
            {
                string var = optimalTable.BasicVariables?[i] ?? $"x{i + 1}";
                varValues[var] = optimalTable.GetElement(i + 1, rhsCol);
            }
            foreach (var kvp in varValues)
            {
                writer.WriteLine($"{kvp.Key}: {FormatDecimal(kvp.Value)}");
            }
            writer.WriteLine();
        }

        private void WriteCanonicalForm(string algorithmName, StreamWriter writer)
        {
            // Write canonical form of the problem to file
            var canonicalTable = TableCache.GetTable("t-canonical") ?? TableCache.GetTable("t-i");
            if (canonicalTable != null)
            {
                writer.WriteLine($"\n=== Canonical Form: {canonicalTable.TableId} ===");
                writer.WriteLine($"Status: {canonicalTable.Status}");
                writer.WriteLine($"Optimization: {canonicalTable.OptimizationType}");

                if (canonicalTable.BasicVariables?.Count > 0)
                {
                    writer.WriteLine($"Basic Variables: [{string.Join(", ", canonicalTable.BasicVariables)}]");
                }

                writer.WriteLine("\nTableau Matrix:");
                writer.Write("      ");
                foreach (var colLabel in canonicalTable.ColumnLabels)
                {
                    if (colLabel == "RHS")
                    {
                        writer.Write($"{"Op",4}{"RHS",8}");
                    }
                    else
                    {
                        writer.Write($"{colLabel,8}");
                    }
                }
                writer.WriteLine();
                writer.Write("      ");
                for (int j = 0; j < canonicalTable.ColumnLabels.Count; j++)
                {
                    if (canonicalTable.ColumnLabels[j] == "RHS")
                    {
                        writer.Write("------------");
                    }
                    else
                    {
                        writer.Write("--------");
                    }
                }
                writer.WriteLine();

                for (int i = 0; i < canonicalTable.GetRowCount(); i++)
                {
                    string rowLabel = i < canonicalTable.RowLabels.Count ? canonicalTable.RowLabels[i] : $"R{i}";
                    writer.Write($"{rowLabel,5} |");
                    for (int j = 0; j < canonicalTable.GetColumnCount(); j++)
                    {
                        if (j == canonicalTable.GetColumnCount() - 1) // RHS column
                        {
                            string operatorSymbol = "=";  // Default operator symbol
                            writer.Write($"{operatorSymbol,4}");
                            writer.Write($"{FormatDecimal(canonicalTable.GetElement(i, j)),8}");
                        }
                        else
                        {
                            writer.Write($"{FormatDecimal(canonicalTable.GetElement(i, j)),8}");
                        }
                    }
                    writer.WriteLine();
                }
                writer.WriteLine($"Objective Value: {FormatDecimal(canonicalTable.GetObjectiveValue())}");
                writer.WriteLine(new string('-', 80));
            }
        }

        private void WriteIterations(List<Table> iterations, StreamWriter writer)
        {
            // Write all algorithm iterations to file
            // Handle different iteration types appropriately
            if (iterations == null || !iterations.Any())
            {
                writer.WriteLine("\n=== No Iterations Available ===");
                writer.WriteLine(new string('-', 80));
                return;
            }

            writer.WriteLine("\n=== Algorithm Iterations ===");
            foreach (var iteration in iterations)
            {
                writer.WriteLine($"\n--- Iteration: {iteration.TableId} ---");
                writer.WriteLine($"Status: {iteration.Status}");
                writer.WriteLine($"Optimization: {iteration.OptimizationType}");

                if (iteration.BasicVariables?.Count > 0)
                {
                    writer.WriteLine($"Basic Variables: [{string.Join(", ", iteration.BasicVariables)}]");
                }

                writer.WriteLine("\nTableau Matrix:");
                writer.Write("      ");
                foreach (var colLabel in iteration.ColumnLabels)
                {
                    if (colLabel == "RHS")
                    {
                        writer.Write($"{"Op",4}{"RHS",8}");
                    }
                    else
                    {
                        writer.Write($"{colLabel,8}");
                    }
                }
                writer.WriteLine();
                writer.Write("      ");
                for (int j = 0; j < iteration.ColumnLabels.Count; j++)
                {
                    if (iteration.ColumnLabels[j] == "RHS")
                    {
                        writer.Write("------------");
                    }
                    else
                    {
                        writer.Write("--------");
                    }
                }
                writer.WriteLine();

                for (int i = 0; i < iteration.GetRowCount(); i++)
                {
                    string rowLabel = i < iteration.RowLabels.Count ? iteration.RowLabels[i] : $"R{i}";
                    writer.Write($"{rowLabel,5} |");
                    for (int j = 0; j < iteration.GetColumnCount(); j++)
                    {
                        if (j == iteration.GetColumnCount() - 1) // RHS column
                        {
                            string operatorSymbol = "=";  // Default operator symbol
                            writer.Write($"{operatorSymbol,4}");
                            writer.Write($"{FormatDecimal(iteration.GetElement(i, j)),8}");
                        }
                        else
                        {
                            writer.Write($"{FormatDecimal(iteration.GetElement(i, j)),8}");
                        }
                    }
                    writer.WriteLine();
                }
                writer.WriteLine($"Objective Value: {FormatDecimal(iteration.GetObjectiveValue())}");
            }
            writer.WriteLine(new string('-', 80));
        }

        private void WriteSolutionSummary(StreamWriter writer)
        {
            // Write final solution summary
            // Include optimal value and variable values
            var optimalTable = TableCache.GetTable("t-optimal");
            if (optimalTable != null)
            {
                writer.WriteLine("\n=== Solution Summary ===");
                writer.WriteLine($"Optimal Value: {FormatDecimal(optimalTable.GetObjectiveValue())}");
                var varValues = new Dictionary<string, double>();
                int rhsCol = optimalTable.GetColumnCount() - 1;
                for (int i = 0; i < optimalTable.GetRowCount() - 1; i++) // Exclude objective row
                {
                    string var = optimalTable.BasicVariables?[i] ?? $"x{i + 1}"; // Fallback if BasicVariables is null
                    varValues[var] = optimalTable.GetElement(i + 1, rhsCol);
                }
                foreach (var kvp in varValues)
                {
                    writer.WriteLine($"{kvp.Key}: {FormatDecimal(kvp.Value)}");
                }
            }
            else
            {
                writer.WriteLine("\n=== No Optimal Solution Available ===");
            }
        }

        private string FormatDecimal(double value)
        {
            // Format decimal to 3 places as required by project specs
            return value.ToString("F3");
        }

        private static int GetTableOrder(string tableId)
        {
            // Define logical ordering for common table types
            return tableId.ToLower() switch
            {
                "t-raw" => 1,           // Raw input table
                "t-i" => 2,             // Initial canonical form
                "t-canonical" => 2,     // Alternative canonical form name
                "t-1" => 10,            // First iteration
                "t-2" => 11,            // Second iteration
                "t-3" => 12,            // Third iteration
                "t-4" => 13,            // Fourth iteration
                "t-5" => 14,            // Fifth iteration
                "t-optimal" => 20,      // LP optimal solution
                "t-1.1" => 30,          // Branch & bound nodes
                "t-1.2" => 31,
                "t-1.1.1" => 32,
                "t-1.1.2" => 33,
                "t-1.2.1" => 34,
                "t-1.2.2" => 35,
                _ => 50                 // Unknown tables go at the end
            };
        }
    }
}