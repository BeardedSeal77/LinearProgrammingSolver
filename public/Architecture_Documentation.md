# Linear Programming Solver - Architecture Overview

## Quick Reference Architecture

### **File Structure:**
```
LinearProgrammingSolver/
├── Tables/
│   ├── Table.cs                    # Core table object
│   └── BranchAndBoundTable.cs      # B&B specialized table
├── LPAlgorithms/
│   ├── PrimalSimplexAlgorithm.cs   # LP solving methods
│   └── RevisedSimplexAlgorithm.cs  # Revised simplex methods
├── IPAlgorithms/
│   ├── BranchAndBoundAlgorithm.cs       # IP solving methods
│   ├── BranchAndBoundKnapsackAlgorithm.cs
│   └── CuttingPlaneAlgorithm.cs
├── Utils/
│   ├── PivotingOperations.cs       # Pivoting utility methods
│   ├── FileReader.cs              # Input parsing
│   └── FileWriter.cs              # Output generation
├── Models/                        # Simple data models
└── Program.cs                     # Main application
```

### **Table Naming Convention:**
- **t-i**: Initial canonical form table
- **t-1, t-2, t-3**: Primal Simplex iterations  
- **t-rev-1, t-rev-2**: Revised Simplex iterations
- **t-1.1, t-1.2**: Branch & Bound sub-problems
- **t-1.1.1, t-1.1.2**: Further B&B branching
- **t-cut-1, t-cut-2**: Cutting Plane iterations

### **Main Program Flow:**
```
1. Load Model → Create t-i (canonical form)
2. Solve LP → t-i → t-1 → t-2 → t-optimal
3. Solve IP → t-optimal → t-1.1, t-1.2 → t-1.1.1, t-1.1.2, etc.
4. Display all tables and export results
```

### **Key Algorithm Classes:**

#### **PrimalSimplexAlgorithm.cs**
- `CreateCanonicalForm()` - Convert model to initial table
- `SolveLP()` - Iterate until optimal
- `SelectEnteringVariable()` - Choose pivot column  
- `SelectLeavingVariable()` - Choose pivot row
- `PerformIteration()` - One simplex step

#### **BranchAndBoundAlgorithm.cs**  
- `SolveIP()` - Main B&B algorithm
- `BranchOnVariable()` - Create t-x.1, t-x.2 children
- `SelectBranchingVariable()` - Choose fractional variable
- `IsIntegerSolution()` - Check if all integer vars are integers

#### **PivotingOperations.cs**
- `PerformPivoting()` - Execute pivot operation
- `DivideRowByPivot()` - Make pivot element = 1
- `EliminateColumn()` - Make other column elements = 0

### **Each Table Object Contains:**
```csharp
string TableId;              // "t-1", "t-1.1", etc.
double[,] Matrix;            // Tableau matrix
string Status;               // "Continue", "Optimal", etc.
List<string> BasicVariables; // Current basic variables
int PivotRow, PivotColumn;   // Last pivot information
```

### **Branch & Bound Tree Example:**
```
t-i (Initial) → t-optimal (LP solution)
                      ↓
                  t-1 (Root IP)
                 ↙         ↘
           t-1.1 (x1≤2)    t-1.2 (x1≥3)  
           ↙    ↘          ↙      ↘
    t-1.1.1   t-1.1.2  t-1.2.1  t-1.2.2
```

## For Complete Implementation Details
**See `Programming_Guide.md`** - Contains detailed method implementations, algorithms, and code examples for every class and method.