# Linear Programming Solver - Algorithm Architecture Design

## Overview

This document describes the new modular algorithm architecture that replaced the monolithic 750+ line `Algorithms.cs` with a clean, maintainable system based on interfaces and dependency injection.

## Core Architecture Components

### 1. Interface System

The interface system provides a clean contract-based approach to algorithm implementation, ensuring consistency while allowing flexibility.

#### IAlgorithm - Core Algorithm Interface
```csharp
public interface IAlgorithm
{
    string Name { get; }                    // Algorithm display name
    string Description { get; }             // Brief description
    ProblemType[] SupportedTypes { get; }   // LP, NLP compatibility
    string[] RequiredTables { get; }        // Prerequisites (t-i, t-optimal)
    
    Table Execute(AlgorithmContext context); // Main execution method
}
```

**Purpose**: Defines the core computational contract that every algorithm must fulfill.

**Key Properties Explained**:
- **Name**: Human-readable algorithm name shown in menus and logs
- **Description**: Brief explanation used in UI displays and documentation
- **SupportedTypes**: Array defining whether algorithm works with LP, NLP, or both
  - `new[] { ProblemType.LinearProgramming }` - LP/IP algorithms only
  - `new[] { ProblemType.NonLinearProgramming }` - NLP algorithms only
  - `new[] { ProblemType.LinearProgramming, ProblemType.NonLinearProgramming }` - Universal
- **RequiredTables**: Dependencies that must exist before execution
  - `new[] { "t-optimal" }` - Needs optimal LP solution (Cutting Plane, Branch & Bound)
  - `new[] { "t-i" }` - Needs canonical form (some advanced algorithms)
  - `new string[0]` - No prerequisites (Primal Simplex)

**Execute Method**: 
- Receives complete `AlgorithmContext` containing all problem data
- Returns `Table` result or `null` if algorithm produces no tabular output (like NLP)
- Should focus purely on computation - no UI concerns

#### IAlgorithmUI - User Interface Responsibilities  
```csharp
public interface IAlgorithmUI
{
    void DisplayHeader();                   // Algorithm title/banner
    void ShowResults(Table result, AlgorithmContext context);  // Display results
    void ExportResults(Table result, AlgorithmContext context); // Write to files
}
```

**Purpose**: Separates presentation logic from computational logic, allowing algorithms to control their own display behavior.

**Method Responsibilities**:
- **DisplayHeader()**: Shows algorithm title, clears screen, displays banners
  - Called BEFORE algorithm execution
  - Should be visually distinctive and informative
  - Example: ASCII art borders, algorithm name, brief description

- **ShowResults()**: Displays algorithm output to console
  - Called AFTER successful algorithm execution
  - Receives both the result Table and full context
  - Should show key metrics, solution values, status information
  - Algorithm-specific formatting (B&B shows tree, Cutting Plane shows cuts applied)

- **ExportResults()**: Writes detailed results to files
  - Creates comprehensive output files with all intermediate steps
  - Should include raw data, formatted results, and algorithm-specific analysis
  - Uses `context.OutputPath` for file destination

#### IFullAlgorithm - Complete Algorithm Implementation
```csharp
public interface IFullAlgorithm : IAlgorithm, IAlgorithmUI
{
    // Combines core algorithm logic with UI responsibilities
    // No additional methods - just enforces implementation of both interfaces
}
```

**Purpose**: Marker interface that combines computational and presentation responsibilities into a single, self-contained unit.

**Why This Design**:
- **Single Responsibility**: Each algorithm manages its own complete lifecycle
- **Consistency**: All algorithms follow the same execution pattern
- **Extensibility**: Adding new algorithms requires only implementing one interface
- **Testability**: UI and logic can be tested separately if needed

#### Interface Inheritance Hierarchy
```
IFullAlgorithm (concrete implementation)
├── IAlgorithm (computational contract)
│   ├── Name, Description, SupportedTypes, RequiredTables
│   └── Execute(AlgorithmContext) → Table
└── IAlgorithmUI (presentation contract)
    ├── DisplayHeader() → void
    ├── ShowResults(Table, AlgorithmContext) → void
    └── ExportResults(Table, AlgorithmContext) → void
```

#### Implementation Example - CuttingPlaneAdapter
```csharp
public class CuttingPlaneAdapter : IFullAlgorithm
{
    // IAlgorithm implementation
    public string Name => "Cutting Plane Algorithm";
    public string Description => "Integer programming via cutting planes";
    public ProblemType[] SupportedTypes => new[] { ProblemType.LinearProgramming };
    public string[] RequiredTables => new[] { "t-optimal" }; // Needs LP relaxation
    
    public Table Execute(AlgorithmContext context)
    {
        // Pure computation - no console output
        var algorithm = new CuttingPlaneAlgorithm();
        var optimalTable = TableCache.GetTable("t-optimal");
        return algorithm.SolveIP(optimalTable);
    }
    
    // IAlgorithmUI implementation
    public void DisplayHeader()
    {
        Console.Clear();
        Console.WriteLine("╔════ CUTTING PLANE ALGORITHM ════╗");
        // Algorithm-specific header styling
    }
    
    public void ShowResults(Table result, AlgorithmContext context)
    {
        // Show cuts applied, integer feasibility status, objective improvement
        Console.WriteLine($"Status: {result.Status}");
        Console.WriteLine($"Cuts applied: {GetCutsCount()}");
        // Custom display logic for this algorithm
    }
    
    public void ExportResults(Table result, AlgorithmContext context)
    {
        // Write detailed cutting plane analysis to file
        using (var writer = new StreamWriter(context.OutputPath))
        {
            writer.WriteLine("=== CUTTING PLANE ANALYSIS ===");
            // Algorithm-specific export format
        }
    }
}
```

#### Interface Benefits

**For Algorithm Developers**:
- Clear contract to implement - no guesswork about required methods
- Freedom to customize display and export behavior
- Automatic integration with pipeline system

**For System Architecture**:
- Generic execution pattern works with any `IFullAlgorithm`
- Type safety at compile time
- Easy to add new algorithms without modifying existing code

**For Maintainability**:
- Each algorithm is self-contained and independently testable
- UI changes don't affect computational logic
- Clear separation of concerns

### 2. AlgorithmContext - Data Container

The `AlgorithmContext` serves as a comprehensive data container that passes all necessary information between the pipeline system and individual algorithms.

```csharp
public class AlgorithmContext
{
    public Table RawTable { get; set; }         // Original input data
    public Table CanonicalTable { get; set; }   // Canonical form (t-i)
    public Table OptimalTable { get; set; }     // Optimal solution (t-optimal)
    public NLPProblem NLPProblem { get; set; }  // NLP-specific data
    public ProblemType ProblemType { get; set; } // LP or NLP
    public string OutputPath { get; set; }      // Export destination
}
```

**Purpose**: Provides a unified way to pass problem data, intermediate results, and configuration between system components.

**Data Flow Through Context**:
```
User loads problem → RawTable populated
├── LP Problem: ProblemType = LinearProgramming
│   ├── Pipeline generates CanonicalTable (t-i) if needed
│   ├── Pipeline generates OptimalTable (t-optimal) if needed
│   └── Algorithm executes with full context
└── NLP Problem: ProblemType = NonLinearProgramming
    ├── NLPProblem populated with functions/constraints
    └── Algorithm executes with NLP-specific data
```

**Property Details**:
- **RawTable**: The original problem as loaded from file (max/min objective, constraints)
- **CanonicalTable**: Standard form conversion (all ≤ constraints, non-negative variables)
- **OptimalTable**: Solution from Primal Simplex or other LP solver
- **NLPProblem**: For non-linear problems - contains functions, derivatives, constraints
- **ProblemType**: Enum determining which algorithms are compatible
- **OutputPath**: File destination for detailed algorithm output (default: "data/output.txt")

**Context Lifecycle**:
1. **Initialization**: Created by `AlgorithmManager` constructor with basic problem info
2. **Pipeline Enhancement**: Missing tables automatically generated as needed
3. **Algorithm Execution**: Full context passed to algorithm's `Execute()` method
4. **Result Integration**: Algorithm results stored back into context for future use

#### How Interfaces Work Together in Practice

The interfaces create a powerful execution pattern that works consistently across all algorithms:

```csharp
// Generic execution pattern in AlgorithmManager
private Table ExecuteAlgorithm<T>() where T : IFullAlgorithm, new()
{
    // 1. Create algorithm instance
    var algorithm = new T();
    
    // 2. Validate compatibility (automatic via interface)
    if (!IsSupported(algorithm.SupportedTypes, _context.ProblemType))
        return null;
    
    // 3. Ensure prerequisites (automatic via interface)
    if (!_pipeline.EnsurePrerequisites(algorithm.RequiredTables))
        return null;
    
    // 4. Execute with consistent pattern
    algorithm.DisplayHeader();                    // IAlgorithmUI
    var result = algorithm.Execute(_context);     // IAlgorithm
    algorithm.ShowResults(result, _context);      // IAlgorithmUI
    algorithm.ExportResults(result, _context);    // IAlgorithmUI
    
    return result;
}
```

**Interface Interaction Flow**:
```
AlgorithmManager.ExecuteAlgorithm()
├── algorithm.SupportedTypes → Compatibility Check
├── algorithm.RequiredTables → Pipeline Prerequisites
├── algorithm.DisplayHeader() → User sees algorithm start
├── algorithm.Execute(context) → Core computation happens
├── algorithm.ShowResults() → User sees results
└── algorithm.ExportResults() → Detailed analysis saved
```

**Why This Pattern Works**:
- **Type Safety**: Compiler ensures all required methods are implemented
- **Consistency**: Every algorithm follows identical execution steps
- **Flexibility**: Each algorithm customizes behavior within standard framework
- **Maintainability**: Adding algorithms doesn't change execution logic
- **Separation**: Computation isolated from presentation concerns

### 3. AlgorithmPipeline.cs - Smart Dependency Manager

The pipeline automatically handles prerequisites and generates missing tables:

**Dependency Resolution Flow:**
```
User: "Run Cutting Plane"
Pipeline: "Need t-optimal for Cutting Plane"
Pipeline: "t-optimal missing, need t-i first" 
Pipeline: "t-i missing, have t-raw"
Pipeline: → Generates t-i from t-raw (canonical form)
Pipeline: → Generates t-optimal from t-i (primal simplex)  
Pipeline: "All dependencies satisfied"
Algorithm: "Ready to execute!"
```

**Key Methods:**
- `EnsurePrerequisites(string[] tables)` - Validates and generates missing tables
- `GenerateCanonicalForm()` - Creates t-i from raw data using CanonicalFormConverter
- `GenerateOptimalSolution()` - Runs PrimalSimplexAlgorithm to get t-optimal

### 4. Algorithm Adapters - Self-Contained Implementations

Each adapter implements `IFullAlgorithm` and handles its own complete lifecycle:

#### PrimalSimplexAdapter.cs
- **Execute()**: Runs `PrimalSimplexAlgorithm` 
- **ShowResults()**: Displays optimal solution, basic variables
- **ExportResults()**: Writes canonical form and all iterations to output.txt

#### BranchAndBoundAdapter.cs
- **Execute()**: Runs `BranchAndBoundAlgorithm`
- **ShowResults()**: Processing log, fathoming reasons, subproblem tree
- **ExportResults()**: Complete B&B analysis with all subproblem tables

#### CuttingPlaneAdapter.cs
- **Execute()**: Runs `CuttingPlaneAlgorithm`
- **ShowResults()**: Cuts applied, final integer status
- **ExportResults()**: Iteration history and cut analysis

#### NLPAdapter.cs
- **Execute()**: Runs `NLPAlgorithm` (returns null - no Table output)
- **ShowResults()**: Critical points, derivatives, Hessian analysis
- **ExportResults()**: NLP-specific mathematical results

## Architecture Benefits

### Old System (750+ lines of mixed concerns):
```
Algorithms.cs = MONOLITHIC EVERYTHING
├── Menu display
├── Input validation  
├── Algorithm execution
├── Results display
├── File export
├── Error handling
└── Duplicate code everywhere
```

### New System (Clean Separation of Concerns):
```
AlgorithmManager (200 lines) = Orchestrator
├── AlgorithmPipeline = Smart dependency resolution
├── PrimalSimplexAdapter = Self-contained Simplex
├── BranchAndBoundAdapter = Self-contained B&B  
├── CuttingPlaneAdapter = Self-contained Cutting Plane
└── NLPAdapter = Self-contained NLP
```

## Key Improvements

### 1. Automatic Dependency Resolution
```csharp
// OLD: Manual prerequisite checking in every algorithm
if (TableCache.GetTable("t-optimal") == null) {
    // Run primal simplex first...
    // Duplicate code in every algorithm
}

// NEW: Automatic pipeline resolution
var algorithm = new CuttingPlaneAdapter();
pipeline.EnsurePrerequisites(algorithm.RequiredTables); // Handles everything
```

### 2. Self-Contained Algorithms
- Each algorithm knows how to display its own results
- Each algorithm exports in its own specialized format
- No more duplicate validation or UI code
- Easy to add new algorithms

### 3. Generic Execution Pattern
```csharp
// Works for ANY algorithm implementing IFullAlgorithm
private Table ExecuteAlgorithm<T>() where T : IFullAlgorithm, new()
{
    var algorithm = new T();
    algorithm.DisplayHeader();
    var result = pipeline.ExecuteWithDependencies(algorithm);
    algorithm.ShowResults(result);
    algorithm.ExportResults(result);
    return result;
}
```

### 4. Extensibility
Adding a new algorithm requires only:
1. Create new adapter implementing `IFullAlgorithm`
2. Add to algorithm registry
3. Done - all pipeline, validation, and UI infrastructure works automatically

## Usage Examples

### Running an Algorithm
```csharp
var manager = new AlgorithmManager(problemType, rawTable);
var result = manager.HandleAlgorithmSelection(); // User picks algorithm
// Pipeline automatically resolves dependencies
// Algorithm executes with clean separation
// Results displayed and exported
```

#### Algorithm Registry System

The algorithm registry provides a flexible factory pattern that creates algorithm instances on demand while maintaining type safety and enabling easy extensibility.

```csharp
// Registry maps menu options to algorithm factory functions
_algorithmRegistry = new Dictionary<AlgorithmOption, Func<IFullAlgorithm>>
{
    { AlgorithmOption.PrimalSimplex, () => new PrimalSimplexAdapter() },
    { AlgorithmOption.BranchBoundSimplex, () => new BranchAndBoundAdapter() },
    { AlgorithmOption.CuttingPlane, () => new CuttingPlaneAdapter() },
    { AlgorithmOption.NonLinearProgramming, () => new NLPAdapter() }
};
```

**Registry Pattern Benefits**:

**1. Factory Creation**:
```csharp
// Clean instantiation with error handling
if (_algorithmRegistry.TryGetValue(option, out var algorithmFactory))
{
    var algorithm = algorithmFactory(); // Creates fresh instance
    // algorithm is guaranteed to implement IFullAlgorithm
}
```

**2. Type Safety**:
- `Func<IFullAlgorithm>` ensures all factories return interface-compliant objects
- Compile-time verification that all algorithms implement required methods
- No runtime casting or type checking needed

**3. Lazy Instantiation**:
- Algorithms only created when selected by user
- No memory overhead from unused algorithm instances
- Each execution gets a fresh, clean algorithm object

**4. Easy Extension**:
```csharp
// Adding a new algorithm is trivial
public enum AlgorithmOption 
{
    // ... existing options
    NewAlgorithm = 8  // Add new option
}

// Register in constructor
{ AlgorithmOption.NewAlgorithm, () => new NewAlgorithmAdapter() }

// That's it - automatically integrated with entire system!
```

**5. Decoupled Architecture**:
```csharp
// Generic execution works with ANY algorithm in registry
private Table ExecuteAlgorithm(AlgorithmOption option)
{
    var algorithmFactory = _algorithmRegistry[option];  // Get factory
    var algorithm = algorithmFactory();                 // Create instance
    
    // Rest of execution is identical for all algorithms
    return ExecuteWithPipeline(algorithm);
}
```

**Interface-Registry Integration**:

The combination of interfaces and registry creates a powerful plugin architecture:

```csharp
// This pattern works for ANY algorithm implementing IFullAlgorithm
public AlgorithmResult ExecuteWithPipeline(IFullAlgorithm algorithm)
{
    try
    {
        // 1. Interface properties provide metadata
        ValidateSupport(algorithm.SupportedTypes);
        
        // 2. Interface contract guarantees prerequisite information
        EnsurePrerequisites(algorithm.RequiredTables);
        
        // 3. Interface methods provide consistent execution
        algorithm.DisplayHeader();
        var result = algorithm.Execute(_context);
        algorithm.ShowResults(result, _context);
        algorithm.ExportResults(result, _context);
        
        return AlgorithmResult.CreateSuccess(result);
    }
    catch (Exception ex)
    {
        return AlgorithmResult.CreateFailure(ex.Message, ex);
    }
}
```

**Real-World Usage Flow**:
```
User selects "5. Cutting Plane Algorithm"
↓
AlgorithmManager.ExecuteAlgorithm(AlgorithmOption.CuttingPlane)
↓
Registry lookup: () => new CuttingPlaneAdapter()
↓
Factory creates: CuttingPlaneAdapter instance
↓
Interface contract guarantees:
├── .SupportedTypes = [LinearProgramming]
├── .RequiredTables = ["t-optimal"]
├── .Execute(context) implemented
├── .DisplayHeader() implemented
├── .ShowResults() implemented
└── .ExportResults() implemented
↓
Generic execution pattern works automatically
```

This design makes adding new algorithms incredibly simple while maintaining strict type safety and consistent behavior across the entire system.

## File Structure (Star Topology)
```
Algorithms/
├── Core/                          # Central hub - system orchestration
│   ├── AlgorithmManager.cs        # Main orchestrator (formerly Algorithms.cs)
│   ├── AlgorithmPipeline.cs       # Dependency resolution system
│   ├── IAlgorithm.cs              # Interface definitions and context
│   └── AlgorithmContext.cs        # Data container class
│
├── Adapters/                      # Interface compliance layer
│   ├── PrimalSimplexAdapter.cs    # Simplex interface wrapper
│   ├── BranchAndBoundAdapter.cs   # Branch & Bound interface wrapper  
│   ├── CuttingPlaneAdapter.cs     # Cutting Plane interface wrapper
│   ├── KnapsackAdapter.cs         # Knapsack interface wrapper
│   └── NLPAdapter.cs              # NLP interface wrapper
│
├── Implementations/               # Pure algorithm logic
│   ├── LP/                        # Linear Programming algorithms
│   │   ├── PrimalSimplexAlgorithm.cs
│   │   ├── DualSimplexAlgorithm.cs
│   │   └── RevisedSimplexAlgorithm.cs (stub - excluded from build)
│   ├── IP/                        # Integer Programming algorithms  
│   │   ├── BranchAndBoundAlgorithm.cs
│   │   ├── CuttingPlaneAlgorithm.cs
│   │   ├── KnapsackAlgorithm.cs
│   │   ├── BranchingVariableInfo.cs
│   │   └── BranchAndBoundKnapsackAlgorithm.cs (stub - excluded from build)
│   └── NLP/                       # Non-Linear Programming algorithms
│       └── NLPAlgorithm.cs
│
└── _old/                          # Legacy monolithic code (excluded from build)
    └── Algorithms.cs              # Original 750+ line monolithic file
```

**Star Topology Benefits:**
- **Core** serves as central hub - all communication flows through it
- **Clear separation** - interfaces, logic, and orchestration are distinct
- **Easy navigation** - developers know exactly where to find/add code
- **Scalable architecture** - new algorithms require minimal system changes

## Troubleshooting & Common Issues

### Build Errors
- **Namespace conflicts**: Ensure `_old/` directory is excluded from compilation in `.csproj`
- **Missing interfaces**: All algorithms must implement `IFullAlgorithm` interface
- **Table cache issues**: Use `new Table()` constructor to avoid reference sharing

### Algorithm Development
- **Prerequisites**: Always declare required tables in `RequiredTables` property
- **Problem types**: Specify supported problem types in `SupportedTypes` array
- **Results**: Return proper Table objects with correct status ("Optimal", "Optimal_Integer", etc.)

### Performance Considerations
- Pipeline automatically caches generated tables to avoid recomputation
- Adapters are lightweight - heavy computation stays in core algorithm classes
- Table operations are optimized for memory efficiency

## Technical Implementation Notes

### Dependency Injection Pattern
The system uses constructor-based dependency injection at the algorithm level, with runtime factory creation for algorithm instances. This provides flexibility while maintaining type safety.

### Error Handling Strategy
- Pipeline failures return `AlgorithmResult.CreateFailure()` with descriptive messages
- Individual algorithms handle their own validation and error states
- Context validation occurs before algorithm execution

### Memory Management
- TableCache automatically manages table lifecycle and prevents memory leaks
- Adapters don't hold references to large data structures after execution
- Context objects are reused across algorithm executions in the same session

This architecture transforms the solver from a monolithic system into a clean, modular, and highly maintainable codebase with automatic dependency resolution and self-contained algorithm implementations.


  Algorithms/
  ├── Core/                          # Central hub
  │   ├── AlgorithmManager.cs        # Main orchestrator
  │   ├── AlgorithmPipeline.cs       # Dependency resolver
  │   ├── IAlgorithm.cs              # Interface definitions
  │   └── AlgorithmContext.cs        # Data container
  │
  ├── Adapters/                      # Interface compliance layer
  │   ├── PrimalSimplexAdapter.cs
  │   ├── BranchAndBoundAdapter.cs
  │   ├── CuttingPlaneAdapter.cs
  │   └── NLPAdapter.cs
  │
  └── Implementations/               # Pure algorithm logic
      ├── LP/
      │   ├── PrimalSimplexAlgorithm.cs
      │   └── DualSimplexAlgorithm.cs
      ├── IP/
      │   ├── BranchAndBoundAlgorithm.cs
      │   └── CuttingPlaneAlgorithm.cs
      └── NLP/
          └── NLPAlgorithm.cs