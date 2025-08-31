using System;
using System.Collections.Generic;
using System.Text;
using LinearProgrammingSolver.Tables;
using LinearProgrammingSolver.Utils;

namespace LinearProgrammingSolver.Analysis
{
    public static class Sensitivity
    {
        public static void RunSensitivityAnalysis(Table optimalTable)
        {
            // Perform analysis and display to console
            PerformSensitivityAnalysis(optimalTable);
            
            // Export analysis results to file
            ExportSensitivityAnalysis(optimalTable);
        }

        public static void PerformSensitivityAnalysis(Table optimalTable)
        {
            Console.WriteLine("SENSITIVITY ANALYSIS");
            Console.WriteLine("===================");
            Console.WriteLine();
            
            Console.WriteLine("Current Optimal Solution Summary:");
            Console.WriteLine("---------------------------------");
            Console.WriteLine($"Status: {optimalTable.Status}");
            Console.WriteLine($"Table Dimensions: {optimalTable.GetRowCount()}x{optimalTable.GetColumnCount()}");
            
            // Extract and display basic variables and their values
            int rhsColumn = optimalTable.GetColumnCount() - 1;
            double objectiveValue = optimalTable.GetElement(0, rhsColumn);
            Console.WriteLine($"Objective Value: {objectiveValue:F6}");
            Console.WriteLine();
            
            Console.WriteLine("Basic Variables and Values:");
            for (int i = 0; i < optimalTable.BasicVariables.Count; i++)
            {
                string basicVar = optimalTable.BasicVariables[i];
                double value = optimalTable.GetElement(i + 1, rhsColumn);
                Console.WriteLine($"  {basicVar}: {value:F6}");
            }
            Console.WriteLine();
            
            // 1. Shadow Prices (Dual Variables)
            DisplayShadowPrices(optimalTable);
            
            // 2. Reduced Costs
            DisplayReducedCosts(optimalTable);
            
            // 3. Allowable Ranges
            DisplayAllowableRanges(optimalTable);
            
            // 4. 100% Rule Information
            Display100PercentRule();
        }

        private static void DisplayShadowPrices(Table optimalTable)
        {
            Console.WriteLine("1. SHADOW PRICES (DUAL VARIABLES)");
            Console.WriteLine("=================================");
            
            int objRow = 0;
            var columnLabels = optimalTable.ColumnLabels;
            
            Console.WriteLine("Variable".PadRight(15) + "Shadow Price");
            Console.WriteLine(new string('-', 30));
            
            for (int j = 0; j < optimalTable.GetColumnCount() - 1; j++)
            {
                string varName = columnLabels[j];
                double shadowPrice = 0.0;
                
                if (varName.StartsWith("s") || varName.StartsWith("e"))
                {
                    shadowPrice = optimalTable.GetElement(objRow, j);
                    Console.WriteLine($"{varName.PadRight(15)}{shadowPrice:F6}");
                }
            }
            
            Console.WriteLine();
            Console.WriteLine("Interpretation:");
            Console.WriteLine("- Shadow price represents the marginal value of relaxing a constraint");
            Console.WriteLine("- Non-zero values indicate binding (active) constraints");
            Console.WriteLine("- Zero values indicate non-binding (slack) constraints");
            Console.WriteLine();
        }

        private static void DisplayReducedCosts(Table optimalTable)
        {
            Console.WriteLine("2. REDUCED COSTS");
            Console.WriteLine("===============");
            
            int objRow = 0;
            var columnLabels = optimalTable.ColumnLabels;
            
            Console.WriteLine("Variable".PadRight(15) + "Reduced Cost");
            Console.WriteLine(new string('-', 30));
            
            for (int j = 0; j < optimalTable.GetColumnCount() - 1; j++)
            {
                string varName = columnLabels[j];
                
                // Only show reduced costs for decision variables (not slack/surplus)
                if (!varName.StartsWith("s") && !varName.StartsWith("e"))
                {
                    double reducedCost = optimalTable.GetElement(objRow, j);
                    Console.WriteLine($"{varName.PadRight(15)}{reducedCost:F6}");
                }
            }
            
            Console.WriteLine();
            Console.WriteLine("Interpretation:");
            Console.WriteLine("- Reduced cost shows how much the objective coefficient must improve");
            Console.WriteLine("  before a non-basic variable would enter the solution");
            Console.WriteLine("- Zero reduced costs indicate basic variables (in the solution)");
            Console.WriteLine("- Non-zero reduced costs indicate non-basic variables");
            Console.WriteLine();
        }

        private static void DisplayAllowableRanges(Table optimalTable)
        {
            Console.WriteLine("3. ALLOWABLE RANGES");
            Console.WriteLine("==================");
            Console.WriteLine();
            
            Console.WriteLine("RHS (Right-Hand Side) Allowable Ranges:");
            Console.WriteLine("---------------------------------------");
            Console.WriteLine("Constraint".PadRight(15) + "Current RHS".PadRight(15) + "Status");
            Console.WriteLine(new string('-', 50));
            
            int rhsColumn = optimalTable.GetColumnCount() - 1;
            
            for (int i = 1; i < optimalTable.GetRowCount(); i++)
            {
                string basicVar = optimalTable.BasicVariables[i - 1];
                double rhsValue = optimalTable.GetElement(i, rhsColumn);
                string status = rhsValue >= 0 ? "Feasible" : "Infeasible";
                
                Console.WriteLine($"{basicVar.PadRight(15)}{rhsValue:F6}".PadRight(30) + status);
            }
            
            Console.WriteLine();
            Console.WriteLine("Objective Coefficient Allowable Ranges:");
            Console.WriteLine("---------------------------------------");
            Console.WriteLine("Note: Detailed range calculations require additional matrix operations");
            Console.WriteLine("      based on the optimal basis inverse and original problem data.");
            Console.WriteLine();
        }

        private static void Display100PercentRule()
        {
            Console.WriteLine("4. 100% RULE");
            Console.WriteLine("============");
            Console.WriteLine();
            Console.WriteLine("The 100% Rule allows for simultaneous changes to multiple parameters:");
            Console.WriteLine();
            Console.WriteLine("For RHS changes:");
            Console.WriteLine("- If Σ(|Δbi|/Ri) ≤ 1, where Ri is the allowable range for bi,");
            Console.WriteLine("  then the current basis remains optimal");
            Console.WriteLine();
            Console.WriteLine("For Objective Coefficient changes:");
            Console.WriteLine("- If Σ(|Δcj|/Rj) ≤ 1, where Rj is the allowable range for cj,");
            Console.WriteLine("  then the current optimal solution remains optimal");
            Console.WriteLine();
            Console.WriteLine("Note: Detailed implementation requires computation of individual");
            Console.WriteLine("      allowable ranges for each parameter.");
            Console.WriteLine();
        }

        public static void ExportSensitivityAnalysis(Table optimalTable)
        {
            try
            {
                var fileWriter = new FileWriter();
                var content = GenerateSensitivityAnalysisContent(optimalTable);
                fileWriter.WriteAnalysisResults("SENSITIVITY ANALYSIS", content, "data/output.txt");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error exporting Sensitivity Analysis: {ex.Message}");
            }
        }

        private static string GenerateSensitivityAnalysisContent(Table optimalTable)
        {
            var sb = new StringBuilder();
            
            sb.AppendLine("Current Optimal Solution Summary:");
            sb.AppendLine("---------------------------------");
            sb.AppendLine($"Status: {optimalTable.Status}");
            sb.AppendLine($"Table Dimensions: {optimalTable.GetRowCount()}x{optimalTable.GetColumnCount()}");
            
            // Extract and display basic variables and their values
            int rhsColumn = optimalTable.GetColumnCount() - 1;
            double objectiveValue = optimalTable.GetElement(0, rhsColumn);
            sb.AppendLine($"Objective Value: {objectiveValue:F6}");
            sb.AppendLine();
            
            sb.AppendLine("Basic Variables and Values:");
            for (int i = 0; i < optimalTable.BasicVariables.Count; i++)
            {
                string basicVar = optimalTable.BasicVariables[i];
                double value = optimalTable.GetElement(i + 1, rhsColumn);
                sb.AppendLine($"  {basicVar}: {value:F6}");
            }
            sb.AppendLine();
            
            // 1. Shadow Prices (Dual Variables)
            sb.AppendLine("1. SHADOW PRICES (DUAL VARIABLES)");
            sb.AppendLine("=================================");
            
            int objRow = 0;
            var columnLabels = optimalTable.ColumnLabels;
            
            sb.AppendLine("Variable".PadRight(15) + "Shadow Price");
            sb.AppendLine(new string('-', 30));
            
            for (int j = 0; j < optimalTable.GetColumnCount() - 1; j++)
            {
                string varName = columnLabels[j];
                double shadowPrice = 0.0;
                
                if (varName.StartsWith("s") || varName.StartsWith("e"))
                {
                    shadowPrice = optimalTable.GetElement(objRow, j);
                    sb.AppendLine($"{varName.PadRight(15)}{shadowPrice:F6}");
                }
            }
            
            sb.AppendLine();
            sb.AppendLine("Interpretation:");
            sb.AppendLine("- Shadow price represents the marginal value of relaxing a constraint");
            sb.AppendLine("- Non-zero values indicate binding (active) constraints");
            sb.AppendLine("- Zero values indicate non-binding (slack) constraints");
            sb.AppendLine();

            // 2. Reduced Costs
            sb.AppendLine("2. REDUCED COSTS");
            sb.AppendLine("===============");
            
            sb.AppendLine("Variable".PadRight(15) + "Reduced Cost");
            sb.AppendLine(new string('-', 30));
            
            for (int j = 0; j < optimalTable.GetColumnCount() - 1; j++)
            {
                string varName = columnLabels[j];
                
                // Only show reduced costs for decision variables (not slack/surplus)
                if (!varName.StartsWith("s") && !varName.StartsWith("e"))
                {
                    double reducedCost = optimalTable.GetElement(objRow, j);
                    sb.AppendLine($"{varName.PadRight(15)}{reducedCost:F6}");
                }
            }
            
            sb.AppendLine();
            sb.AppendLine("Interpretation:");
            sb.AppendLine("- Reduced cost shows how much the objective coefficient must improve");
            sb.AppendLine("  before a non-basic variable would enter the solution");
            sb.AppendLine("- Zero reduced costs indicate basic variables (in the solution)");
            sb.AppendLine("- Non-zero reduced costs indicate non-basic variables");
            sb.AppendLine();

            // 3. Allowable Ranges
            sb.AppendLine("3. ALLOWABLE RANGES");
            sb.AppendLine("==================");
            sb.AppendLine();
            
            sb.AppendLine("RHS (Right-Hand Side) Allowable Ranges:");
            sb.AppendLine("---------------------------------------");
            sb.AppendLine("Constraint".PadRight(15) + "Current RHS".PadRight(15) + "Status");
            sb.AppendLine(new string('-', 50));
            
            for (int i = 1; i < optimalTable.GetRowCount(); i++)
            {
                string basicVar = optimalTable.BasicVariables[i - 1];
                double rhsValue = optimalTable.GetElement(i, rhsColumn);
                string status = rhsValue >= 0 ? "Feasible" : "Infeasible";
                
                sb.AppendLine($"{basicVar.PadRight(15)}{rhsValue:F6}".PadRight(30) + status);
            }
            
            sb.AppendLine();
            sb.AppendLine("Objective Coefficient Allowable Ranges:");
            sb.AppendLine("---------------------------------------");
            sb.AppendLine("Note: Detailed range calculations require additional matrix operations");
            sb.AppendLine("      based on the optimal basis inverse and original problem data.");
            sb.AppendLine();

            // 4. 100% Rule
            sb.AppendLine("4. 100% RULE");
            sb.AppendLine("============");
            sb.AppendLine();
            sb.AppendLine("The 100% Rule allows for simultaneous changes to multiple parameters:");
            sb.AppendLine();
            sb.AppendLine("For RHS changes:");
            sb.AppendLine("- If Σ(|Δbi|/Ri) ≤ 1, where Ri is the allowable range for bi,");
            sb.AppendLine("  then the current basis remains optimal");
            sb.AppendLine();
            sb.AppendLine("For Objective Coefficient changes:");
            sb.AppendLine("- If Σ(|Δcj|/Rj) ≤ 1, where Rj is the allowable range for cj,");
            sb.AppendLine("  then the current optimal solution remains optimal");
            sb.AppendLine();
            sb.AppendLine("Note: Detailed implementation requires computation of individual");
            sb.AppendLine("      allowable ranges for each parameter.");

            return sb.ToString();
        }
    }
}