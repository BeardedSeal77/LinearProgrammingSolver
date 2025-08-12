# Linear Programming Solver Project

## Project Overview

Create a program that solves Linear Programming and Integer Programming Models and analyzes how changes in LP parameters affect the optimal solution.

**Technical Requirements:**
- Visual Studio project using any .NET programming language
- Build executable: `solve.exe`
- Menu-driven interface
- Input: Text file with mathematical model
- Output: Text file with all results

## Minimum Requirements

- ✅ Accept random amount of decision variables
- ✅ Accept random amount of constraints
- ✅ Use comments in programming
- ✅ Implement programming best practices

## Input File Format

### First Line (Objective Function)
Space-separated format:
- `max` or `min` (optimization type)
- Operator (`+` or `-`) for each decision variable coefficient
- Coefficient value for each decision variable

### Constraint Lines
For each constraint:
- Operators (`+` or `-`) for technological coefficients (same order as objective function)
- Technological coefficients (same order as objective function)
- Constraint relation (`=`, `<=`, or `>=`)
- Right-hand-side value

### Sign Restrictions Line
Space-separated sign restrictions: `+`, `-`, `urs`, `int`, `bin` (same order as objective function)

### Example Input File
```
max +2 +3 +3 +5 +2 +4
+11 +8 +6 +14 +10 +10 <=40
bin bin bin bin bin bin
```

**Note:** Enter the original LP/IP model, not canonical forms or relaxed models.

## Processing Requirements

### Algorithm Selection Menu
Program must provide options to select solving algorithm.

### Post-Solution Analysis
Program must provide sensitivity analysis options after solving.

## Programming Model Support

- ✅ Solve normal max Linear Programming Models (specifically the given Knapsack IP)
- ✅ Solve binary Integer Programming Models (specifically the given Knapsack IP)

## Required Algorithms

### 1. Simplex Methods
- **Primal Simplex Algorithm**
  - Display Canonical Form
  - Display all tableau iterations
  
- **Revised Primal Simplex Algorithm**
  - Display Canonical Form
  - Display all Product Form and Price Out iterations

### 2. Branch and Bound Methods
- **Branch & Bound Simplex Algorithm** (or Revised version)
  - Display Canonical Form
  - Implement backtracking
  - Create all possible sub-problems to branch on
  - Fathom all possible nodes of sub-problems
  - Display all table iterations of sub-problems
  - Display best candidate

- **Branch & Bound Knapsack Algorithm**
  - Implement backtracking
  - Create all possible sub-problems to branch on
  - Fathom all possible nodes of sub-problems
  - Display all table iterations of sub-problems
  - Display best candidate

### 3. Cutting Plane Algorithm
- **Cutting Plane Algorithm** (or Revised version)
  - Display Canonical Form
  - Display all Product Form and Price Out iterations

## Output File Format

- Contains Canonical form
- Contains all tableau iterations of selected algorithm
- All decimal values rounded to 3 decimal places

## Sensitivity Analysis Features

### Variable Analysis
- Display range of selected Non-Basic Variable
- Apply and display change of selected Non-Basic Variable
- Display range of selected Basic Variable
- Apply and display change of selected Basic Variable
- Display range of selected variable in Non-Basic Variable column
- Apply and display change of selected variable in Non-Basic Variable column

### Constraint Analysis
- Display range of selected constraint right-hand-side value
- Apply and display change of selected constraint right-hand-side value

### Solution Modifications
- Add new activity to optimal solution
- Add new constraint to optimal solution
- Display shadow prices

### Duality Analysis
- Apply Duality to the programming model
- Solve the Dual Programming Model
- Verify whether model has Strong or Weak Duality

## Special Case Handling

- ✅ Identify and resolve infeasible programming models
- ✅ Identify and resolve unbounded programming models

## Bonus Features

**Non-linear Problem Solving** (+10 marks)
- Ability to solve non-linear problems like f(x)=x²
- Must explain code implementation for this feature

## Implementation Checklist

### Core Components
- [ ] File I/O system (input/output text files)
- [ ] Mathematical model parser
- [ ] Menu-driven user interface (`solve.exe`)
- [ ] Algorithm selection system

### Algorithm Implementations
- [ ] Primal Simplex Algorithm
- [ ] Revised Primal Simplex Algorithm
- [ ] Branch & Bound Simplex Algorithm (or Revised version)
- [ ] Branch & Bound Knapsack Algorithm
- [ ] Cutting Plane Algorithm (or Revised version)

### Analysis Features
- [ ] Sensitivity analysis module (12 different operations)
- [ ] Duality analysis (apply, solve, verify Strong/Weak duality)
- [ ] Shadow price calculations
- [ ] Range analysis for variables and constraints

### Error Handling
- [ ] Infeasible model detection and resolution
- [ ] Unbounded model detection and resolution
- [ ] Input validation and error messages
- [ ] Interface presentation error handling

### Output Features
- [ ] Canonical form display for all algorithms
- [ ] Tableau iteration display (all iterations)
- [ ] Product Form and Price Out iterations (for Revised algorithms)
- [ ] 3-decimal place rounding
- [ ] Comprehensive result reporting

### Technical Requirements
- [ ] Visual Studio .NET project structure
- [ ] Executable builds to `solve.exe`
- [ ] Support for variable numbers of decision variables
- [ ] Support for variable numbers of constraints
- [ ] Code comments throughout
- [ ] Programming best practices implementation

### Testing Requirements
- [ ] Test with provided knapsack example
- [ ] Test all algorithm combinations
- [ ] Test all sensitivity analysis operations
- [ ] Test special cases (infeasible/unbounded)
- [ ] Test input file parsing with various formats