using System;
using System.Collections.Generic;
using LinearProgrammingSolver.Tables;

namespace LinearProgrammingSolver.Algorithms.Implementations.LP
{
    public class DualSimplexAlgorithm
    {
        public Table SolveLP(Table initialTable)
        {
            if (initialTable == null)
            {
                return null;
            }

            Table currentTable = initialTable;
            int iterationCount = 1;

            while (!IsFeasible(currentTable))
            {
                Table nextTable = PerformIteration(currentTable);

                if (nextTable.Status == "Infeasible")
                {
                    TableCache.StoreTable(nextTable);
                    return nextTable;
                }

                // For iterations, preserve the original ID and append iteration count
                string baseId = nextTable.TableId;
                nextTable.TableId = $"{baseId}{iterationCount}";
                nextTable.Status = "Iteration";
                TableCache.StoreTable(nextTable);

                currentTable = nextTable;
                iterationCount++;

                if (iterationCount > 50)
                {
                    currentTable.Status = "Max_Iterations_Reached";
                    TableCache.StoreTable(currentTable);
                    return currentTable;
                }
            }

            // Table is now feasible - mark as optimal and store
            currentTable.Status = "Optimal";
            TableCache.StoreTable(currentTable);
            return currentTable;
        }

        public Table PerformIteration(Table currentTable)
        {
            int leavingRow = SelectLeavingVariable(currentTable);
            if (leavingRow == -1)
            {
                var infeasibleTable = new Table($"{currentTable.TableId}-infeasible", currentTable, "Infeasible");
                return infeasibleTable;
            }

            int enteringColumn = SelectEnteringVariable(currentTable, leavingRow);
            if (enteringColumn == -1)
            {
                var infeasibleTable = new Table($"{currentTable.TableId}-infeasible", currentTable, "Infeasible");
                return infeasibleTable;
            }

            Table newTable = PerformPivotOperation(currentTable, leavingRow, enteringColumn);
            UpdateBasicVariables(newTable, leavingRow, enteringColumn);

            return newTable;
        }

        public int SelectLeavingVariable(Table table)
        {
            int rhsCol = table.GetColumnCount() - 1;
            int bestRow = -1;
            double mostNegative = 0;

            for (int i = 1; i < table.GetRowCount(); i++)
            {
                double rhsValue = table.GetElement(i, rhsCol);
                if (rhsValue < mostNegative)
                {
                    mostNegative = rhsValue;
                    bestRow = i;
                }
            }

            return bestRow;
        }

        public int SelectEnteringVariable(Table table, int leavingRow)
        {
            int rhsCol = table.GetColumnCount() - 1;
            int bestColumn = -1;
            double bestRatio = double.NegativeInfinity;
            
            // Check if this is a Branch & Bound constraint row (has negative RHS)
            double rhsValue = table.GetElement(leavingRow, rhsCol);
            bool isBranchConstraint = rhsValue < -0.001;
            
            if (isBranchConstraint)
            {
                // Check if this is a B-side constraint by looking for excess variables (e prefix)
                bool isBSideConstraint = false;
                for (int j = 0; j < table.ColumnLabels.Count && j < rhsCol; j++)
                {
                    string colLabel = table.ColumnLabels[j];
                    double element = table.GetElement(leavingRow, j);
                    if (colLabel.StartsWith("e") && Math.Abs(element - 1.0) < 0.001)
                    {
                        isBSideConstraint = true;
                        break;
                    }
                }
                
                if (isBSideConstraint)
                {
                    // B-side constraint: Allow normal dual simplex pivoting
                    for (int j = 0; j < rhsCol; j++)
                    {
                        double leavingRowElement = table.GetElement(leavingRow, j);
                        double objectiveElement = table.GetElement(0, j);

                        if (leavingRowElement < -0.001)
                        {
                            double ratio = objectiveElement / leavingRowElement;
                            if (ratio > bestRatio)
                            {
                                bestRatio = ratio;
                                bestColumn = j;
                            }
                        }
                    }
                }
                else
                {
                    // A-side constraint: Allow normal dual simplex pivoting
                    for (int j = 0; j < rhsCol; j++)
                    {
                        double leavingRowElement = table.GetElement(leavingRow, j);
                        double objectiveElement = table.GetElement(0, j);

                        if (leavingRowElement < -0.001)
                        {
                            double ratio = objectiveElement / leavingRowElement;
                            if (ratio > bestRatio)
                            {
                                bestRatio = ratio;
                                bestColumn = j;
                            }
                        }
                    }
                }
            }
            else
            {
                // Standard dual simplex pivot selection for non-B&B constraints
                for (int j = 0; j < rhsCol; j++)
                {
                    double leavingRowElement = table.GetElement(leavingRow, j);
                    double objectiveElement = table.GetElement(0, j);

                    if (leavingRowElement < -0.001)
                    {
                        double ratio = objectiveElement / leavingRowElement;
                        if (ratio > bestRatio) // Changed from < to > to select maximum ratio
                        {
                            bestRatio = ratio;
                            bestColumn = j;
                        }
                    }
                }
            }

            return bestColumn;
        }

        public bool IsFeasible(Table table)
        {
            int rhsCol = table.GetColumnCount() - 1;
            for (int i = 1; i < table.GetRowCount(); i++)
            {
                if (table.GetElement(i, rhsCol) < -0.001)
                {
                    return false;
                }
            }
            return true;
        }

        private Table PerformPivotOperation(Table currentTable, int pivotRow, int pivotColumn)
        {
            // Preserve the original table ID from Branch & Bound
            string newTableId = string.IsNullOrEmpty(currentTable.TableId) ? "temp" : currentTable.TableId;
            Table newTable = new Table(newTableId, currentTable);
    
            double pivotElement = currentTable.GetElement(pivotRow, pivotColumn);
    
            for (int j = 0; j < newTable.GetColumnCount(); j++)
            {
                double value = currentTable.GetElement(pivotRow, j) / pivotElement;
                newTable.SetElement(pivotRow, j, value);
            }
    
            for (int i = 0; i < newTable.GetRowCount(); i++)
            {
                if (i != pivotRow)
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

        private void UpdateBasicVariables(Table table, int leavingRow, int enteringColumn)
        {
            int constraintIndex = leavingRow - 1;
            string enteringVariable = table.ColumnLabels[enteringColumn];
    
            if (constraintIndex >= 0 && constraintIndex < table.BasicVariables.Count)
            {
                table.BasicVariables[constraintIndex] = enteringVariable;
            }
        }
    }
}