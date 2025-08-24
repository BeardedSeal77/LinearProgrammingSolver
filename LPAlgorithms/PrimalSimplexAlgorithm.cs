using System;
using System.Collections.Generic;
using LinearProgrammingSolver.Tables;

namespace LinearProgrammingSolver.LPAlgorithms
{
    public class PrimalSimplexAlgorithm
    {
        public Table SolveLP(Table initialTable)
        // Solve LP using Primal Simplex method
        // Return optimal table or indicate infeasible/unbounded
        {
            if (initialTable == null)
            {
                Console.WriteLine("Error: Initial table is null.");
                return null;
            }

            Table currentTable = initialTable;  // Start from canonical form (t-i)
            int iterationCount = 1;

            while (!IsOptimal(currentTable))
            {
              /* REMOVED Infeasibility check to avoid false positives
               // Check for infeasibility
                if (IsInfeasible(currentTable))
                {
                    currentTable.Status = "Infeasible";
                    TableCache.StoreTable(currentTable);
                    return currentTable;
                } */

                // Perform one iteration (pivot)
                Table nextTable = PerformIteration(currentTable);

                // ADDED: Check for terminal status to prevent overwriting Unbounded
                if (nextTable.Status == "Unbounded")
                {
                    TableCache.StoreTable(nextTable);
                    return nextTable;
                }

                // Name and store the iteration
                nextTable.TableId = $"t-{iterationCount}";
                nextTable.Status = "Iteration";
                TableCache.StoreTable(nextTable);

                currentTable = nextTable;
                iterationCount++;

                // Prevent infinite loops (cycling/degeneracy)
                if (iterationCount > 50)
                {
                    currentTable.Status = "Max_Iterations_Reached";
                    TableCache.StoreTable(currentTable);
                    return currentTable;
                }
            }

            // Mark final as optimal
            currentTable.Status = "Optimal";
            currentTable.TableId = "t-optimal";
            TableCache.StoreTable(currentTable);

            // CHANGED: Added caching after setting Infeasible
            // Post-optimal check for infeasibility if artificial variables are positive
            if (IsInfeasiblePostOptimal(currentTable))
            {
                Console.WriteLine("Infeasible post-optimal: Artificial variables positive.");
                currentTable.Status = "Infeasible";
                TableCache.StoreTable(currentTable); // ADDED: Re-cache Infeasible table
            }

            return currentTable;
        }

        public Table PerformIteration(Table currentTable)

        // Perform one iteration of Primal Simplex
        // Return new table after pivoting
        {
            // Step 1: Select entering variable (most negative for max)
            int enteringColumn = SelectEnteringVariable(currentTable);
            if (enteringColumn == -1) return currentTable; // Already optimal

            // Step 2: Check for unboundedness
            if (IsUnbounded(currentTable, enteringColumn))
            {
                var unboundedTable = new Table($"{currentTable.TableId}-unbounded", currentTable, "Unbounded");
                TableCache.StoreTable(unboundedTable);
                return unboundedTable;
            }

            // Step 3: Select leaving variable (minimum ratio test)
            int leavingRow = SelectLeavingVariable(currentTable, enteringColumn);
            if (leavingRow == -1)
            {
                var unboundedTable = new Table($"{currentTable.TableId}-unbounded", currentTable, "Unbounded");
                TableCache.StoreTable(unboundedTable);
                return unboundedTable;
            }

            // Step 4: Perform pivot operation
            Table newTable = PerformPivotOperation(currentTable, leavingRow, enteringColumn);

            // Step 5: Update basic variables list
            UpdateBasicVariables(newTable, leavingRow, enteringColumn);

            return newTable;
        }

        public int SelectEnteringVariable(Table table)
        {
            // Choose entering variable (most negative in objective row)
            // Return column index of entering variable
            int objRow = 0;
            int rhsCol = table.GetColumnCount() - 1;
            int bestColumn = -1;
            double bestValue = 0;

            for (int j = 0; j < rhsCol; j++) // Exclude RHS column
            {
                double coefficient = table.GetElement(objRow, j);

                if (table.OptimizationType == OptimizationType.Maximize)
                {
                    // Most negative coefficient for maximization
                    if (coefficient < bestValue)
                    {
                        bestValue = coefficient;
                        bestColumn = j;
                    }
                }
                else // Minimize
                {
                    // Most positive coefficient for minimization
                    if (coefficient > bestValue)
                    {
                        bestValue = coefficient;
                        bestColumn = j;
                    }
                }
            }

            return bestColumn;
        }

        public int SelectLeavingVariable(Table table, int enteringColumn)
        {
            // Choose leaving variable using minimum ratio test
            // Return row index of leaving variable
            int bestRow = -1;
            double minRatio = double.PositiveInfinity;
            int rhsCol = table.GetColumnCount() - 1;

            for (int i = 1; i < table.GetRowCount(); i++) // Skip objective row
            {
                double pivotElement = table.GetElement(i, enteringColumn);
                double rhsValue = table.GetElement(i, rhsCol);

                if (pivotElement > 0.001) // Only positive pivot elements
                {
                    double ratio = rhsValue / pivotElement;
                    if (ratio >= 0 && ratio < minRatio)
                    {
                        minRatio = ratio;
                        bestRow = i;
                    }
                }
            }

            return bestRow;
        }

        public bool IsOptimal(Table table)
        {
            // Check if current table is optimal
            // All coefficients in objective row should be non-negative

            int objRow = 0; // Objective is always first row
            int rhsCol = table.GetColumnCount() - 1; // Exclude RHS column

            for (int j = 0; j < rhsCol; j++)
            {
                double coefficient = table.GetElement(objRow, j);

                if (table.OptimizationType == OptimizationType.Maximize)
                {
                    if (coefficient < -0.001) // Negative coefficient means not optimal
                        return false;
                }
                else // Minimize
                {
                    if (coefficient > 0.001) // Positive coefficient means not optimal
                        return false;
                }
            }

            return true;
        }

        public bool IsUnbounded(Table table, int enteringColumn)
        {
            // Check if problem is unbounded
            // All coefficients in entering column should be non-positive
            // Problem is unbounded if all coefficients in entering column are <= 0
            // (excluding objective row)

            for (int i = 1; i < table.GetRowCount(); i++) // Skip objective row
            {
                if (table.GetElement(i, enteringColumn) > 0.001)
                {
                    return false; // Found positive coefficient, not unbounded
                }
            }

            return true; // All coefficients <= 0, unbounded
        }

        /*public Table CreateCanonicalForm(LinearProgrammingModel model)
        {
            // Convert LP model to canonical form table
            // Add slack variables and set up initial tableau

            int numVars = model.Variables.Count;
            int numConstraints = model.Constraints.Count;

            // Count auxiliary variables
            int numSlacks = 0, numSurplus = 0, numArtificials = 0;
            foreach (var cons in model.Constraints)
            {
                switch (cons.Type)
                {
                    case ConstraintType.LessThanOrEqual:
                        numSlacks++;
                        break;
                    case ConstraintType.GreaterThanOrEqual:
                        numSurplus++;
                        numArtificials++;
                        break;
                    case ConstraintType.Equal:
                        numArtificials++;
                        break;
                }
            }

            int totalCols = numVars + numSlacks + numSurplus + numArtificials + 1;  // + RHS
            int totalRows = 1 + numConstraints;  // Obj + constraints

            // Row labels: OBJ + constraint names
            List<string> rowLabels = new List<string> { "OBJ" };
            rowLabels.AddRange(model.Constraints.Select(c => c.Name ?? $"C{model.Constraints.IndexOf(c) + 1}"));

            Table table = new Table("t-i", new double[totalRows, totalCols], rowLabels, new List<string>(), model.OptimizationType, "Canonical");

            // Column labels: decision vars + aux + RHS
            List<string> colLabels = model.Variables.Select(v => v.Name ?? $"x{model.Variables.IndexOf(v) + 1}").ToList();
            for (int i = 1; i <= numSlacks; i++) colLabels.Add($"s{i}");
            for (int i = 1; i <= numSurplus; i++) colLabels.Add($"e{i}");
            for (int i = 1; i <= numArtificials; i++) colLabels.Add($"a{i}");
            colLabels.Add("RHS");
            table.ColumnLabels = colLabels;

            // Set objective row: -coeff for max, +coeff for min
            double signObj = model.OptimizationType == OptimizationType.Maximize ? -1.0 : 1.0;
            for (int j = 0; j < numVars; j++)
            {
                table.SetElement(0, j, signObj * model.Variables[j].Coefficient);
            }

            // Set auxiliary obj coeffs: 0 for slack/surplus, Big M for artificials
            double M = 10000.0;
            double artificialObj = model.OptimizationType == OptimizationType.Maximize ? -M : M;
            int auxCol = numVars;
            for (int i = 0; i < numSlacks; i++) table.SetElement(0, auxCol++, 0.0);
            for (int i = 0; i < numSurplus; i++) table.SetElement(0, auxCol++, 0.0);
            for (int i = 0; i < numArtificials; i++) table.SetElement(0, auxCol++, artificialObj);

            // Set constraint rows and operators
            Dictionary<string, ConstraintOperator> ops = new Dictionary<string, ConstraintOperator>();
            List<string> basicVars = new List<string>();
            int slackIdx = 0, surplusIdx = 0, artificialIdx = 0;

             // ADDED: Track artificial contributions for Big-M adjustment
            double[] artificialContributions = new double[numConstraints];
            for (int r = 0; r < numConstraints; r++)
            {
                var cons = model.Constraints[r];
                int row = r + 1;  // Offset for obj
                string rowName = rowLabels[row];

                // Decision var coeffs
                for (int j = 0; j < numVars; j++)
                {
                    table.SetElement(row, j, cons.Coefficients[j]);
                }

                // Aux vars
                auxCol = numVars;
                switch (cons.Type)
                {
                    case ConstraintType.LessThanOrEqual:
                        table.SetElement(row, auxCol + slackIdx, 1.0);
                        basicVars.Add($"s{slackIdx + 1}");
                        slackIdx++;
                        ops[rowName] = ConstraintOperator.LessThanOrEqual;
                        break;
                    case ConstraintType.GreaterThanOrEqual:
                        table.SetElement(row, auxCol + numSlacks + surplusIdx, -1.0);
                        table.SetElement(row, auxCol + numSlacks + numSurplus + artificialIdx, 1.0);
                        basicVars.Add($"a{artificialIdx + 1}");
                        surplusIdx++;
                        artificialIdx++;
                        ops[rowName] = ConstraintOperator.GreaterThanOrEqual;
                        break;
                    case ConstraintType.Equal:
                        table.SetElement(row, auxCol + numSlacks + numSurplus + artificialIdx, 1.0);
                        basicVars.Add($"a{artificialIdx + 1}");
                        artificialIdx++;
                        ops[rowName] = ConstraintOperator.Equal;
                        break;
                }

                // RHS
                table.SetElement(row, totalCols - 1, cons.RightHandSide);
            }

            // ADDED: Adjust objective row for artificial variables in basis
            for (int r = 0; r < numConstraints; r++)
            {
                if (basicVars[r].StartsWith("a"))
                {
                    int row = r + 1;
                    for (int j = 0; j < totalCols - 1; j++)
                    {
                        double currentObj = table.GetElement(0, j);
                        double constraintCoeff = table.GetElement(row, j);
                        table.SetElement(0, j, currentObj + artificialObj * constraintCoeff);
                    }
                    table.SetElement(0, totalCols - 1, table.GetElement(0, totalCols - 1) + artificialObj * artificialContributions[r]);
                }
            }

            //Finalize table 
            table.BasicVariables = basicVars;
            table.ConstraintOperators = ops;
            TableCache.StoreTable(table);

            return table;
        } */

        public void DisplayCanonicalForm(Table table)
        {
            Console.WriteLine("Canonical Form:");
            table.DisplayTraditional();
        }

        public void DisplayAllIterations(List<Table> allTables)
        {
            foreach (var table in allTables)
            {
                Console.WriteLine($"Table {table.TableId} ({table.Status}):");
                table.DisplayTraditional();  
            }
        }

        /* //Removed to avoid false positives
        
              private bool IsInfeasible(Table table)
        {
            int rhsCol = table.GetColumnCount() - 1;
            for (int i = 1; i < table.GetRowCount(); i++)
            {
                if (table.GetElement(i, rhsCol) < -0.001) return true;
            }
            return false;
        } */


        /// Post-optimal check for infeasibility (artificial variables positive).
        private bool IsInfeasiblePostOptimal(Table table)
        {
            int rhsCol = table.GetColumnCount() - 1;
            for (int i = 0; i < table.BasicVariables.Count; i++)
            {
                string bv = table.BasicVariables[i];
                if (bv.StartsWith("a") && table.GetElement(i + 1, rhsCol) > 0.001)
                {
                    return true;
                }
            }
            return false;
        }

        /// Performs Gaussian elimination pivot.
        /// Creates new table to avoid modifying original.
        private Table PerformPivotOperation(Table currentTable, int pivotRow, int pivotColumn)
        {
            // Create new table with same structure
            Table newTable = new Table($"temp", currentTable);
    
            double pivotElement = currentTable.GetElement(pivotRow, pivotColumn);
    
            // Step 1: Normalize pivot row
            for (int j = 0; j < newTable.GetColumnCount(); j++)
            {
                double value = currentTable.GetElement(pivotRow, j) / pivotElement;
                newTable.SetElement(pivotRow, j, value);
            }
    
            // Step 2: Eliminate other rows
            for (int i = 0; i < newTable.GetRowCount(); i++)
            {
                if (i != pivotRow) // Don't modify pivot row
                {
                    double multiplier = currentTable.GetElement(i, pivotColumn);
            
                    for (int j = 0; j < newTable.GetColumnCount(); j++)
                    {
                        double currentValue = currentTable.GetElement(i, j);
                        double pivotRowValue = newTable.GetElement(pivotRow, j);
                        double newValue = currentValue - (multiplier * pivotRowValue);
                        newTable.SetElement(i, j, newValue);
                    }
                }
            }
    
            return newTable;
        }

        /// Updates basic variables after pivot.
        private void UpdateBasicVariables(Table table, int leavingRow, int enteringColumn)
        {
            // Replace leaving variable with entering variable in basic variables list
            int constraintIndex = leavingRow - 1; // Skip objective row
            string enteringVariable = table.ColumnLabels[enteringColumn];
    
            if (constraintIndex >= 0 && constraintIndex < table.BasicVariables.Count)
            {
                table.BasicVariables[constraintIndex] = enteringVariable;
            }
        }  
    }
}