using System;
using System.Collections.Generic;
using LinearProgrammingSolver.Tables;
using LinearProgrammingSolver.IPAlgorithms;
using LinearProgrammingSolver.LPAlgorithms;

class DebugConstraintCreation
{
    public static void DebugTest()
    {
        Console.WriteLine("=== DEBUG: Testing constraint creation for t-optimal-B1-A1 ===");
        
        // Create the t-optimal-B1 table exactly as it should be from JSON
        var matrix = new double[,]
        {
            {0, 0, 0, 1, 1, 41},      // OBJ
            {0, 1, 0, 0.2, 1.8, 1.8}, // C1: x2 basic here (row 1)
            {1, 0, 0, 0, -1, 4},       // C2: x1 basic here  
            {0, 0, 1, -0.2, -0.8, 0.2} // C3
        };
        
        var rowLabels = new List<string> {"OBJ", "C1", "C2", "C3"};
        var columnLabels = new List<string> {"x1", "x2", "s1", "s2", "e3", "RHS"};
        var table = new Table("t-optimal-B1-debug", matrix, rowLabels, columnLabels, OptimizationType.Maximize, "Optimal");
        table.BasicVariables = new List<string> {"x2", "x1", "s1"};
        
        Console.WriteLine("Original table t-optimal-B1:");
        Console.WriteLine(table.ToString());
        Console.WriteLine();
        
        // Test SelectBranchingVariable - should pick x2
        var bnb = new BranchAndBoundAlgorithm();
        var branchingInfo = bnb.SelectBranchingVariable(table);
        
        Console.WriteLine($"Selected branching variable: {branchingInfo?.VariableName}");
        Console.WriteLine($"Column Index: {branchingInfo?.ColumnIndex}");
        Console.WriteLine($"Basic Row Index: {branchingInfo?.BasicRowIndex}");
        Console.WriteLine($"Current Value: {branchingInfo?.CurrentValue:F6}");
        Console.WriteLine();
        
        // Create A branch constraint (x2 <= 1)
        var subproblems = bnb.BranchOnVariable(table, branchingInfo);
        var subproblemA = subproblems[0]; // Should be the A branch
        
        Console.WriteLine($"A branch table {subproblemA.TableId} BEFORE dual simplex:");
        Console.WriteLine(subproblemA.ToString());
        Console.WriteLine();
        
        // Show specific constraint row details
        int lastRow = subproblemA.GetRowCount() - 1;
        Console.WriteLine($"New constraint row {lastRow} values:");
        for (int j = 0; j < subproblemA.GetColumnCount(); j++)
        {
            var colLabel = subproblemA.ColumnLabels[j];
            var value = subproblemA.GetElement(lastRow, j);
            Console.WriteLine($"  {colLabel}: {value:F6}");
        }
        Console.WriteLine();
        
        // Expected constraint from JSON: [0, 0, 0, -0.2, -1.8, 1, -0.8]
        Console.WriteLine("Expected constraint from JSON: [0, 0, 0, -0.2, -1.8, 1, -0.8]");
        Console.WriteLine();
        
        // Apply dual simplex
        var dualSimplex = new DualSimplexAlgorithm();
        var solvedA = dualSimplex.SolveLP(subproblemA);
        
        Console.WriteLine($"After dual simplex - {solvedA.TableId}:");
        Console.WriteLine(solvedA.ToString());
        Console.WriteLine();
        
        // Show final basic variable values
        Console.WriteLine("Final basic variable values:");
        for (int i = 0; i < solvedA.BasicVariables.Count; i++)
        {
            var varName = solvedA.BasicVariables[i];
            var value = solvedA.GetElement(i + 1, solvedA.GetColumnCount() - 1);
            Console.WriteLine($"  {varName} = {value:F6}");
        }
        
        Console.WriteLine($"Objective value: {solvedA.GetObjectiveValue():F6}");
        Console.WriteLine();
        
        // Expected values for comparison
        Console.WriteLine("Expected values from JSON:");
        Console.WriteLine("  x1 = 4.444000");
        Console.WriteLine("  x2 = 1.000000"); 
        Console.WriteLine("  Objective = 40.556000");
    }
}