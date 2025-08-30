using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.Json;
using LinearProgrammingSolver.Tables;
using LinearProgrammingSolver.Models;
// using LinearProgrammingSolver.Iterations; // Commented out - missing namespace

namespace LinearProgrammingSolver.Utils
{
    public class FileWriter
 {
        // Main entry: writes the full solution result to a text file.
        // Includes canonical form, all iterations, and final solution.
        public void WriteResultToFile(SolutionResult result, string outputPath)
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
                WriteCanonicalForm(result, writer);

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
                                string operatorSymbol = table.GetConstraintOperatorSymbol(rowLabel);
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
                WriteSolutionSummary(result, writer);
            }
        }

        private void WriteCanonicalForm(SolutionResult result, StreamWriter writer)
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
                            string operatorSymbol = canonicalTable.GetConstraintOperatorSymbol(rowLabel);
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
                            string operatorSymbol = iteration.GetConstraintOperatorSymbol(rowLabel);
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

        private void WriteSolutionSummary(SolutionResult result, StreamWriter writer)
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