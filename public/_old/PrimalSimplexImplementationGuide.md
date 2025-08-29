# Primal Simplex Algorithm Implementation Guide

## Overview
This guide explains how to implement the `PrimalSimplexAlgorithm.cs` using traditional tableau method (not revised simplex) to work with the existing Table/TableCache architecture.

## Current System Understanding

### Existing Components
- **TableCache.cs**: Stores all tables with unique IDs (`t-raw`, `t-i`, `t-1`, `t-2`, etc.)
- **Table.cs**: Contains tableau data, matrix, labels, basic variables, status
- **Program.cs**: Main coordinator that creates FileReader → CanonicalConverter → stores tables

### Current Output
The system currently creates:
- `t-raw`: Raw input table (4x3)
- `t-i`: Canonical form table (4x6) - Ready for Simplex

## Implementation Strategy

<div style="page-break-before: always;"></div>

### Main Method Integration
Add this to `Program.cs` after the canonical table creation:

```csharp
// Step 4: Run Primal Simplex Algorithm
var simplexSolver = new PrimalSimplexAlgorithm();
var optimalTable = simplexSolver.SolveLP(TableCache.GetTable("t-i"));
Console.WriteLine("✓ Simplex algorithm completed");
```

<div style="page-break-before: always;"></div>

### Core Implementation Structure

#### 1. SolveLP Method (Main Controller)
```csharp
public Table SolveLP(Table initialTable)
{
    if (initialTable == null) return null;
    
    Table currentTable = initialTable;
    int iterationCount = 1;
    
    while (!IsOptimal(currentTable))
    {
        // Check for infeasibility
        if (IsInfeasible(currentTable))
        {
            currentTable.Status = "Infeasible";
            TableCache.StoreTable(currentTable);
            return currentTable;
        }
        
        // Perform one iteration
        Table nextTable = PerformIteration(currentTable);
        
        // Name and store the iteration
        nextTable.TableId = $"t-{iterationCount}";
        nextTable.Status = "Iteration";
        TableCache.StoreTable(nextTable);
        
        currentTable = nextTable;
        iterationCount++;
        
        // Safety check to prevent infinite loops
        if (iterationCount > 50)
        {
            currentTable.Status = "Max_Iterations_Reached";
            break;
        }
    }
    
    // Mark final table as optimal
    currentTable.Status = "Optimal";
    currentTable.TableId = "t-optimal";
    TableCache.StoreTable(currentTable);
    
    return currentTable;
}
```

<div style="page-break-before: always;"></div>

#### 2. IsOptimal Check (Traditional Method)
```csharp
public bool IsOptimal(Table table)
{
    // For maximization: optimal when all objective coefficients >= 0
    // For minimization: optimal when all objective coefficients <= 0
    
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
```

<div style="page-break-before: always;"></div>

#### 3. PerformIteration Method (Traditional Tableau)
```csharp
public Table PerformIteration(Table currentTable)
{
    // Step 1: Select entering variable (most negative for max)
    int enteringColumn = SelectEnteringVariable(currentTable);
    if (enteringColumn == -1) return currentTable; // Already optimal
    
    // Step 2: Check for unboundedness
    if (IsUnbounded(currentTable, enteringColumn))
    {
        var unboundedTable = new Table($"{currentTable.TableId}-unbounded", currentTable, "Unbounded");
        return unboundedTable;
    }
    
    // Step 3: Select leaving variable (minimum ratio test)
    int leavingRow = SelectLeavingVariable(currentTable, enteringColumn);
    if (leavingRow == -1)
    {
        var unboundedTable = new Table($"{currentTable.TableId}-unbounded", currentTable, "Unbounded");
        return unboundedTable;
    }
    
    // Step 4: Perform pivot operation
    Table newTable = PerformPivotOperation(currentTable, leavingRow, enteringColumn);
    
    // Step 5: Update basic variables list
    UpdateBasicVariables(newTable, leavingRow, enteringColumn);
    
    return newTable;
}
```

<div style="page-break-before: always;"></div>

#### 4. Key Helper Methods (Traditional Tableau)

**Select Entering Variable (Dantzig's Rule):**
```csharp
public int SelectEnteringVariable(Table table)
{
    // Choose most negative coefficient in objective row (for maximization)
    // Choose most positive coefficient in objective row (for minimization)
    
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
```

<div style="page-break-before: always;"></div>

**Check Unboundedness:**
```csharp
public bool IsUnbounded(Table table, int enteringColumn)
{
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
```

**Select Leaving Variable (Minimum Ratio Test):**
```csharp
public int SelectLeavingVariable(Table table, int enteringColumn)
{
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
```

<div style="page-break-before: always;"></div>

**Perform Pivot Operation (Traditional Gaussian Elimination):**
```csharp
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
```

**Update Basic Variables:**
```csharp
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
```

<div style="page-break-before: always;"></div>

## Integration with Table System

### Table Naming Convention
- `t-i`: Initial canonical form (already exists)
- `t-1`: First iteration  
- `t-2`: Second iteration
- `t-n`: nth iteration
- `t-optimal`: Final optimal solution

### Status Updates
- `"Iteration"`: Intermediate iteration tables
- `"Optimal"`: Final optimal solution
- `"Infeasible"`: No feasible solution exists
- `"Unbounded"`: Objective can increase infinitely

### Display Integration
The existing `TableCache.DisplayAllTablesDetailed()` will automatically show:
1. Raw table
2. Canonical table  
3. All iteration tables (t-1, t-2, etc.)
4. Final optimal table

## Testing Strategy

1. **Start with t-i table**: Use `TableCache.GetTable("t-i")` 
2. **Verify each iteration**: Check that basic variables update correctly
3. **Check optimality**: Verify reduced costs meet optimality conditions
4. **Validate tableau operations**: Ensure pivoting maintains feasibility

<div style="page-break-before: always;"></div>

## Usage Examples

### Standalone Usage
```csharp
var simplexSolver = new PrimalSimplexAlgorithm();
var initialTable = TableCache.GetTable("t-i");
var optimalTable = simplexSolver.SolveLP(initialTable);
```

### For Branch & Bound Integration
```csharp
// Branch & Bound can use the same SolveLP method
var branchTable = TableCache.GetTable("t-1.1"); // Branch node
var optimalBranchTable = simplexSolver.SolveLP(branchTable);
```

## Key Implementation Notes

1. **Traditional Pivoting**: Use Gaussian elimination for tableau operations
2. **Numerical Stability**: Use small tolerances (0.001) for floating-point comparisons  
3. **Error Handling**: Check for degenerate cases and cycling
4. **Degeneracy**: Handle ties in minimum ratio test (choose first occurrence)
5. **Debugging**: Add verbose output options to trace each pivot operation

<div style="page-break-before: always;"></div>

## Traditional Simplex vs Revised Simplex

**This implementation uses Traditional Simplex:**
- Works directly with full tableau
- Performs Gaussian elimination on entire matrix
- Easier to understand and debug
- Shows all intermediate steps clearly

**NOT using Revised Simplex which would:**
- Use basis inverse calculations
- Work with matrix decomposition (B, N matrices)
- More computationally efficient for large problems

This approach allows the Primal Simplex to work independently while integrating seamlessly with the existing table caching system and being reusable for Branch & Bound algorithms.