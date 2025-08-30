using System;
using System.Collections.Generic;
using LinearProgrammingSolver.Tables;
using LinearProgrammingSolver.Algorithms.Implementations.LP;
using LinearProgrammingSolver.Utils;

namespace LinearProgrammingSolver.Algorithms.Implementations.IP
{
    public class CuttingPlaneAlgorithm
    {
        private PrimalSimplexAlgorithm _primalSimplex;
        private DualSimplexAlgorithm _dualSimplex;
        private List<Table> _allIterations;
        private List<string> _cuttingPlanes;
        private const int MAX_CUTS = 10;
        private const double TOLERANCE = 1e-6;

        public CuttingPlaneAlgorithm()
        {
            _primalSimplex = new PrimalSimplexAlgorithm();
            _dualSimplex = new DualSimplexAlgorithm();
            _allIterations = new List<Table>();
            _cuttingPlanes = new List<string>();
        }

        public Table SolveIP(Table lpOptimalTable)
        {
            if (lpOptimalTable?.Status != "Optimal")
            {
                Console.WriteLine("Error: Need optimal LP solution");
                return null;
            }

            // Check if already integer-feasible
            if (IsIntegerFeasible(lpOptimalTable))
            {
                Console.WriteLine("Solution is already integer");
                var integerTable = new Table($"{lpOptimalTable.TableId}-integer", lpOptimalTable);
                integerTable.Status = "Optimal_Integer";
                TableCache.StoreTable(integerTable);
                return integerTable;
            }

            Console.WriteLine("Applying cuts...");

            Table workingTable = new Table("cutting-start", lpOptimalTable);
            int cutIteration = 0;
            double previousObjValue = workingTable.GetElement(0, workingTable.GetColumnCount() - 1);
            int noImprovementCount = 0;
            
            // Step 2: Iterative cutting process
            while (cutIteration < MAX_CUTS && !IsIntegerFeasible(workingTable))
            {
                cutIteration++;
                
                // Find cutting row
                var (fractionalVar, fractionalRow, fractionalValue) = FindGomoryCuttingRow(workingTable);
                
                if (fractionalVar == null)
                {
                    Console.WriteLine("No fractional variables found");
                    break;
                }
                
                // Generate cut
                Table cutTable = GenerateGomoryCut(workingTable, fractionalRow, cutIteration);
                
                if (cutTable == null)
                {
                    Console.WriteLine("Failed to generate cut");
                    break;
                }
                
                // Solve with dual simplex
                Table newSolution = _dualSimplex.SolveLP(cutTable);
                
                if (newSolution?.Status != "Optimal")
                {
                    Console.WriteLine($"Dual simplex failed: {newSolution?.Status ?? "Error"}");
                    break;
                }
                
                workingTable = newSolution;
                workingTable.TableId = $"cut-{cutIteration}";
                TableCache.StoreTable(workingTable);
                
                double newObjValue = workingTable.GetElement(0, workingTable.GetColumnCount() - 1);
                Console.WriteLine($"Cut {cutIteration}: obj = {newObjValue:F3}");
                
                // Check if integer feasible
                if (IsIntegerFeasible(workingTable))
                {
                    break;
                }
                
                // Check for improvement
                if (Math.Abs(newObjValue - previousObjValue) < TOLERANCE)
                {
                    noImprovementCount++;
                    if (noImprovementCount >= 3)
                    {
                        Console.WriteLine("No improvement - stopping");
                        break;
                    }
                }
                else
                {
                    noImprovementCount = 0;
                }
                
                previousObjValue = newObjValue;
            }
            
            // Create final result table (avoid modifying the working table)
            var finalTable = new Table("cp-final", workingTable);
            
            if (IsIntegerFeasible(finalTable))
            {
                finalTable.Status = "Optimal_Integer";
            }
            else
            {
                finalTable.Status = "Partial_Solution";
                Console.WriteLine("Max cuts reached");
            }
            
            TableCache.StoreTable(finalTable);
            return finalTable;
        }

        // Generate proper Gomory cutting plane from fractional row
        private Table GenerateGomoryCut(Table currentTable, int fractionalRow, int cutNumber)
        {
            int numColumns = currentTable.GetColumnCount() - 1;
            double[] cutCoefficients = new double[numColumns];
            
            // Get RHS fractional part
            double rhsValue = currentTable.GetElement(fractionalRow, currentTable.GetColumnCount() - 1);
            double rhsFraction = GetFractionalPart(rhsValue);
            
            // Generate cut coefficients
            for (int j = 0; j < numColumns; j++)
            {
                double coefficient = currentTable.GetElement(fractionalRow, j);
                double fractionalPart = GetFractionalPart(coefficient);
                cutCoefficients[j] = -fractionalPart;
            }
            
            rhsFraction = -rhsFraction;
            
            // Store cut description
            _cuttingPlanes.Add($"Cut #{cutNumber}");
            
            return ExpandTableWithCut(currentTable, cutCoefficients, rhsFraction, $"gomory{cutNumber}");
        }
        
        // Get fractional part for Gomory cuts
        private double GetFractionalPart(double value)
        {
            // For Gomory cuts, we need proper fractional part calculation
            // For negative values like -1.25: -1.25 = -2 + 0.75, so fractional part is 0.75
            double fraction = value - Math.Floor(value);
            return fraction;
        }
        
        // Find the column index of a specific variable in the table
        private int FindVariableColumnIndex(Table table, string variableName)
        {
            for (int j = 0; j < table.ColumnLabels.Count; j++)
            {
                if (table.ColumnLabels[j].Equals(variableName, StringComparison.OrdinalIgnoreCase))
                {
                    return j;
                }
            }
            return -1; // Variable not found
        }

        // Legacy method - kept for interface compatibility
        public double[] GenerateGomoryCut(Table table, int basicRow)
        {
            // This is a simplified placeholder - full Gomory cuts are more complex
            // Our implementation uses simpler rounding-based cuts in ApplyCuttingConstraint
            Console.WriteLine("Note: Using simplified cuts instead of full Gomory cuts");
            return new double[table.GetColumnCount() - 1];
        }

        // Legacy method - kept for interface compatibility  
        public int SelectCuttingRow(Table table)
        {
            // Our implementation uses FindGomoryCuttingRow instead
            var (_, row, _) = FindGomoryCuttingRow(table);
            return row;
        }

        // Check if current solution contains fractional integer variables
        private bool IsIntegerFeasible(Table table)
        {
            // Examine all basic variables in the optimal solution
            for (int i = 0; i < table.BasicVariables.Count; i++)
            {
                string basicVar = table.BasicVariables[i];
                
                // Only check decision variables (x1, x2, etc.) - ignore slack/surplus/artificial
                if (IsDecisionVariable(basicVar))
                {
                    double value = table.GetElement(i + 1, table.GetColumnCount() - 1); // +1 to skip obj row
                    
                    // Check if value is significantly non-integer
                    if (Math.Abs(value - Math.Round(value)) > TOLERANCE)
                    {
                        return false; // Found fractional integer variable
                    }
                }
            }
            return true; // All integer variables have integer values
        }
        
        // Find the row for Gomory cut - RHS fraction closest to 0.5
        private (string variable, int row, double value) FindGomoryCuttingRow(Table table)
        {
            string selectedVar = null;
            int selectedRow = -1;
            double selectedValue = 0;
            double closestTo05 = double.MaxValue;
            
            for (int i = 0; i < table.BasicVariables.Count; i++)
            {
                string basicVar = table.BasicVariables[i];
                double rhsValue = table.GetElement(i + 1, table.GetColumnCount() - 1);
                
                // Only consider decision variables (x1, x2, etc.)
                if (IsDecisionVariable(basicVar))
                {
                    double fractionalPart = rhsValue - Math.Floor(rhsValue);
                    double distanceFrom05 = Math.Abs(fractionalPart - 0.5);
                    
                    // Select row with fraction closest to 0.5
                    if (fractionalPart > TOLERANCE && 
                        (distanceFrom05 < closestTo05 || 
                         (Math.Abs(distanceFrom05 - closestTo05) < TOLERANCE && 
                          string.Compare(basicVar, selectedVar) < 0)))
                    {
                        closestTo05 = distanceFrom05;
                        selectedVar = basicVar;
                        selectedRow = i + 1;
                        selectedValue = rhsValue;
                    }
                }
            }
            
            return (selectedVar, selectedRow, selectedValue);
        }
        
        // Helper to identify decision variables vs auxiliary variables
        private bool IsDecisionVariable(string varName)
        {
            // Decision variables are x1, x2, x3, etc.
            // Auxiliary variables are s1, s2 (slack), e1, e2 (surplus), a1, a2 (artificial)
            return varName.StartsWith("x") && 
                   !varName.StartsWith("s") && 
                   !varName.StartsWith("e") && 
                   !varName.StartsWith("a");
        }

        // Expand the current table by adding a Gomory cutting constraint (no slack variable)
        private Table ExpandTableWithCut(Table originalTable, double[] cutCoefficients, double cutRHS, string cutName)
        {
            int originalRows = originalTable.GetRowCount();
            int originalCols = originalTable.GetColumnCount(); 
            
            // New table dimensions: +1 row (for cut), same columns (no new variables)
            int newRows = originalRows + 1;
            int newCols = originalCols; // No new columns for cutting plane
            
            // Create expanded matrix
            double[,] expandedMatrix = new double[newRows, newCols];
            
            // Copy original matrix data to new matrix
            for (int i = 0; i < originalRows; i++)
            {
                for (int j = 0; j < originalCols; j++)
                {
                    expandedMatrix[i, j] = originalTable.GetElement(i, j);
                }
            }
            
            // Add the cutting constraint row
            int cutRowIndex = originalRows;
            for (int j = 0; j < cutCoefficients.Length; j++)
            {
                expandedMatrix[cutRowIndex, j] = cutCoefficients[j];
            }
            // Set RHS for the cutting constraint (last column)
            expandedMatrix[cutRowIndex, newCols - 1] = cutRHS;
            
            // Create updated labels (same columns, new row)
            var newRowLabels = new List<string>(originalTable.RowLabels) { cutName };
            var newColumnLabels = new List<string>(originalTable.ColumnLabels);
            
            // Basic variables stay the same - no new slack variable added
            var newBasicVariables = new List<string>(originalTable.BasicVariables);
            
            // Create the expanded table using TableCache
            var expandedTable = TableCache.CreateAndStoreTable(
                $"{originalTable.TableId}-{cutName}",
                expandedMatrix,
                newRowLabels,
                newColumnLabels,
                originalTable.OptimizationType,
                "Canonical"
            );
            
            // Set the basic variables and other metadata
            expandedTable.BasicVariables = newBasicVariables;
            expandedTable.ConstraintOperators = new Dictionary<string, ConstraintOperator>(originalTable.ConstraintOperators);
            expandedTable.ConstraintOperators[cutName] = ConstraintOperator.LessThanOrEqual;
            
            return expandedTable;
        }

        // Display methods for algorithm analysis
        public void DisplayProductForm(Table table)
        {
            Console.WriteLine("=== CUTTING PLANE - PRODUCT FORM ===");
            Console.WriteLine("Product form representation not implemented for Cutting Plane algorithm.");
            Console.WriteLine("Cutting planes modify the constraint matrix directly.");
            Console.WriteLine();
        }

        public void DisplayPriceOut(Table table)
        {
            Console.WriteLine("=== CUTTING PLANE - PRICE OUT ===");
            Console.WriteLine("Price out representation not implemented for Cutting Plane algorithm.");
            Console.WriteLine("Standard simplex tableau operations are used after each cut.");
            Console.WriteLine();
        }

        public void DisplayAllProductFormAndPriceOut()
        {
            Console.WriteLine("=== ALL CUTTING PLANE ITERATIONS ===");
            
            if (_allIterations.Count == 0)
            {
                Console.WriteLine("No cutting iterations performed.");
                return;
            }
            
            Console.WriteLine($"Total cutting iterations: {_allIterations.Count}");
            foreach (var iteration in _allIterations)
            {
                Console.WriteLine($"Iteration: {iteration.TableId} - Status: {iteration.Status}");
                double objValue = iteration.GetElement(0, iteration.GetColumnCount() - 1);
                Console.WriteLine($"Objective Value: {objValue:F4}");
                Console.WriteLine();
            }
        }

        public void DisplayCuttingPlanes()
        {
            if (_cuttingPlanes.Count > 0)
            {
                Console.WriteLine($"Cuts applied: {_cuttingPlanes.Count}");
            }
        }
    }
}