using System;
using System.Collections.Generic;
using System.Linq;
using LinearProgrammingSolver.Tables;
using LinearProgrammingSolver.Algorithms.Implementations.LP;

namespace LinearProgrammingSolver.Algorithms.Implementations.IP
{
    public class BranchAndBoundAlgorithm
    {
        private DualSimplexAlgorithm _dualSimplex;
        private List<Table> _allSubproblems;
        private Table _bestIntegerSolution;
        private Dictionary<string, string> _fathomReasons;
        private List<string> _processingOrder;
        private Stack<Table> _pendingNodes;

        public BranchAndBoundAlgorithm()
        {
            _dualSimplex = new DualSimplexAlgorithm();
            _allSubproblems = new List<Table>();
            _bestIntegerSolution = null;
            _fathomReasons = new Dictionary<string, string>();
            _processingOrder = new List<string>();
            _pendingNodes = new Stack<Table>();
        }

        public Table SolveIP(Table lpOptimalTable)
        {
            if (lpOptimalTable == null || !lpOptimalTable.IsOptimal())
            {
                return null;
            }

            if (IsIntegerSolution(lpOptimalTable))
            {
                _bestIntegerSolution = lpOptimalTable;
                return lpOptimalTable;
            }

            // Initialize LIFO stack and tracking
            _pendingNodes.Clear();
            _processingOrder.Clear();
            _fathomReasons.Clear();
            _bestIntegerSolution = null;

            // Start with initial subproblems
            var initialSubproblems = BranchOnVariable(lpOptimalTable, SelectBranchingVariable(lpOptimalTable));
            for (int i = initialSubproblems.Count - 1; i >= 0; i--)
            {
                _pendingNodes.Push(initialSubproblems[i]);
            }

            // LIFO processing
            int maxIterations = 20; // Safety limit for debugging
            int currentIteration = 0;
            
            while (_pendingNodes.Count > 0 && currentIteration < maxIterations)
            {
                Table currentNode = _pendingNodes.Pop();
                _processingOrder.Add($"Processing: {currentNode.TableId}");
                _allSubproblems.Add(currentNode);
                currentIteration++;

                // Apply DualSimplex to restore feasibility
                _processingOrder.Add($"  Before DualSimplex: {currentNode.GetObjectiveValue():F3}");
                
                // Debug: Check feasibility and RHS values for B-B subproblems
                if (currentNode.TableId.Contains("-B") && !currentNode.TableId.Contains("-A"))
                {
                    bool isFeasible = _dualSimplex.IsFeasible(currentNode);
                    _processingOrder.Add($"  DEBUG: Table {currentNode.TableId} is feasible: {isFeasible}");
                    
                    // Show RHS values
                    int rhsCol = currentNode.GetColumnCount() - 1;
                    var rhsValues = new List<string>();
                    for (int i = 1; i < currentNode.GetRowCount(); i++)
                    {
                        rhsValues.Add($"R{i}:{currentNode.GetElement(i, rhsCol):F3}");
                    }
                    _processingOrder.Add($"  DEBUG: RHS values: {string.Join(", ", rhsValues)}");
                }
                
                Table solvedNode = _dualSimplex.SolveLP(currentNode);
                _processingOrder.Add($"  After DualSimplex: {solvedNode.GetObjectiveValue():F3}");

                // Debug: Show final table for B1-A1 case
                if (solvedNode.TableId.Contains("B1-A1"))
                {
                    _processingOrder.Add($"  DEBUG: Final table {solvedNode.TableId}:");
                    var tableLines = solvedNode.ToString().Split('\n');
                    foreach (var line in tableLines)
                    {
                        if (!string.IsNullOrWhiteSpace(line))
                            _processingOrder.Add($"    {line}");
                    }
                    _processingOrder.Add($"  DEBUG: Basic variables:");
                    for (int i = 0; i < solvedNode.BasicVariables.Count; i++)
                    {
                        var varName = solvedNode.BasicVariables[i];
                        var value = solvedNode.GetElement(i + 1, solvedNode.GetColumnCount() - 1);
                        _processingOrder.Add($"    {varName} = {value:F6}");
                    }
                }

                // Check fathoming conditions
                if (ShouldFathom(solvedNode, _bestIntegerSolution, out string reason))
                {
                    FathomNode(solvedNode, reason);
                    _fathomReasons[solvedNode.TableId] = reason;
                    _processingOrder.Add($"Fathomed: {solvedNode.TableId} - {reason}");
                    continue;
                }
                
                // Debug: Log objective value for non-fathomed nodes
                _processingOrder.Add($"  Objective: {solvedNode.GetObjectiveValue():F3} (not fathomed)");

                // Check if integer solution
                if (IsIntegerSolution(solvedNode))
                {
                    UpdateBestSolution(solvedNode, ref _bestIntegerSolution);
                    _processingOrder.Add($"Integer solution: {solvedNode.TableId} - Obj: {solvedNode.GetObjectiveValue():F3}");
                    continue;
                }

                // Branch further (fractional solution)
                var branchingInfo = SelectBranchingVariable(solvedNode);
                string branchingVarName = branchingInfo?.VariableName ?? "NONE";
                double branchingVarValue = branchingInfo?.CurrentValue ?? 0.0;
                _processingOrder.Add($"Branching from: {solvedNode.TableId} on {branchingVarName} = {branchingVarValue:F3}");
                
                // Debug: Show the table before branching
                if (solvedNode.TableId.Contains("B1"))
                {
                    _processingOrder.Add($"  DEBUG: Table {solvedNode.TableId} before branching:");
                    var tableLines = solvedNode.ToString().Split('\n');
                    foreach (var line in tableLines)
                    {
                        if (!string.IsNullOrWhiteSpace(line))
                            _processingOrder.Add($"    {line}");
                    }
                    
                    // Show the basic variables explicitly
                    _processingOrder.Add($"  DEBUG: Basic variables:");
                    for (int i = 0; i < solvedNode.BasicVariables.Count; i++)
                    {
                        var varName = solvedNode.BasicVariables[i];
                        var value = solvedNode.GetElement(i + 1, solvedNode.GetColumnCount() - 1);
                        _processingOrder.Add($"    {varName} = {value:F3}");
                    }
                }
                
                var newSubproblems = BranchOnVariable(solvedNode, branchingInfo);
                for (int i = newSubproblems.Count - 1; i >= 0; i--)
                {
                    _pendingNodes.Push(newSubproblems[i]);
                }
            }

            if (currentIteration >= maxIterations)
            {
                _processingOrder.Add($"WARNING: Maximum iterations ({maxIterations}) reached. Stopping algorithm.");
            }

            return _bestIntegerSolution;
        }

        public List<Table> BranchOnVariable(Table parentTable, BranchingVariableInfo branchingInfo)
        {
            var subproblems = new List<Table>();
            if (branchingInfo == null) return subproblems;
            

            var subproblemA = CreateSubproblemWithConstraint(parentTable, branchingInfo, branchingInfo.FloorValue, true, "A");
            var subproblemB = CreateSubproblemWithConstraint(parentTable, branchingInfo, branchingInfo.CeilValue, false, "B"); 

            subproblems.Add(subproblemA);
            subproblems.Add(subproblemB);

            return subproblems;
        }

        public BranchingVariableInfo SelectBranchingVariable(Table table)
        {
            double closestToHalf = double.MaxValue;
            BranchingVariableInfo bestBranchingInfo = null;

            for (int i = 0; i < table.BasicVariables.Count; i++)
            {
                string varName = table.BasicVariables[i];
                
                if (varName.StartsWith("x"))
                {
                    double value = table.GetElement(i + 1, table.GetColumnCount() - 1);
                    double fractionalPart = Math.Abs(value - Math.Round(value));
                    
                    if (fractionalPart > 0.001)
                    {
                        double distanceFromHalf = Math.Abs(fractionalPart - 0.5);
                        int varIndex = table.ColumnLabels.IndexOf(varName);
                        
                        if (distanceFromHalf < closestToHalf || 
                           (Math.Abs(distanceFromHalf - closestToHalf) < 0.001 && (bestBranchingInfo == null || varIndex < bestBranchingInfo.ColumnIndex)))
                        {
                            closestToHalf = distanceFromHalf;
                            bestBranchingInfo = new BranchingVariableInfo
                            {
                                VariableName = varName,
                                ColumnIndex = varIndex,
                                BasicRowIndex = i + 1,  // Row index where this variable is basic
                                CurrentValue = value,
                                FractionalPart = fractionalPart,
                                DistanceFromHalf = distanceFromHalf
                            };
                        }
                    }
                }
            }

            return bestBranchingInfo;
        }

        public bool IsIntegerSolution(Table table)
        {
            for (int i = 0; i < table.BasicVariables.Count; i++)
            {
                string varName = table.BasicVariables[i];
                
                if (varName.StartsWith("x"))
                {
                    double value = table.GetElement(i + 1, table.GetColumnCount() - 1);
                    if (Math.Abs(value - Math.Round(value)) > 0.001)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private bool ShouldFathom(Table table, Table currentBest, out string reason)
        {
            reason = "";
            
            // Rule 1: Infeasible (DualSimplex couldn't restore feasibility)
            if (table.Status == "Infeasible") 
            {
                reason = "Infeasible subproblem";
                return true;
            }
                
            // Rule 2: Bound (worse than current best integer solution)
            if (currentBest != null)
            {
                double currentObj = table.GetObjectiveValue();
                double bestObj = currentBest.GetObjectiveValue();
                
                if (table.OptimizationType == OptimizationType.Maximize && currentObj <= bestObj)
                {
                    reason = $"Bound: {currentObj:F3} ≤ {bestObj:F3}";
                    return true;
                }
                else if (table.OptimizationType == OptimizationType.Minimize && currentObj >= bestObj)
                {
                    reason = $"Bound: {currentObj:F3} ≥ {bestObj:F3}";
                    return true;
                }
            }
            
            // Rule 3: Integer solution (handled separately in main loop)
            return false;
        }

        public void FathomNode(Table table, string reason)
        {
            table.Status = $"Fathomed: {reason}";
        }

        private void UpdateBestSolution(Table candidateTable, ref Table bestIntegerSolution)
        {
            if (!IsIntegerSolution(candidateTable)) return;
            
            if (bestIntegerSolution == null)
            {
                bestIntegerSolution = candidateTable;
                return;
            }

            double candidateObjective = candidateTable.GetObjectiveValue();
            double bestObjective = bestIntegerSolution.GetObjectiveValue();

            if (candidateTable.OptimizationType == OptimizationType.Maximize)
            {
                if (candidateObjective > bestObjective)
                {
                    bestIntegerSolution = candidateTable;
                }
            }
            else
            {
                if (candidateObjective < bestObjective)
                {
                    bestIntegerSolution = candidateTable;
                }
            }
        }

        private Table CreateSubproblemWithConstraint(Table parentTable, BranchingVariableInfo branchingInfo, int boundValue, bool isUpperBound, string suffix)
        {
            int oldRows = parentTable.GetRowCount();
            int oldCols = parentTable.GetColumnCount();
            
            var newTable = new Table($"{parentTable.TableId}-{suffix}", parentTable);
            
            int newRows = oldRows + 1;
            int newCols = oldCols + 1;
            int newAuxColIndex = oldCols - 1;  // New auxiliary variable column position
            int newRhsColIndex = newCols - 1;  // New RHS column position
            
            var newMatrix = new double[newRows, newCols];
            
            // Copy existing rows with proper column handling
            for (int i = 0; i < oldRows; i++)
            {
                // Copy columns 0 to oldCols-2 (all columns except old RHS)
                for (int j = 0; j < oldCols - 1; j++)
                {
                    newMatrix[i, j] = parentTable.GetElement(i, j);
                }
                
                // Set new auxiliary variable column to 0 for existing rows
                newMatrix[i, newAuxColIndex] = 0.0;
                
                // Copy old RHS to new RHS position
                newMatrix[i, newRhsColIndex] = parentTable.GetElement(i, oldCols - 1);
            }
            
            // Set up new constraint row
            for (int j = 0; j < newCols; j++)
            {
                if (j == branchingInfo.ColumnIndex)
                {
                    // Branching variable coefficient = 1
                    newMatrix[oldRows, j] = 1.0;
                }
                else if (j == newAuxColIndex)
                {
                    // Different coefficients for ≤ and ≥ constraints
                    // x ≤ b: x + s = b (slack variable s ≥ 0)  
                    // x ≥ b: x - e = b (excess variable e ≥ 0)
                    newMatrix[oldRows, j] = isUpperBound ? 1.0 : -1.0;
                }
                else if (j == newRhsColIndex)
                {
                    // RHS = bound value
                    newMatrix[oldRows, j] = boundValue;
                }
                else
                {
                    // All other coefficients = 0
                    newMatrix[oldRows, j] = 0.0;
                }
            }
            
            newTable.Matrix = newMatrix;
            
            var newRowLabels = new List<string>(parentTable.RowLabels);
            newRowLabels.Add($"C{oldRows}");
            newTable.RowLabels = newRowLabels;
            
            var newColLabels = new List<string>(parentTable.ColumnLabels);
            newColLabels.Insert(newColLabels.Count - 1, isUpperBound ? $"s{oldRows}" : $"e{oldRows}");
            newTable.ColumnLabels = newColLabels;
            
            var newBasicVars = new List<string>(parentTable.BasicVariables);
            newTable.BasicVariables = newBasicVars;
            
            // CRITICAL: Perform row manipulation BEFORE adding new basic variable
            // This ensures we use the original basic variables list
            PerformRowManipulation(newTable, branchingInfo, oldRows, isUpperBound);
            
            // Now add the new basic variable after manipulation
            newTable.BasicVariables.Add(isUpperBound ? $"s{oldRows}" : $"e{oldRows}");
            
            return newTable;
        }

        public List<string> GetProcessingOrder()
        {
            return new List<string>(_processingOrder);
        }

        public Dictionary<string, string> GetFathomReasons()
        {
            return new Dictionary<string, string>(_fathomReasons);
        }

        public List<Table> GetAllSubproblems()
        {
            return new List<Table>(_allSubproblems);
        }

        public Table GetBestIntegerSolution()
        {
            return _bestIntegerSolution;
        }

        private void PerformRowManipulation(Table table, BranchingVariableInfo branchingInfo, int newConstraintRow, bool isUpperBound)
        {
            // Use pre-stored basic row information - no searching needed!
            int basicVarRow = branchingInfo.BasicRowIndex;
            
            // Different row manipulations for ≤ and ≥ constraints
            if (isUpperBound)
            {
                // For x ≤ bound (Subproblem A): (basic_row - constraint_row) * -1
                for (int j = 0; j < table.GetColumnCount(); j++)
                {
                    double basicRowValue = table.GetElement(basicVarRow, j);
                    double constraintValue = table.GetElement(newConstraintRow, j);
                    double newValue = (basicRowValue - constraintValue) * -1.0;
                    table.SetElement(newConstraintRow, j, newValue);
                }
            }
            else
            {
                // For x ≥ bound (Subproblem B): basic_row - constraint_row
                for (int j = 0; j < table.GetColumnCount(); j++)
                {
                    double basicRowValue = table.GetElement(basicVarRow, j);
                    double constraintValue = table.GetElement(newConstraintRow, j);
                    double newValue = basicRowValue - constraintValue;
                    table.SetElement(newConstraintRow, j, newValue);
                }
            }
        }
    }
}