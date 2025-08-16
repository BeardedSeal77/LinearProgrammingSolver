# Compilation Exclusions

**WARNING: The following files/folders are currently EXCLUDED from compilation in the .csproj file**

This was done to test the FileReader and Table functionality without fixing all the incomplete algorithm classes.

## Excluded Directories:
- `LPAlgorithms/` - All LP algorithm classes (PrimalSimplex, RevisedSimplex, etc.)
- `IPAlgorithms/` - All IP algorithm classes (BranchAndBound, CuttingPlane, etc.)  
- `Models/` - All model classes (LinearProgrammingModel, Variable, Constraint, etc.)

## Excluded Files:
- `Utils/FileWriter.cs` - File output functionality
- `Utils/PivotingOperations.cs` - Pivot utility methods
- `Tables/BranchAndBoundTable.cs` - Specialized B&B table class
- `Form1.cs` - Windows Forms UI
- `Form1.Designer.cs` - Windows Forms designer

## Reason for Exclusion:
These classes have incomplete method stubs that don't compile (missing return statements, etc.). They were excluded to allow testing of the core FileReader → Table → Display functionality.

## IMPORTANT:
**Before implementing algorithms, you MUST re-enable these files by removing the `<Compile Remove="..." />` entries from LinearProgrammingSolver.csproj and fixing the compilation errors.**

## Current Working Components:
- ✅ `Program.cs` - Main entry point
- ✅ `Utils/FileReader.cs` - Parses input files into Table objects
- ✅ `Tables/Table.cs` - Core table structure with display functionality

## Next Steps:
1. Fix compilation errors in excluded classes
2. Remove exclusions from .csproj
3. Implement canonical form algorithm
4. Implement simplex algorithms