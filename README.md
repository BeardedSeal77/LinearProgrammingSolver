# Linear Programming Solver

A console-based Linear Programming and Integer Programming solver built in C# for academic purposes.

## Overview

This solver implements multiple algorithms for solving Linear Programming (LP) and Integer Programming (IP) problems:

### Linear Programming Algorithms
- **Primal Simplex Algorithm** - Standard tableau-based simplex method
- **Revised Simplex Algorithm** - Matrix-based simplex with basis inverse operations

### Integer Programming Algorithms  
- **Branch and Bound Simplex** - General integer programming solver
- **Branch and Bound Knapsack** - Specialized knapsack problem solver
- **Cutting Plane Algorithm** - Gomory cuts for integer solutions

## Features

- **Menu-driven console interface**
- **Input file parsing** for LP/IP models
- **Complete iteration tracking** with table naming (t-i, t-1, t-1.1, etc.)
- **Output file generation** with all solution steps
- **Canonical form display** for all algorithms
- **Sensitivity analysis support** (planned)

## Input File Format

```
max +2 +3 +3 +5 +2 +4
+11 +8 +6 +14 +10 +10 <=40
bin bin bin bin bin bin
```

## Quick Start

1. Build the project in Visual Studio
2. Run `solve.exe`
3. Choose "Load Model from File"
4. Select LP or IP algorithm
5. View results and export to file

## Project Structure

```
├── Tables/              # Table objects (t-i, t-1, etc.)
├── LPAlgorithms/        # Simplex algorithm implementations
├── IPAlgorithms/        # Integer programming algorithms
├── Utils/               # File I/O and pivoting operations
├── Models/              # Basic data models
└── public/              # Documentation
```

## Documentation

- **`Architecture_Documentation.md`** - System overview and design
- **`Programming_Guide.md`** - Complete implementation guide with code examples

## Requirements

- .NET Framework / .NET Core
- Visual Studio (recommended)

## Academic Use Only

This project is developed for university coursework and is not intended for commercial or public use. See LICENSE for details.

## Authors

University Student Project - LPR 381