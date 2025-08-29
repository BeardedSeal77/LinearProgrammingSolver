# Branch & Bound Implementation Plan

## 🎯 **Current Status**
✅ **COMPLETED:**
- DualSimplex algorithm implemented and working
- Subproblem generation with correct row manipulation:
  - Subproblem A (≤): Uses `(basic_row - new_row) * -1` → negative RHS
  - Subproblem B (≥): Uses `basic_row - new_row` → negative RHS
- Matrix expansion working correctly (preserves RHS column position)
- TableCache integration functional

## 📋 **Project Requirements (from Project_Outline.md)**
Branch & Bound Simplex Algorithm must:
1. ✅ Display Canonical Form
2. ✅ Implement backtracking 
3. ✅ Create all possible sub-problems to branch on
4. ✅ Fathom all possible nodes of sub-problems
5. ✅ Display all table iterations of sub-problems
6. ✅ Display best candidate

**Output Requirements:**
- Text file output with ALL results
- All tableau iterations of selected algorithm  
- 3 decimal place rounding

## 🏗️ **Implementation Strategy**

### **1. Branch & Bound Main Algorithm Structure**
```csharp
public Table SolveIP(Table lpOptimalTable)
{
    // Initialize LIFO stack and tracking
    Stack<Table> pendingNodes = new Stack<Table>();
    Table bestIntegerSolution = null;
    List<string> processingLog = new List<string>();
    
    // Start with initial subproblems
    var initialSubproblems = BranchOnVariable(lpOptimalTable, SelectBranchingVariable(lpOptimalTable));
    foreach (var sub in initialSubproblems.Reverse()) // LIFO order
        pendingNodes.Push(sub);
    
    // LIFO processing
    while (pendingNodes.Count > 0)
    {
        Table currentNode = pendingNodes.Pop();
        processingLog.Add($"Processing: {currentNode.TableId}");
        
        // Apply DualSimplex to restore feasibility
        Table solvedNode = _dualSimplex.SolveLP(currentNode);
        
        // Check fathoming conditions
        if (ShouldFathom(solvedNode, bestIntegerSolution, out string reason))
        {
            FathomNode(solvedNode, reason);
            processingLog.Add($"Fathomed: {solvedNode.TableId} - {reason}");
            continue;
        }
        
        // Check if integer solution
        if (IsIntegerSolution(solvedNode))
        {
            UpdateBestSolution(solvedNode, ref bestIntegerSolution);
            processingLog.Add($"Integer solution: {solvedNode.TableId} - Obj: {solvedNode.GetObjectiveValue()}");
            continue;
        }
        
        // Branch further (fractional solution)
        processingLog.Add($"Branching from: {solvedNode.TableId}");
        var newSubproblems = BranchOnVariable(solvedNode, SelectBranchingVariable(solvedNode));
        foreach (var subproblem in newSubproblems.Reverse()) // LIFO order
        {
            pendingNodes.Push(subproblem);
        }
    }
    
    return bestIntegerSolution;
}
```

### **2. Naming Convention System**
```
t-optimal (LP relaxation)
├── t-optimal-A (initial subproblem A: x1 ≤ 3)
│   ├── t-optimal-A1 (after 1st DualSimplex iteration)  
│   ├── t-optimal-A2 (after 2nd DualSimplex iteration)
│   └── t-optimal-A3 (after 3rd DualSimplex iteration - FINAL)
│       └── If A3 is fractional, branch again:
│           ├── t-optimal-A3-A (x2 ≤ 2)
│           │   ├── t-optimal-A3-A1, A3-A2... (DualSimplex on this branch)
│           └── t-optimal-A3-B (x2 ≥ 3)
│               ├── t-optimal-A3-B1, A3-B2... (DualSimplex on this branch)
└── t-optimal-B (initial subproblem B: x1 ≥ 4)
    ├── t-optimal-B1 (after 1st DualSimplex iteration)
    └── t-optimal-B2 (after 2nd DualSimplex iteration - FINAL)
        └── If B2 is fractional, branch again:
            ├── t-optimal-B2-A, t-optimal-B2-B...
```

### **3. Enhanced Fathoming Rules**
```csharp
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
```

### **4. LIFO Stack Processing Order**
```
Initial: Stack = [t-optimal-B, t-optimal-A]  (A on top for LIFO)
1. Pop: t-optimal-A → DualSimplex → t-optimal-A1, A2, A3
   Evaluate A3: Fractional → Branch → Push [t-optimal-A3-B, t-optimal-A3-A]
2. Pop: t-optimal-A3-A → DualSimplex → t-optimal-A3-A1, A3-A2  
   Evaluate A3-A2: Integer → Update best, continue
3. Pop: t-optimal-A3-B → DualSimplex → t-optimal-A3-B1
   Evaluate A3-B1: Infeasible → Fathom, continue  
4. Pop: t-optimal-B → DualSimplex → t-optimal-B1, B2
   Evaluate B2: Fractional but worse bound → Fathom, continue
5. Stack empty → Return best solution: t-optimal-A3-A2
```

### **5. Complete Output Generation**
All tables stored in TableCache with full iteration history:
- **Canonical Form**: t-i (already stored)
- **LP Optimal**: t-optimal  
- **All Subproblems**: t-optimal-A, t-optimal-A1, t-optimal-A2, etc.
- **Processing Log**: Order of evaluation and fathoming reasons
- **Best Solution**: Final integer solution with full path

### **6. Data Structures for Tracking**
```csharp
public class BranchAndBoundAlgorithm 
{
    private DualSimplexAlgorithm _dualSimplex;
    private List<Table> _allSubproblems;        // Complete history
    private Table _bestIntegerSolution;         // Best found
    private Dictionary<string, string> _fathomReasons;  // Why each node was fathomed
    private List<string> _processingOrder;      // Educational output
    private Stack<Table> _pendingNodes;         // LIFO processing queue
}
```

### **7. Expected Test Results**
For current problem (max 8x1 + 5x2, x1+x2≤6, 9x1+5x2≤45, x1,x2 binary):
- **LP Optimal**: x1=3.75, x2=2.25, obj=41.25
- **Branch on x1**: Create x1≤3, x1≥4 subproblems  
- **Expected Processing Order**: Depends on LIFO and feasibility
- **Expected Integer Solution**: One of {(3,3), (4,2), (3,2), (4,1), ...}

### **8. Implementation Checklist**
✅ DualSimplex working  
✅ Subproblem generation working  
✅ Row manipulation correct  
🔄 Complete SolveIP with LIFO stack  
❌ Enhanced fathoming logic  
❌ Processing order tracking  
❌ Complete output generation  
❌ Text file export  

### **9. Next Coding Steps**
1. **Implement complete SolveIP()** with LIFO stack processing
2. **Add comprehensive tracking** (processing log, fathom reasons)
3. **Test with current example** and verify tree traversal
4. **Generate complete text output** for Project requirements  
5. **Verify against manual B&B calculations**