using System;
using System.Collections.Generic;
using LinearProgrammingSolver.Tables;
using LinearProgrammingSolver.LPAlgorithms;

class TestDualSimplex
{
    public static void TestEnteringVariable()
    {
        Console.WriteLine("=== Testing Dual Simplex Entering Variable Selection ===");
        
        // Create the exact table that goes into dual simplex from debug output
        var matrix = new double[,]
        {
            {0, 0, 0, 1, 1, 0, 41},        // OBJ row
            {0, 1, 0, 0.2, 1.8, 0, 1.8},  // C1: x2 = 1.8
            {1, 0, 0, 0, -1, 0, 4},        // C2: x1 = 4
            {0, 0, 1, -0.2, -0.8, 0, 0.2}, // C3: s1 = 0.2
            {0, 0, 0, -0.2, -1.8, 1, -0.8} // C4: New constraint (negative RHS)
        };
        
        var rowLabels = new List<string> {"OBJ", "C1", "C2", "C3", "C4"};
        var columnLabels = new List<string> {"x1", "x2", "s1", "s2", "e3", "s4", "RHS"};
        var table = new Table("test-dual", matrix, rowLabels, columnLabels, OptimizationType.Maximize, "Before_Dual");
        table.BasicVariables = new List<string> {"x2", "x1", "s1", "s4"};
        
        Console.WriteLine("Table before dual simplex:");
        Console.WriteLine(table.ToString());
        Console.WriteLine();
        
        var dualSimplex = new DualSimplexAlgorithm();
        
        // Test leaving variable selection
        int leavingRow = dualSimplex.SelectLeavingVariable(table);
        Console.WriteLine($"Leaving row: {leavingRow} (should be 4 - the constraint with negative RHS)");
        
        // Test entering variable selection with detailed ratio calculations
        int rhsCol = table.GetColumnCount() - 1;
        int leavingRowIndex = 4; // We know it's row 4
        
        Console.WriteLine($"\nAnalyzing entering variable for leaving row {leavingRowIndex}:");
        Console.WriteLine("Constraint row values: [0, 0, 0, -0.2, -1.8, 1, -0.8]");
        Console.WriteLine("Objective row values:  [0, 0, 0,  1,    1,   0,  41]");
        Console.WriteLine();
        
        Console.WriteLine("Ratio calculations (obj_element / leaving_element) for negative leaving elements:");
        
        double bestRatio = double.PositiveInfinity;
        int bestColumn = -1;
        
        for (int j = 0; j < rhsCol; j++)
        {
            double leavingElement = table.GetElement(leavingRowIndex, j);
            double objElement = table.GetElement(0, j);
            string colName = table.ColumnLabels[j];
            
            if (leavingElement < -0.001)
            {
                double ratio = objElement / leavingElement;
                Console.WriteLine($"Column {j} ({colName}): {objElement:F3} / {leavingElement:F3} = {ratio:F6}");
                
                if (ratio < bestRatio)
                {
                    bestRatio = ratio;
                    bestColumn = j;
                }
            }
            else if (Math.Abs(leavingElement) > 0.001)
            {
                Console.WriteLine($"Column {j} ({colName}): {objElement:F3} / {leavingElement:F3} = Not negative, skip");
            }
        }
        
        Console.WriteLine($"\nBest entering column: {bestColumn} ({table.ColumnLabels[bestColumn]}) with ratio {bestRatio:F6}");
        
        // Test actual method
        int actualEntering = dualSimplex.SelectEnteringVariable(table, leavingRowIndex);
        Console.WriteLine($"Actual method result: {actualEntering} ({table.ColumnLabels[actualEntering]})");
        
        Console.WriteLine();
        Console.WriteLine("Expected: Should choose a column that leads to correct solution");
        Console.WriteLine("From JSON, final basic variables should be: x2, x1, s1, e3");
        Console.WriteLine("This means s4 should leave and e3 should enter");
    }
}