# Linear Programming Solver - Complete Programming Guide

## Overview
This guide provides everything you need to implement each method in the codebase. Each section is self-contained with input/output specifications, algorithms, and examples.

---

## Project Structure and Flow

### **Main Flow:**
1. **t-i** (Initial Table) ← Created from input file
2. **LP Solving:** t-i → t-1 → t-2 → ... → t-optimal
3. **IP Solving:** t-optimal → t-1.1, t-1.2 → t-1.1.1, t-1.1.2, etc.

### **Table Naming Convention:**
- **t-i**: Initial canonical form
- **t-1, t-2, t-3**: Primal Simplex iterations
- **t-rev-1, t-rev-2**: Revised Simplex iterations  
- **t-1.1, t-1.2**: Branch & Bound sub-problems
- **t-cut-1, t-cut-2**: Cutting Plane iterations

---

## Tables/Table.cs - Core Table Object

### Table Properties:
```csharp
string TableId;              // "t-i", "t-1", "t-1.1", etc.
double[,] Matrix;            // Tableau matrix [rows, cols]
string TableType;            // "Initial", "Simplex", "BranchBound", "CuttingPlane"
string Status;               // "Continue", "Optimal", "Infeasible", "Unbounded"
List<string> BasicVariables; // ["x3", "x4", "x5"] - current basic vars
List<string> NonBasicVariables; // ["x1", "x2"] - current non-basic vars
double ObjectiveValue;       // Current objective function value
int PivotRow, PivotColumn;   // Last pivot location
double PivotElement;         // Last pivot element value
```

### Methods to Implement:

#### `Table(string tableId, string tableType)`
```csharp
// Initialize new table
TableId = tableId;
TableType = tableType;
Status = "Continue";
BasicVariables = new List<string>();
NonBasicVariables = new List<string>();
ObjectiveValue = 0.0;
PivotRow = -1; PivotColumn = -1; PivotElement = 0;
CreatedTime = DateTime.Now;
```

#### `void Display()`
```csharp
// Print table information
Console.WriteLine($"\n=== Table {TableId} ({TableType}) ===");
Console.WriteLine($"Status: {Status}, Objective: {ObjectiveValue:F3}");
if (PivotRow >= 0) Console.WriteLine($"Last Pivot: [{PivotRow},{PivotColumn}] = {PivotElement:F3}");
DisplayMatrix(); // Call private method below
```

#### `private void DisplayMatrix()`
```csharp
// Display tableau in grid format
if (Matrix == null) return;
int rows = Matrix.GetLength(0), cols = Matrix.GetLength(1);

// Print column headers
Console.Write("    ");
for (int j = 0; j < cols; j++) Console.Write($"{j,8}");
Console.WriteLine();

// Print rows with data
for (int i = 0; i < rows; i++) {
    Console.Write($"{i,3}:");
    for (int j = 0; j < cols; j++) {
        Console.Write($"{Matrix[i,j],8:F3}");
    }
    Console.WriteLine();
}
```

#### `Table Clone()`
```csharp
// Create deep copy
var clone = new Table(TableId + "-copy", TableType);
clone.Status = this.Status;
clone.ObjectiveValue = this.ObjectiveValue;
clone.Matrix = (double[,])this.Matrix?.Clone();
clone.BasicVariables = new List<string>(this.BasicVariables);
clone.NonBasicVariables = new List<string>(this.NonBasicVariables);
return clone;
```

#### `bool IsOptimal()`, `bool IsInfeasible()`, `bool IsUnbounded()`
```csharp
return Status == "Optimal";  // or "Infeasible" or "Unbounded"
```

---

## LPAlgorithms/PrimalSimplexAlgorithm.cs

### Key Algorithm: Primal Simplex Method
**Input:** Table with canonical form  
**Output:** Optimal table or infeasible/unbounded status

### Methods to Implement:

#### `Table CreateCanonicalForm(LinearProgrammingModel model)`
```csharp
// Convert LP model to canonical form tableau
// INPUT: model with variables, constraints, objective
// OUTPUT: Initial table (t-i) with slack variables added

int numVars = model.Variables.Count;
int numConstraints = model.Constraints.Count;
int numSlacks = 0;

// Count slack variables needed
foreach(var constraint in model.Constraints) {
    if(constraint.Type != ConstraintType.Equal) numSlacks++;
}

// Create matrix [constraints+1 rows, variables+slacks+1 cols]
int totalCols = numVars + numSlacks + 1; // +1 for RHS
double[,] matrix = new double[numConstraints + 1, totalCols];

// Fill constraint rows
int slackIndex = numVars;
for(int i = 0; i < numConstraints; i++) {
    var constraint = model.Constraints[i];
    
    // Original variable coefficients
    for(int j = 0; j < numVars; j++) {
        matrix[i, j] = constraint.Coefficients[j];
    }
    
    // Add slack variable if needed
    if(constraint.Type == ConstraintType.LessThanOrEqual) {
        matrix[i, slackIndex] = 1.0;
        slackIndex++;
    } else if(constraint.Type == ConstraintType.GreaterThanOrEqual) {
        matrix[i, slackIndex] = -1.0; // surplus
        slackIndex++;
    }
    
    // RHS
    matrix[i, totalCols - 1] = constraint.RightHandSide;
}

// Fill objective row (bottom row)
for(int j = 0; j < numVars; j++) {
    double coeff = model.Variables[j].Coefficient;
    // For maximization, negate coefficients
    if(model.OptimizationType == OptimizationType.Maximize) {
        coeff = -coeff;
    }
    matrix[numConstraints, j] = coeff;
}

// Create table
Table table = new Table("t-i", "Initial");
table.Matrix = matrix;
table.Status = "Continue";

// Set up basic variables (initially slack variables)
for(int i = numVars; i < numVars + numSlacks; i++) {
    table.BasicVariables.Add($"s{i - numVars + 1}");
}
// Set up non-basic variables (decision variables)
for(int i = 0; i < numVars; i++) {
    table.NonBasicVariables.Add($"x{i + 1}");
}

return table;
```

#### `Table SolveLP(Table initialTable)`
```csharp
// Solve LP using iterative simplex method
List<Table> iterations = new List<Table>();
Table currentTable = initialTable.Clone();
currentTable.TableId = "t-i";
iterations.Add(currentTable);

int iterationCount = 1;

while(!IsOptimal(currentTable) && iterationCount < 100) { // Max 100 iterations
    if(IsUnbounded(currentTable)) {
        currentTable.Status = "Unbounded";
        break;
    }
    
    // Perform one iteration
    Table nextTable = PerformIteration(currentTable);
    nextTable.TableId = $"t-{iterationCount}";
    
    iterations.Add(nextTable);
    currentTable = nextTable;
    iterationCount++;
}

if(IsOptimal(currentTable)) {
    currentTable.Status = "Optimal";
    currentTable.TableId = "t-optimal";
}

return currentTable;
```

#### `int SelectEnteringVariable(Table table)`
```csharp
// Choose entering variable (most negative coefficient in objective row)
// RULE: Select column with most negative value in bottom row

double[,] matrix = table.Matrix;
int numRows = matrix.GetLength(0);
int numCols = matrix.GetLength(1);
int objRow = numRows - 1; // Bottom row is objective

int enteringCol = -1;
double mostNegative = 0;

// Search objective row for most negative coefficient
for(int j = 0; j < numCols - 1; j++) { // Skip RHS column
    if(matrix[objRow, j] < mostNegative) {
        mostNegative = matrix[objRow, j];
        enteringCol = j;
    }
}

return enteringCol; // Returns -1 if optimal (no negative coefficients)
```

#### `int SelectLeavingVariable(Table table, int enteringColumn)`
```csharp
// Choose leaving variable using minimum ratio test
// RULE: min(RHS[i] / column[i]) where column[i] > 0

double[,] matrix = table.Matrix;
int numRows = matrix.GetLength(0);
int numCols = matrix.GetLength(1);
int rhsCol = numCols - 1;

int leavingRow = -1;
double minRatio = double.MaxValue;

// Apply minimum ratio test
for(int i = 0; i < numRows - 1; i++) { // Skip objective row
    double columnValue = matrix[i, enteringColumn];
    double rhsValue = matrix[i, rhsCol];
    
    if(columnValue > 1e-6) { // Only consider positive coefficients
        double ratio = rhsValue / columnValue;
        if(ratio < minRatio) {
            minRatio = ratio;
            leavingRow = i;
        }
    }
}

return leavingRow;
```

#### `Table PerformIteration(Table currentTable)`
```csharp
// Perform one complete simplex iteration
// STEPS: 1) Select entering 2) Select leaving 3) Pivot 4) Update

// Step 1: Select entering variable
int enteringCol = SelectEnteringVariable(currentTable);
if(enteringCol == -1) {
    currentTable.Status = "Optimal";
    return currentTable;
}

// Step 2: Select leaving variable  
int leavingRow = SelectLeavingVariable(currentTable, enteringCol);
if(leavingRow == -1) {
    currentTable.Status = "Unbounded";
    return currentTable;
}

// Step 3: Perform pivoting
Table newTable = PivotingOperations.PerformPivoting(currentTable, leavingRow, enteringCol);
newTable.PivotRow = leavingRow;
newTable.PivotColumn = enteringCol;
newTable.PivotElement = currentTable.Matrix[leavingRow, enteringCol];

// Step 4: Update basic/non-basic variables
string enteringVar = currentTable.NonBasicVariables[enteringCol];
string leavingVar = currentTable.BasicVariables[leavingRow];

newTable.NonBasicVariables[enteringCol] = leavingVar;
newTable.BasicVariables[leavingRow] = enteringVar;

// Step 5: Update objective value
newTable.ObjectiveValue = CalculateObjectiveValue(newTable);

return newTable;
```

#### `bool IsOptimal(Table table)`
```csharp
// Check if all coefficients in objective row are non-negative
double[,] matrix = table.Matrix;
int objRow = matrix.GetLength(0) - 1;
int numCols = matrix.GetLength(1);

for(int j = 0; j < numCols - 1; j++) { // Skip RHS
    if(matrix[objRow, j] < -1e-6) { // Negative coefficient found
        return false;
    }
}
return true;
```

#### `bool IsUnbounded(Table table, int enteringColumn)`
```csharp
// Check if all coefficients in entering column are non-positive
double[,] matrix = table.Matrix;
int numRows = matrix.GetLength(0);

for(int i = 0; i < numRows - 1; i++) { // Skip objective row
    if(matrix[i, enteringColumn] > 1e-6) { // Positive coefficient found
        return false;
    }
}
return true; // All coefficients are non-positive = unbounded
```

---

## Utils/PivotingOperations.cs - Pivoting Methods

### `static Table PerformPivoting(Table inputTable, int pivotRow, int pivotColumn)`
```csharp
// Perform complete pivoting operation
// ALGORITHM: 1) Make pivot element = 1, 2) Make other elements in column = 0

Table newTable = inputTable.Clone();
double[,] matrix = newTable.Matrix;
int numRows = matrix.GetLength(0);
int numCols = matrix.GetLength(1);

double pivotElement = matrix[pivotRow, pivotColumn];
if(Math.Abs(pivotElement) < 1e-10) {
    throw new Exception("Invalid pivot: element is zero");
}

// Step 1: Divide pivot row by pivot element (make pivot = 1)
for(int j = 0; j < numCols; j++) {
    matrix[pivotRow, j] /= pivotElement;
}

// Step 2: Eliminate other elements in pivot column (make them = 0)
for(int i = 0; i < numRows; i++) {
    if(i != pivotRow) {
        double factor = matrix[i, pivotColumn];
        for(int j = 0; j < numCols; j++) {
            matrix[i, j] -= factor * matrix[pivotRow, j];
        }
    }
}

newTable.Matrix = matrix;
return newTable;
```

---

## IPAlgorithms/BranchAndBoundAlgorithm.cs

### Core Algorithm: Branch and Bound for Integer Programming
**Flow:** t-optimal → t-1.1, t-1.2 → t-1.1.1, t-1.1.2, etc.

### Methods to Implement:

#### `BranchAndBoundTable SolveIP(Table lpOptimalTable)`
```csharp
// Solve IP using Branch and Bound starting from LP optimal
// INPUT: LP optimal solution (may have fractional integer variables)
// OUTPUT: Best integer solution found

_allNodes = new List<BranchAndBoundTable>();
_bestIntegerSolution = null;

// Create root node from LP optimal
var rootNode = new BranchAndBoundTable("t-1", 1);
rootNode.Matrix = lpOptimalTable.Matrix;
rootNode.ObjectiveValue = lpOptimalTable.ObjectiveValue;
rootNode.Status = lpOptimalTable.Status;

// Check if already integer solution
if(IsIntegerSolution(rootNode)) {
    rootNode.Status = "Optimal";
    return rootNode;
}

// Start branching process
Queue<BranchAndBoundTable> nodesToProcess = new Queue<BranchAndBoundTable>();
nodesToProcess.Enqueue(rootNode);

while(nodesToProcess.Count > 0) {
    var currentNode = nodesToProcess.Dequeue();
    _allNodes.Add(currentNode);
    
    // Check if should fathom
    if(ShouldFathom(currentNode)) {
        FathomNode(currentNode, "Bounded or Infeasible");
        continue;
    }
    
    // Branch on fractional variable
    int branchVar = SelectBranchingVariable(currentNode);
    var children = BranchOnVariable(currentNode, branchVar);
    
    foreach(var child in children) {
        var solvedChild = SolveSubProblem(child);
        
        if(IsIntegerSolution(solvedChild)) {
            UpdateBestSolution(solvedChild);
        } else if(!ShouldFathom(solvedChild)) {
            nodesToProcess.Enqueue(solvedChild);
        }
    }
}

return _bestIntegerSolution;
```

#### `List<BranchAndBoundTable> BranchOnVariable(BranchAndBoundTable parentTable, int variableIndex)`
```csharp
// Create two child nodes by branching on variable
// RULE: x_i ≤ floor(value) and x_i ≥ ceil(value)

double fractionalValue = GetVariableValue(parentTable, variableIndex);
double floorValue = Math.Floor(fractionalValue);
double ceilValue = Math.Ceiling(fractionalValue);

// Create first child: x_i ≤ floor(value)
var child1 = new BranchAndBoundTable($"{parentTable.TableId}.1", parentTable.SubProblemNumber * 10 + 1);
child1.Matrix = (double[,])parentTable.Matrix.Clone();
child1.BranchingVariable = variableIndex;
child1.BranchingValue = floorValue;
child1.BranchingConstraint = $"x{variableIndex + 1} <= {floorValue}";
child1.ParentTable = parentTable;

// Add constraint: x_i ≤ floorValue (add new row to matrix)
AddConstraintToTable(child1, variableIndex, floorValue, "<=");

// Create second child: x_i ≥ ceil(value)  
var child2 = new BranchAndBoundTable($"{parentTable.TableId}.2", parentTable.SubProblemNumber * 10 + 2);
child2.Matrix = (double[,])parentTable.Matrix.Clone();
child2.BranchingVariable = variableIndex;
child2.BranchingValue = ceilValue;
child2.BranchingConstraint = $"x{variableIndex + 1} >= {ceilValue}";
child2.ParentTable = parentTable;

// Add constraint: x_i ≥ ceilValue
AddConstraintToTable(child2, variableIndex, ceilValue, ">=");

parentTable.AddChild(child1);
parentTable.AddChild(child2);

return new List<BranchAndBoundTable> { child1, child2 };
```

#### `int SelectBranchingVariable(BranchAndBoundTable table)`
```csharp
// Find first integer variable with fractional value
// STRATEGY: Choose variable with largest fractional part

int bestVar = -1;
double maxFractionalPart = 0;

for(int i = 0; i < GetNumberOfDecisionVariables(table); i++) {
    if(IsIntegerVariable(i)) {
        double value = GetVariableValue(table, i);
        double fractionalPart = value - Math.Floor(value);
        
        if(fractionalPart > 1e-6 && fractionalPart < 1 - 1e-6) { // Truly fractional
            if(fractionalPart > maxFractionalPart) {
                maxFractionalPart = fractionalPart;
                bestVar = i;
            }
        }
    }
}

return bestVar;
```

#### `bool IsIntegerSolution(BranchAndBoundTable table)`
```csharp
// Check if all integer variables have integer values
for(int i = 0; i < GetNumberOfDecisionVariables(table); i++) {
    if(IsIntegerVariable(i)) {
        double value = GetVariableValue(table, i);
        if(Math.Abs(value - Math.Round(value)) > 1e-6) {
            return false; // Found fractional integer variable
        }
    }
}
return true; // All integer variables have integer values
```

---

## Input File Format (for Utils/FileReader.cs)

### Expected Format:
```
max +2 +3 +3 +5 +2 +4
+11 +8 +6 +14 +10 +10 <=40
bin bin bin bin bin bin
```

### FileReader.ReadModelFromFile() Implementation:
```csharp
// Read and parse input file
string[] lines = File.ReadAllLines(filePath);
if(lines.Length < 3) throw new Exception("Invalid file format");

// Parse first line: objective function
string[] objTokens = lines[0].Split(' ', StringSplitOptions.RemoveEmptyEntries);
var optimizationType = objTokens[0].ToLower() == "max" ? OptimizationType.Maximize : OptimizationType.Minimize;

var variables = new List<Variable>();
for(int i = 1; i < objTokens.Length; i += 2) {
    string sign = objTokens[i];     // "+" or "-"
    double coeff = double.Parse(objTokens[i + 1]);
    if(sign == "-") coeff = -coeff;
    
    variables.Add(new Variable(i / 2, $"x{i / 2 + 1}", coeff));
}

// Parse constraint lines  
var constraints = new List<Constraint>();
for(int lineIdx = 1; lineIdx < lines.Length - 1; lineIdx++) {
    string[] tokens = lines[lineIdx].Split(' ', StringSplitOptions.RemoveEmptyEntries);
    
    var coefficients = new List<double>();
    int tokenIdx = 0;
    
    // Parse coefficients
    while(tokenIdx < tokens.Length - 2) { // Last 2 tokens are operator and RHS
        string sign = tokens[tokenIdx++];
        double coeff = double.Parse(tokens[tokenIdx++]);
        if(sign == "-") coeff = -coeff;
        coefficients.Add(coeff);
    }
    
    // Parse constraint type and RHS
    string operatorStr = tokens[tokens.Length - 2];
    double rhs = double.Parse(tokens[tokens.Length - 1]);
    
    ConstraintType type = operatorStr switch {
        "<=" => ConstraintType.LessThanOrEqual,
        ">=" => ConstraintType.GreaterThanOrEqual,
        "=" => ConstraintType.Equal,
        _ => throw new Exception($"Invalid operator: {operatorStr}")
    };
    
    constraints.Add(new Constraint(lineIdx - 1, $"C{lineIdx}", coefficients, type, rhs));
}

// Parse sign restrictions (last line)
string[] signRestrictions = lines[lines.Length - 1].Split(' ', StringSplitOptions.RemoveEmptyEntries);
for(int i = 0; i < signRestrictions.Length && i < variables.Count; i++) {
    string restriction = signRestrictions[i].ToLower();
    variables[i].Type = restriction switch {
        "bin" => VariableType.Binary,
        "int" => VariableType.Integer,
        _ => VariableType.Continuous
    };
}

// Create model
var model = new LinearProgrammingModel("Loaded Model");
model.OptimizationType = optimizationType;
model.Variables = variables;
model.Constraints = constraints;

return model;
```

---

## Testing Your Implementation

### Quick Test Flow:
1. **Load Model:** Create t-i from input file
2. **Solve LP:** t-i → t-1 → t-2 → t-optimal  
3. **Check LP:** Verify t-optimal has correct objective value
4. **Solve IP:** t-optimal → t-1.1, t-1.2 → best integer solution
5. **Display:** Show all tables created

### Debug Tips:
- **Print each table** after creation to verify matrix correctness
- **Check pivot elements** are non-zero before pivoting
- **Verify constraints** are added correctly in branch and bound
- **Test optimality conditions** at each iteration

This guide provides everything needed to implement each method independently. Each method has clear inputs, outputs, and algorithmic steps!