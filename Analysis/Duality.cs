using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LinearProgrammingSolver.Tables;
using LinearProgrammingSolver.Utils;
using LinearProgrammingSolver.Algorithms.Implementations.LP;

namespace LinearProgrammingSolver.Analysis
{
    public static class Duality
    {
        public static void RunDualityAnalysis(Table canonicalTable, Table optimalTable)
        {
            // Perform analysis and display to console
            PerformDualityAnalysis(canonicalTable, optimalTable);
            
            // Export analysis results to file
            ExportDualityAnalysis(canonicalTable, optimalTable);
        }

        public static void PerformDualityAnalysis(Table canonicalTable, Table optimalTable)
        {
            Console.WriteLine("DUALITY ANALYSIS");
            Console.WriteLine("================");
            Console.WriteLine();

            Console.WriteLine("Step 1: Displaying Primal Problem (Canonical Form)");
            Console.WriteLine("---------------------------------------------------");
            canonicalTable.DisplayTraditional();
            Console.WriteLine();

            Console.WriteLine("Step 2: Displaying Primal Optimal Solution");
            Console.WriteLine("-------------------------------------------");
            optimalTable.DisplayTraditional();
            Console.WriteLine();

            double primalObjectiveValue = ExtractPrimalObjectiveValue(optimalTable);
            Console.WriteLine($"Primal Objective Value (Z): {primalObjectiveValue:F6}");
            Console.WriteLine();

            Console.WriteLine("Step 3: Extracting Shadow Prices from Optimal Tableau");
            Console.WriteLine("------------------------------------------------------");
            var shadowPrices = ExtractShadowPrices(optimalTable);
            DisplayShadowPrices(shadowPrices);
            Console.WriteLine();

            Console.WriteLine("Step 4: Constructing Dual Problem");
            Console.WriteLine("----------------------------------");
            var dualTable = ConstructDualProblem(canonicalTable);
            dualTable.DisplayTraditional();
            Console.WriteLine();

            Console.WriteLine("Step 5: Solving Dual Problem");
            Console.WriteLine("-----------------------------");
            var primalSimplex = new PrimalSimplexAlgorithm();
            var dualOptimal = primalSimplex.SolveLP(dualTable);
            
            if (dualOptimal != null && dualOptimal.Status == "Optimal")
            {
                dualOptimal.DisplayTraditional();
                Console.WriteLine();

                double dualObjectiveValue = ExtractDualObjectiveValue(dualOptimal);
                Console.WriteLine($"Dual Objective Value (W): {dualObjectiveValue:F6}");
                Console.WriteLine();

                Console.WriteLine("Step 6: Duality Verification");
                Console.WriteLine("-----------------------------");
                VerifyDuality(primalObjectiveValue, dualObjectiveValue, shadowPrices, dualOptimal);
            }
            else
            {
                Console.WriteLine("Error: Could not solve dual problem optimally.");
                Console.WriteLine($"Dual Status: {dualOptimal?.Status ?? "Unknown"}");
            }
        }

        private static double ExtractPrimalObjectiveValue(Table optimalTable)
        {
            int rhsColumn = optimalTable.GetColumnCount() - 1;
            return optimalTable.GetElement(0, rhsColumn);
        }

        private static double ExtractDualObjectiveValue(Table dualOptimalTable)
        {
            int rhsColumn = dualOptimalTable.GetColumnCount() - 1;
            double dualValue = dualOptimalTable.GetElement(0, rhsColumn);
            
            if (dualOptimalTable.OptimizationType == OptimizationType.Minimize)
            {
                return -dualValue;
            }
            return dualValue;
        }

        private static Dictionary<string, double> ExtractShadowPrices(Table optimalTable)
        {
            var shadowPrices = new Dictionary<string, double>();
            int objRow = 0;
            
            for (int j = 0; j < optimalTable.GetColumnCount() - 1; j++)
            {
                string varName = optimalTable.ColumnLabels[j];
                if (varName.StartsWith("s"))
                {
                    double shadowPrice = optimalTable.GetElement(objRow, j);
                    shadowPrices[varName] = shadowPrice;
                }
            }
            
            return shadowPrices;
        }

        private static void DisplayShadowPrices(Dictionary<string, double> shadowPrices)
        {
            Console.WriteLine("Shadow Prices (from Optimal Tableau):");
            foreach (var kvp in shadowPrices)
            {
                Console.WriteLine($"  {kvp.Key}: {kvp.Value:F6}");
            }
        }

        private static Table ConstructDualProblem(Table canonicalTable)
        {
            int numConstraints = canonicalTable.GetRowCount() - 1;
            int numDecisionVars = CountDecisionVariables(canonicalTable);
            
            var constraintRHS = ExtractConstraintRHS(canonicalTable);
            var objectiveCoeffs = ExtractObjectiveCoefficients(canonicalTable, numDecisionVars);
            var constraintMatrix = ExtractConstraintMatrix(canonicalTable, numDecisionVars, numConstraints);
            
            int dualRows = 1 + numDecisionVars;
            int dualCols = numConstraints + numDecisionVars + 1;
            
            var dualMatrix = new double[dualRows, dualCols];
            var dualRowLabels = new List<string> { "OBJ" };
            var dualColumnLabels = new List<string>();
            
            for (int i = 0; i < numConstraints; i++)
            {
                dualColumnLabels.Add($"y{i + 1}");
            }
            for (int i = 0; i < numDecisionVars; i++)
            {
                dualRowLabels.Add($"C{i + 1}");
                dualColumnLabels.Add($"s{i + 1}");
            }
            dualColumnLabels.Add("RHS");
            
            if (canonicalTable.OptimizationType == OptimizationType.Maximize)
            {
                for (int j = 0; j < numConstraints; j++)
                {
                    dualMatrix[0, j] = -constraintRHS[j];
                }
                
                for (int i = 0; i < numDecisionVars; i++)
                {
                    for (int j = 0; j < numConstraints; j++)
                    {
                        dualMatrix[i + 1, j] = constraintMatrix[i, j];
                    }
                    dualMatrix[i + 1, numConstraints + i] = 1.0;
                    dualMatrix[i + 1, dualCols - 1] = objectiveCoeffs[i];
                }
            }
            
            var dualTable = TableCache.CreateAndStoreTable(
                "t-dual-canonical", 
                dualMatrix, 
                dualRowLabels, 
                dualColumnLabels, 
                OptimizationType.Minimize, 
                "Canonical"
            );
            
            // Set basic variables for the dual (slack variables s1, s2)
            var dualBasicVariables = new List<string>();
            for (int i = 0; i < numDecisionVars; i++)
            {
                dualBasicVariables.Add($"s{i + 1}");
            }
            dualTable.BasicVariables = dualBasicVariables;
            
            return dualTable;
        }

        private static int CountDecisionVariables(Table table)
        {
            int count = 0;
            foreach (string label in table.ColumnLabels)
            {
                if (label.StartsWith("x") && !label.StartsWith("s") && !label.StartsWith("e") && !label.StartsWith("a") && label != "RHS")
                {
                    count++;
                }
            }
            return count;
        }

        private static double[] ExtractConstraintRHS(Table table)
        {
            int rhsColumn = table.GetColumnCount() - 1;
            int numConstraints = table.GetRowCount() - 1;
            double[] rhs = new double[numConstraints];
            
            for (int i = 0; i < numConstraints; i++)
            {
                rhs[i] = table.GetElement(i + 1, rhsColumn);
            }
            
            return rhs;
        }

        private static double[] ExtractObjectiveCoefficients(Table table, int numDecisionVars)
        {
            double[] coeffs = new double[numDecisionVars];
            
            for (int j = 0; j < numDecisionVars; j++)
            {
                coeffs[j] = table.GetElement(0, j);
                if (table.OptimizationType == OptimizationType.Maximize)
                {
                    coeffs[j] = -coeffs[j];
                }
            }
            
            return coeffs;
        }

        private static double[,] ExtractConstraintMatrix(Table table, int numDecisionVars, int numConstraints)
        {
            double[,] matrix = new double[numDecisionVars, numConstraints];
            
            for (int i = 0; i < numConstraints; i++)
            {
                for (int j = 0; j < numDecisionVars; j++)
                {
                    matrix[j, i] = table.GetElement(i + 1, j);
                }
            }
            
            return matrix;
        }

        private static void VerifyDuality(double primalValue, double dualValue, 
                                        Dictionary<string, double> shadowPrices, Table dualOptimal)
        {
            Console.WriteLine("DUALITY VERIFICATION:");
            Console.WriteLine($"  Primal Objective (Z): {primalValue:F6}");
            Console.WriteLine($"  Dual Objective (W):   {dualValue:F6}");
            Console.WriteLine($"  Difference:           {Math.Abs(primalValue - dualValue):F6}");
            Console.WriteLine();

            if (Math.Abs(primalValue - dualValue) < 0.001)
            {
                Console.WriteLine("✓ STRONG DUALITY VERIFIED: Primal and dual objective values are equal.");
            }
            else
            {
                Console.WriteLine("✗ STRONG DUALITY NOT SATISFIED: Objective values differ significantly.");
            }
            Console.WriteLine();

            Console.WriteLine("COMPLEMENTARY SLACKNESS CHECK:");
            Console.WriteLine("(This would require more detailed analysis of dual variables and slack values)");
            Console.WriteLine();

            Console.WriteLine("ECONOMIC INTERPRETATION:");
            Console.WriteLine("Shadow prices represent the marginal value of relaxing constraints.");
            Console.WriteLine("Dual variables represent the value of resources in the optimal solution.");
        }

        public static void ExportDualityAnalysis(Table canonicalTable, Table optimalTable)
        {
            try
            {
                var fileWriter = new FileWriter();
                var content = GenerateDualityAnalysisContent(canonicalTable, optimalTable);
                fileWriter.WriteAnalysisResults("DUALITY ANALYSIS", content, "data/output.txt");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error exporting Duality Analysis: {ex.Message}");
            }
        }

        private static string GenerateDualityAnalysisContent(Table canonicalTable, Table optimalTable)
        {
            var sb = new StringBuilder();
            
            sb.AppendLine("Step 1: Primal Problem (Canonical Form)");
            sb.AppendLine("---------------------------------------");
            sb.AppendLine($"Status: {canonicalTable.Status}");
            sb.AppendLine($"Optimization: {canonicalTable.OptimizationType}");
            sb.AppendLine("Canonical Form Table:");
            sb.AppendLine(canonicalTable.ToString());
            sb.AppendLine();

            sb.AppendLine("Step 2: Primal Optimal Solution");
            sb.AppendLine("-------------------------------");
            sb.AppendLine($"Status: {optimalTable.Status}");
            sb.AppendLine("Optimal Table:");
            sb.AppendLine(optimalTable.ToString());
            sb.AppendLine();

            double primalObjectiveValue = ExtractPrimalObjectiveValue(optimalTable);
            sb.AppendLine($"Primal Objective Value (Z): {primalObjectiveValue:F6}");
            sb.AppendLine();

            sb.AppendLine("Step 3: Shadow Prices from Optimal Tableau");
            sb.AppendLine("------------------------------------------");
            var shadowPrices = ExtractShadowPrices(optimalTable);
            foreach (var kvp in shadowPrices)
            {
                sb.AppendLine($"{kvp.Key}: {kvp.Value:F6}");
            }
            sb.AppendLine();

            sb.AppendLine("Step 4: Dual Problem Construction");
            sb.AppendLine("---------------------------------");
            var dualTable = ConstructDualProblem(canonicalTable);
            sb.AppendLine("Dual Problem Table:");
            sb.AppendLine(dualTable.ToString());
            sb.AppendLine();

            sb.AppendLine("Step 5: Dual Problem Solution");
            sb.AppendLine("-----------------------------");
            var primalSimplex = new PrimalSimplexAlgorithm();
            var dualOptimal = primalSimplex.SolveLP(dualTable);
            
            if (dualOptimal != null && dualOptimal.Status == "Optimal")
            {
                sb.AppendLine("Dual Optimal Table:");
                sb.AppendLine(dualOptimal.ToString());
                sb.AppendLine();

                double dualObjectiveValue = ExtractDualObjectiveValue(dualOptimal);
                sb.AppendLine($"Dual Objective Value (W): {dualObjectiveValue:F6}");
                sb.AppendLine();

                sb.AppendLine("Step 6: Duality Verification");
                sb.AppendLine("----------------------------");
                sb.AppendLine($"Primal Objective: {primalObjectiveValue:F6}");
                sb.AppendLine($"Dual Objective: {dualObjectiveValue:F6}");
                sb.AppendLine($"Difference: {Math.Abs(primalObjectiveValue - dualObjectiveValue):F6}");
                
                if (Math.Abs(primalObjectiveValue - dualObjectiveValue) < 1e-6)
                {
                    sb.AppendLine("✓ Strong Duality Theorem verified: Primal = Dual");
                }
                else
                {
                    sb.AppendLine("⚠ Duality gap detected - may indicate numerical errors");
                }
            }
            else
            {
                sb.AppendLine("Error: Could not solve dual problem optimally.");
                sb.AppendLine($"Dual Status: {dualOptimal?.Status ?? "Unknown"}");
            }

            return sb.ToString();
        }
    }
}