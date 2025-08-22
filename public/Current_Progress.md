# Linear Programming Solver - Current Progress

## Overview
This document tracks the current implementation progress and data flow of our Linear Programming Solver. The system follows a **Star/Tree Topology** with Program.cs as the central orchestrator and clean separation of concerns.

---

## Current Input Data (`data/input.txt`)
```
min +100 +30
+0 +1 <= 3
+1 +1 >= 7
+10 +4 >= 40
bin bin
```

**Problem Definition:**
- **Objective**: Minimize 100x₁ + 30x₂
- **Constraints**: 
  - C1: 0x₁ + 1x₂ ≤ 3
  - C2: 1x₁ + 1x₂ ≥ 7
  - C3: 10x₁ + 4x₂ ≥ 40
- **Variables**: x₁, x₂ (both binary)

---

## Current System Architecture

### 1. **Core Classes Implemented**
- ✅ **Table.cs** - Pure data storage with simplified constructors
- ✅ **TableCache.cs** - Static cache for storing solution history tables
- ✅ **FileReader.cs** - Parses input files into raw data components (no table construction)
- ✅ **CanonicalFormConverter.cs** - Converts raw tables to canonical form

### 2. **Classes To Be Implemented**
- 🔲 **PrimalSimplexAlgorithm.cs** - Solves LP using primal simplex method
- 🔲 **RevisedSimplexAlgorithm.cs** - Solves LP using revised simplex method
- 🔲 **BranchAndBoundAlgorithm.cs** - Solves IP using branch & bound
- 🔲 **CuttingPlaneAlgorithm.cs** - Solves IP using cutting plane method

---

## Star/Tree Topology Architecture

### **System Topology Diagram**
```
                    Program.cs
                 (Central Orchestrator)
                    /       |       \
                   /        |        \
           FileReader    Table    TableCache
          (Parser Only) (Storage) (Cache/Stack)
              |            |           |
        Input: file    Input: raw   Input: Table
        Output: tuple  Output: obj  Output: void
```

---

## Data Flow Walkthrough (Star Topology)

### **Step 1: Program.cs Initialization**
**Program.cs does the following setup:**
```csharp
// Program.cs:18-22
TableCache.ClearAllTables();              // Clears any previous solution history
var fileReader = new FileReader();        // Creates file parsing utility
var canonicalConverter = new CanonicalFormConverter();  // Creates conversion utility
```

**What happens:** Program.cs prepares a clean environment and creates the utility objects it will coordinate.

---

### **Step 2: Program.cs → FileReader**
**Program.cs feeds the file path into FileReader, FileReader returns raw data components:**

```csharp
// Program.cs:39 - Program.cs calls FileReader
var (matrix, rowLabels, columnLabels, optimizationType) = fileReader.ParseFile(inputPath);
```

**FileReader receives:** `string inputPath` (e.g., `"data/input.txt"`)

**FileReader processes the input file:**
1. **Parses Line 1**: `"min +100 +30"` 
   → Extracts `OptimizationType.Minimize` and `coefficients [100, 30]`
2. **Parses Lines 2-4**: Constraint equations with operators
   → Builds `constraintData` with coefficients, RHS values, and operators  
3. **Parses Line 5**: `"bin bin"` 
   → Extracts `variableConstraints [Binary, Binary]`
4. **Builds raw matrix** (4x3):
   ```
   OBJ | 100.00  30.00   0.00
   C1  |   0.00   1.00   3.00
   C2  |   1.00   1.00   7.00
   C3  |  10.00   4.00  40.00
   ```
5. **Creates labels**:
   - `rowLabels = ["OBJ", "C1", "C2", "C3"]`
   - `columnLabels = ["x1", "x2", "RHS"]`

**FileReader returns to Program.cs:** A tuple containing `(matrix, rowLabels, columnLabels, optimizationType)`

**Key Point**: FileReader does **NO table construction** - only parsing!

---

### **Step 3: Program.cs → Table Constructor**
**Program.cs takes the return from FileReader and feeds it into the Table constructor:**

```csharp
// Program.cs:42 - Program.cs constructs Table object
var rawTable = new Table("t-raw", matrix, rowLabels, columnLabels, optimizationType, "Raw");
```

**Table Constructor receives:**
- `tableId: "t-raw"`
- `matrix: double[4,3]` (from FileReader)
- `rowLabels: ["OBJ", "C1", "C2", "C3"]` (from FileReader)
- `columnLabels: ["x1", "x2", "RHS"]` (from FileReader)
- `optimizationType: Minimize` (from FileReader)
- `status: "Raw"`

**Table Constructor processes:**
- Makes deep copies of all input data
- Initializes `BasicVariables = []` (empty for raw table)
- Sets `CreatedTime = DateTime.Now`

**Table Constructor returns to Program.cs:** Complete `Table` object ready for storage

---

### **Step 4: Program.cs → TableCache**
**Program.cs takes the Table object and stores it in TableCache:**

```csharp
// Program.cs:45 - Program.cs stores table
TableCache.StoreTable(rawTable);
```

**TableCache receives:** Complete `Table rawTable` object from Program.cs

**TableCache processes:** Stores table in static dictionary as `_tableCache["t-raw"] = rawTable`

**TableCache returns to Program.cs:** `bool true` (success confirmation)

**Result:** Table is now cached and retrievable via `TableCache.GetTable("t-raw")`

---

### **Step 5: Program.cs → Table Display Methods**
**Program.cs calls the Table's display methods to show output:**

```csharp
// Program.cs:48-52 - Program.cs requests display
rawTable.DisplayTraditional();  // Traditional tableau format
rawTable.DisplayMatrix();       // Mathematical matrix format
```

**Table Display Methods receive:** Self-reference to the Table object

**Table Display Methods process:**
- **DisplayTraditional()**: Formats as simplex tableau with row/column headers
- **DisplayMatrix()**: Decomposes into Xbv, Xnb, RHS, costs matrices

**Table Display Methods output:** Formatted console output showing the table data

---

### **Step 6: Program.cs → CanonicalFormConverter**
**Program.cs feeds the raw table into CanonicalFormConverter, CanonicalFormConverter returns canonical table:**

```csharp
// Program.cs:56 - Program.cs requests conversion
var canonicalTable = canonicalConverter.ConvertToCanonicalForm(rawTable);
```

**CanonicalFormConverter receives:** `Table rawTable` (4x3 matrix) from Program.cs

**CanonicalFormConverter processes:**
1. **Analyzes Structure**:
   - Original variables: 2 (x₁, x₂)
   - Constraints: 3 (C1, C2, C3)  
   - Additional variables needed: 3 (s₁, s₂, s₃)

2. **Expands Matrix** to 4x6:
   ```
   OBJ | 100.00  30.00   0.00   0.00   0.00   0.00
   C1  |   0.00   1.00   1.00   0.00   0.00   3.00
   C2  |   1.00   1.00   0.00   1.00   0.00   7.00
   C3  |  10.00   4.00   0.00   0.00   1.00  40.00
   ```

3. **Updates Labels**:
   - `columnLabels = ["x1", "x2", "s1", "s2", "s3", "RHS"]`
   - `basicVariables = ["s1", "s2", "s3"]`

**CanonicalFormConverter returns to Program.cs:** New `Table` object ("t-i", Canonical status)

---

### **Step 7: Program.cs → Final Storage & Summary**
**Program.cs stores the canonical table and requests a summary:**

```csharp
// Program.cs:57 & 67 - Program.cs stores and summarizes
TableCache.StoreTable(canonicalTable);  // Store canonical table
TableCache.DisplayTableSummary();       // Show solution history
```

**Program.cs feeds canonical table into TableCache:** Complete canonical `Table` object

**TableCache stores and confirms:** Canonical table stored successfully

**Program.cs requests summary from TableCache:** Request to display all stored tables

**TableCache displays:** Summary showing both tables now cached

**Final Result - TableCache contains:**
```
_tableCache = {
    ["t-raw"] = Table(4x3, Raw, t-raw),
    ["t-i"] = Table(4x6, Canonical, t-i)
}
```

---

## Input/Output Summary by Component

| Component | Input | Processing | Output |
|-----------|-------|------------|---------|
| **Program.cs** | Command line | Orchestrates all operations | Console display |
| **FileReader** | File path string | Parse file → build matrix/labels | Raw data tuple |
| **Table** | Raw data components | Store data + metadata | Table object |
| **TableCache** | Table objects | Store in dictionary | Cached tables |
| **CanonicalConverter** | Raw table object | Add slack variables | Canonical table |

---

## Current Table Storage State

After running the current implementation, **TableCache** contains:

| TableID | Status    | Size | Description |
|---------|-----------|------|-------------|
| t-raw   | Raw       | 4x3  | Original input data |
| t-i     | Canonical | 4x6  | Canonical form with slack variables |

---

## Object Relationships (Star Topology)

```
Program.cs (Central Coordinator)
├── Creates: FileReader, CanonicalFormConverter  
├── Constructs: All Table objects
├── Calls: TableCache.StoreTable() for each table
└── Retrieves: Tables for display

FileReader (Parser Only)
├── Input: File path string
├── Output: Raw data tuple (matrix, labels, optType)
└── Internal: Parses file → creates matrix → returns components

CanonicalFormConverter  
├── Input: Table object (raw)
├── Output: Table object ("t-i") 
└── Internal: Expands matrix → adds slack vars → creates Table

TableCache (Static Cache)
├── Stores: Dictionary<string, Table> (solution history stack)
├── Methods: StoreTable(), GetTable(), DisplayTableSummary()
└── Purpose: Cache/stack for all tables during solution process

Table
├── Storage: Matrix, Labels, BasicVariables, Metadata
├── Constructors: (raw data) + (copy from existing table)
└── Display: DisplayTraditional() + DisplayMatrix()
```

---

## Key Topology Benefits

✅ **No Circular Dependencies**: FileReader → Program.cs ← Table ← TableCache  
✅ **Single Responsibility**: Each component has one clear job  
✅ **Central Control**: Program.cs orchestrates all object creation  
✅ **Clean Interfaces**: Clear inputs/outputs between components  
✅ **Easy Testing**: Each component can be tested in isolation

---

## Next Implementation Steps

1. **Simplex Algorithms**: Create t-1, t-2, ..., t-optimal tables
2. **Branch & Bound**: Create t-1.1, t-1.2, t-1.1.1, etc. tables  
3. **File Export**: Save all tables to output files via FileWriter
4. **Interactive Menu**: Allow user to choose algorithms and view specific tables

---

## Key Design Principles Achieved

✅ **Clean Separation**: Data storage vs. business logic vs. presentation  
✅ **Star Topology**: Parse → Construct → Store → Call  
✅ **Flexible Storage**: All table types use same structure  
✅ **Clear Flow**: Each class has single responsibility  
✅ **Easy Testing**: Each component can be tested independently  
✅ **Solution History**: TableCache acts as growing stack of all tables

The foundation is architecturally sound and ready for algorithm implementation!