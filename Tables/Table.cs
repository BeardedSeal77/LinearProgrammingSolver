namespace LinearProgrammingSolver.Tables
{
    // Simplified Table class that stores tableau data in raw format.
    // Responsible ONLY for data storage - no processing logic.
    // All tables stored with same structure for consistency and simplicity.
    public class Table
    {
        // =================================================================
        // CORE DATA STORAGE (Raw tableau data only)
        // =================================================================
        
        // Unique identifier for this table (e.g., "t-raw", "t-canonical", "t-1", "t-optimal")
        public string TableId { get; set; }
        
        // The complete tableau matrix including objective row and RHS column.
        // Structure: [rows x columns] where last column is RHS
        // Row 0 = Objective function, Rows 1+ = Constraints
        public double[,] Matrix { get; set; }
        
        // Labels for each row in the matrix.
        // Example: ["OBJ", "C1", "C2", "C3"] for objective + 3 constraints
        public List<string> RowLabels { get; set; }
        
        // Labels for each column in the matrix.
        // Example: ["x1", "x2", "s1", "e1", "RHS"] for 2 decision vars + slack + surplus + RHS
        public List<string> ColumnLabels { get; set; }
        
        // Current basic variables (variables with coefficient 1 in identity columns).
        // Example: ["s1", "s2", "e1"] for canonical form basis
        public List<string> BasicVariables { get; set; }
        
        // =================================================================
        // METADATA (Problem definition and state)
        // =================================================================
        
        // Whether this is a maximization or minimization problem
        public OptimizationType OptimizationType { get; set; }
        
        // Constraint operators for each constraint row (C1: <=, C2: >=, etc.)
        // Key: Constraint label (C1, C2, C3...), Value: Operator (<=, >=, =)
        public Dictionary<string, ConstraintOperator> ConstraintOperators { get; set; }
        
        // Current status of this table in the solution process
        // Examples: "Raw", "Canonical", "Iteration", "Optimal", "Infeasible"
        public string Status { get; set; }
        
        // When this table was created (for tracking solution progress)
        public DateTime CreatedTime { get; set; }
        
        // =================================================================
        // CONSTRUCTORS (Different ways to create table objects)
        // =================================================================
        
        // Constructor 1: Create table from raw parsed data (used by FileReader)
        // tableId: Unique identifier for this table
        // matrix: Complete tableau matrix with RHS as last column
        // rowLabels: Labels for each row (OBJ, C1, C2, etc.)
        // columnLabels: Labels for each column (x1, x2, s1, RHS, etc.)
        // optimizationType: Maximize or Minimize
        // status: Current status of table
        // constraintOperators: Optional constraint operators dictionary
        public Table(string tableId, double[,] matrix, List<string> rowLabels, 
                     List<string> columnLabels, OptimizationType optimizationType, 
                     string status = "Unknown", Dictionary<string, ConstraintOperator> constraintOperators = null)
        {
            TableId = tableId;
            Matrix = matrix;
            RowLabels = new List<string>(rowLabels);
            ColumnLabels = new List<string>(columnLabels);
            BasicVariables = new List<string>();
            OptimizationType = optimizationType;
            ConstraintOperators = constraintOperators ?? new Dictionary<string, ConstraintOperator>();
            Status = status;
            CreatedTime = DateTime.Now;
        }
        
        // Constructor 2: Create table from existing table (used for iterations/transformations)
        // Creates a deep copy of source table with new ID
        // newTableId: New unique identifier
        // sourceTable: Table to copy from
        // newStatus: Optional new status for the copied table
        public Table(string newTableId, Table sourceTable, string newStatus = null)
        {
            if (sourceTable == null)
                throw new ArgumentNullException(nameof(sourceTable));
                
            TableId = newTableId;
            Status = newStatus ?? sourceTable.Status;
            OptimizationType = sourceTable.OptimizationType;
            CreatedTime = DateTime.Now;
            
            // Deep copy the matrix
            int rows = sourceTable.Matrix.GetLength(0);
            int cols = sourceTable.Matrix.GetLength(1);
            Matrix = new double[rows, cols];
            for (int i = 0; i < rows; i++)
                for (int j = 0; j < cols; j++)
                    Matrix[i, j] = sourceTable.Matrix[i, j];
            
            // Deep copy the lists
            RowLabels = new List<string>(sourceTable.RowLabels);
            ColumnLabels = new List<string>(sourceTable.ColumnLabels);
            BasicVariables = new List<string>(sourceTable.BasicVariables);
            ConstraintOperators = new Dictionary<string, ConstraintOperator>(sourceTable.ConstraintOperators);
        }
        
        // =================================================================
        // DATA ACCESS METHODS (Simple get/set operations)
        // =================================================================
        
        // Get matrix element by row/column index
        public double GetElement(int row, int col)
        {
            if (Matrix == null || row < 0 || row >= Matrix.GetLength(0) || 
                col < 0 || col >= Matrix.GetLength(1))
                return 0.0;
            return Matrix[row, col];
        }
        
        // Set matrix element by row/column index
        public void SetElement(int row, int col, double value)
        {
            if (Matrix != null && row >= 0 && row < Matrix.GetLength(0) && 
                col >= 0 && col < Matrix.GetLength(1))
                Matrix[row, col] = value;
        }
        
        // Get the objective function value (RHS of objective row)
        public double GetObjectiveValue()
        {
            if (Matrix == null || Matrix.GetLength(0) == 0 || Matrix.GetLength(1) == 0)
                return 0.0;
            return Matrix[0, Matrix.GetLength(1) - 1]; // Last column of first row
        }
        
        // Get number of rows in the tableau
        public int GetRowCount() => Matrix?.GetLength(0) ?? 0;
        
        // Get number of columns in the tableau
        public int GetColumnCount() => Matrix?.GetLength(1) ?? 0;
        
        // Get number of variables (excluding RHS column)
        public int GetVariableCount() => GetColumnCount() - 1;
        
        // =================================================================
        // DISPLAY METHODS (Two formats: Traditional tableau vs Matrix decomposition)
        // =================================================================
        
        // Display table in traditional simplex tableau format
        // Shows complete matrix with row and column labels
        public void DisplayTraditional()
        {
            Console.WriteLine($"\n=== Table: {TableId} ===");
            Console.WriteLine($"Status: {Status}");
            Console.WriteLine($"Optimization: {OptimizationType}");
            
            if (BasicVariables.Count > 0)
            {
                Console.WriteLine($"Basic Variables: [{string.Join(", ", BasicVariables)}]");
            }
            
            if (Matrix == null || ColumnLabels == null || RowLabels == null)
            {
                Console.WriteLine("Matrix not initialized");
                return;
            }
            
            Console.WriteLine("\nTableau Matrix:");
            
            // Print column headers (include space for operator column)
            Console.Write("      ");
            foreach (var colLabel in ColumnLabels)
            {
                if (colLabel == "RHS")
                {
                    Console.Write($"{"Op",4}{"RHS",8}"); // Add operator column before RHS
                }
                else
                {
                    Console.Write($"{colLabel,8}");
                }
            }
            Console.WriteLine();
            
            // Print separator line
            Console.Write("      ");
            for (int j = 0; j < ColumnLabels.Count; j++)
            {
                if (ColumnLabels[j] == "RHS")
                {
                    Console.Write("------------"); // Extra space for operator column
                }
                else
                {
                    Console.Write("--------");
                }
            }
            Console.WriteLine();
            
            // Print each row with row label
            for (int i = 0; i < Matrix.GetLength(0); i++)
            {
                string rowLabel = i < RowLabels.Count ? RowLabels[i] : $"R{i}";
                Console.Write($"{rowLabel,5} |");
                
                for (int j = 0; j < Matrix.GetLength(1); j++)
                {
                    if (j == Matrix.GetLength(1) - 1) // RHS column
                    {
                        // Add operator column before RHS
                        string operatorSymbol = GetConstraintOperatorSymbol(rowLabel);
                        Console.Write($"{operatorSymbol,4}");
                        Console.Write($"{Matrix[i, j],8:F3}");
                    }
                    else
                    {
                        Console.Write($"{Matrix[i, j],8:F3}");
                    }
                }
                Console.WriteLine();
            }
            
            Console.WriteLine($"Objective Value: {GetObjectiveValue():F3}");
        }
        
        // Display table in mathematical matrix decomposition format
        // Shows separate matrices for basic variables, non-basic variables, basis, costs, and RHS
        public void DisplayMatrix()
        {
            Console.WriteLine($"\n=== Math Matrix Format (Source: {TableId}) ===");
            
            if (Matrix == null || ColumnLabels == null || RowLabels == null)
            {
                Console.WriteLine("Matrix not initialized");
                return;
            }
            
            // Identify basic and non-basic variables from current basis
            var basicIndices = new List<int>();
            var nonBasicIndices = new List<int>();
            
            // Separate variables based on current basis
            for (int j = 0; j < ColumnLabels.Count - 1; j++) // Exclude RHS
            {
                if (BasicVariables.Contains(ColumnLabels[j]))
                {
                    basicIndices.Add(j);
                }
                else if (ColumnLabels[j] != "RHS") // Don't include RHS in variables
                {
                    nonBasicIndices.Add(j);
                }
            }
            
            int constraintCount = Matrix.GetLength(0) - 1; // Exclude objective row
            
            // Display Basic Variables Matrix (Xbv)
            Console.WriteLine("\nBasic Variables (Xbv):");
            if (basicIndices.Count > 0)
            {
                for (int j = 0; j < basicIndices.Count; j++)
                {
                    Console.Write($"{ColumnLabels[basicIndices[j]],6} |");
                    for (int i = 1; i <= constraintCount; i++)
                    {
                        Console.Write($"{Matrix[i, basicIndices[j]],8:F3}");
                    }
                    Console.WriteLine();
                }
            }
            else
            {
                Console.WriteLine("  (Empty matrix)");
            }
            
            // Display Non-Basic Variables Matrix (Xnb)
            Console.WriteLine("\nNon-Basic Variables (Xnb):");
            if (nonBasicIndices.Count > 0)
            {
                for (int j = 0; j < nonBasicIndices.Count; j++)
                {
                    Console.Write($"{ColumnLabels[nonBasicIndices[j]],6} |");
                    for (int i = 1; i <= constraintCount; i++)
                    {
                        Console.Write($"{Matrix[i, nonBasicIndices[j]],8:F3}");
                    }
                    Console.WriteLine();
                }
            }
            else
            {
                Console.WriteLine("  (Empty matrix)");
            }
            
            // Display RHS Vector
            Console.WriteLine("\nRight-Hand Side (RHS):");
            int rhsCol = Matrix.GetLength(1) - 1;
            for (int i = 1; i <= constraintCount; i++)
            {
                string constraintLabel = i - 1 < RowLabels.Count - 1 ? RowLabels[i] : $"C{i}";
                Console.WriteLine($"  {constraintLabel}: {Matrix[i, rhsCol]:F3}");
            }
            
            // Display Costs
            Console.WriteLine("\nCosts:");
            Console.WriteLine("Basic Costs (Cb):");
            foreach (int idx in basicIndices)
            {
                Console.WriteLine($"  {ColumnLabels[idx]}: {Matrix[0, idx]:F3}");
            }
            
            Console.WriteLine("Non-Basic Costs (Cnb):");
            foreach (int idx in nonBasicIndices)
            {
                Console.WriteLine($"  {ColumnLabels[idx]}: {Matrix[0, idx]:F3}");
            }
        }
        
        // =================================================================
        // UTILITY METHODS (Helper operations)
        // =================================================================
        
        // Create a deep copy of this table with a new ID
        public Table Clone(string newTableId)
        {
            return new Table(newTableId, this);
        }
        
        // Get the constraint operator symbol for a given row.
        // rowLabel: Row label (e.g., "OBJ", "C1", "C2")
        // Returns: Operator symbol ("=", "<=", ">=") or empty for objective row
        private string GetConstraintOperatorSymbol(string rowLabel)
        {
            if (rowLabel == "OBJ")
                return "="; // Objective is always equality
                
            if (ConstraintOperators.ContainsKey(rowLabel))
            {
                return ConstraintOperators[rowLabel] switch
                {
                    ConstraintOperator.LessThanOrEqual => "<=",
                    ConstraintOperator.GreaterThanOrEqual => ">=", 
                    ConstraintOperator.Equal => "=",
                    _ => "="
                };
            }
            
            return "="; // Default to equality if not found
        }

        // =================================================================
        // STATUS DETECTION METHODS (Simplex algorithm status checks)
        // =================================================================
        
        public bool IsOptimal()
        {
            // Check if current table is optimal
            // All coefficients in objective row should be non-negative for maximization
            // All coefficients in objective row should be non-positive for minimization
            
            int objRow = 0; // Objective is always first row
            int rhsCol = GetColumnCount() - 1; // Exclude RHS column
            
            for (int j = 0; j < rhsCol; j++)
            {
                double coefficient = GetElement(objRow, j);
                
                if (OptimizationType == OptimizationType.Maximize)
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
        
        public bool IsUnbounded(int enteringColumn)
        {
            // Check if problem is unbounded for the given entering variable
            // Problem is unbounded if all coefficients in entering column are <= 0
            // (excluding objective row)
            
            for (int i = 1; i < GetRowCount(); i++) // Skip objective row
            {
                if (GetElement(i, enteringColumn) > 0.001)
                {
                    return false; // Found positive coefficient, not unbounded
                }
            }
            
            return true; // All coefficients <= 0, unbounded
        }
        
        public bool IsInfeasible()
        {
            // Check if current table represents an infeasible solution
            // Two main checks:
            // 1. Negative RHS values (basic infeasibility)
            // 2. Artificial variables are positive in optimal solution
            
            int rhsCol = GetColumnCount() - 1;
            
            // Check for negative RHS values
            for (int i = 1; i < GetRowCount(); i++)
            {
                if (GetElement(i, rhsCol) < -0.001)
                    return true;
            }
            
            // Check for positive artificial variables in basis (post-optimal check)
            if (BasicVariables != null)
            {
                for (int i = 0; i < BasicVariables.Count; i++)
                {
                    string bv = BasicVariables[i];
                    if (bv.StartsWith("a") && GetElement(i + 1, rhsCol) > 0.001)
                    {
                        return true;
                    }
                }
            }
            
            return false;
        }
        
        public bool IsFeasible()
        {
            // Opposite of IsInfeasible()
            return !IsInfeasible();
        }
        
        // Simple string representation of the table
        public override string ToString()
        {
            return $"Table {TableId} ({Status}) - {GetRowCount()}x{GetColumnCount()}";
        }
    }
    
    // =================================================================
    // ENUMS (Supporting data types)
    // =================================================================
    
    public enum OptimizationType
    {
        Maximize,
        Minimize
    }
    
    public enum VariableType
    {
        Decision,    // x1, x2, etc. (original decision variables)
        Slack,       // s1, s2, etc. (slack variables for <=)
        Surplus,     // e1, e2, etc. (surplus variables for >=)
        Artificial,  // a1, a2, etc. (artificial variables)
        RHS          // Right-hand side column
    }
    
    public enum VariableConstraint
    {
        NonNegative,    // x >= 0 (default)
        NonPositive,    // x <= 0
        Unrestricted,   // no bounds (urs)
        Integer,        // integer values only
        Binary          // 0 or 1 only
    }
    
    public enum ConstraintOperator
    {
        LessThanOrEqual,     // <=
        GreaterThanOrEqual,  // >=
        Equal                // =
    }
}