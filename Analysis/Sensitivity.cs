using System;
using LinearProgrammingSolver.Tables;

namespace LinearProgrammingSolver.Analysis
{
    public static class Sensitivity
    {
        public static void RunSensitivityAnalysis(Table optimalTable)
        {
            Console.WriteLine("SENSITIVITY ANALYSIS");
            Console.WriteLine("===================");
            Console.WriteLine();
            
            Console.WriteLine("Shadow prices / dual variables: approximated via final tableau (see optimal tableau).");
            Console.WriteLine("Allowable ranges for RHS and costs require final basis; omitted detailed numeric bounds here.");
            Console.WriteLine();
            
            Console.WriteLine("Operations supported:");
            Console.WriteLine("- Add activity (column) or constraint (row): not executed in this demo run.");
            Console.WriteLine("- What-if changes: recompute optimal value by re-solving.");
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
            
            Console.WriteLine("Note: For detailed sensitivity analysis (ranges, parametric analysis),");
            Console.WriteLine("      additional computational methods would be required.");
        }
    }
}