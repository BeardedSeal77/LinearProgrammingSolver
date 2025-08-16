namespace LinearProgrammingSolver.Tables
{
    public class Table
    {
        public string TableId { get; set; }
        
        // Tableau structure
        public double[,] Matrix { get; set; }  // Full tableau including RHS as last column
        public List<string> RowLabels { get; set; }  // ["OBJ", "C1", "C2", ...]
        public List<string> ColumnLabels { get; set; }  // ["x1", "x2", "s1", "s2", "RHS"]
        public List<VariableType> ColumnTypes { get; set; }  // [Decision, Decision, Slack, Surplus, RHS]
        
        // Current basis information
        public List<string> BasicVariables { get; set; }  // Variables currently in basis
        
        // Problem definition
        public OptimizationType OptimizationType { get; set; }  // Maximize or Minimize
        public Dictionary<string, VariableConstraint> VariableConstraints { get; set; }  // {x1=Binary, x2=Integer, etc.}
        public Dictionary<string, ConstraintOperator> ConstraintOperators { get; set; }  // {C1=LessThanOrEqual, C2=GreaterThanOrEqual, etc.}
        
        // Table metadata
        public string TableType { get; set; }  // "Raw", "Initial", "Iteration", "Optimal"
        public string Status { get; set; }  // "Continue", "Optimal", "Infeasible", "Unbounded"
        public string Description { get; set; }
        public DateTime CreatedTime { get; set; }

        // Pivoting information
        public int PivotRow { get; set; }
        public int PivotColumn { get; set; }
        public double PivotElement { get; set; }
        
        // Helper properties
        public double ObjectiveValue => Matrix[0, Matrix.GetLength(1) - 1];  // RHS of objective row
        public int Rows => Matrix.GetLength(0);
        public int Columns => Matrix.GetLength(1);
        public int VariableCount => ColumnLabels.Count - 1;  // Exclude RHS column

        public Table(string tableId, string tableType = "Unknown")
        {
            // Initialize table with ID and type
            TableId = tableId;
            TableType = tableType;
            
            // Initialize collections
            RowLabels = new List<string>();
            ColumnLabels = new List<string>();
            ColumnTypes = new List<VariableType>();
            BasicVariables = new List<string>();
            VariableConstraints = new Dictionary<string, VariableConstraint>();
            ConstraintOperators = new Dictionary<string, ConstraintOperator>();
            
            // Set defaults
            Status = "Unknown";
            Description = "";
            CreatedTime = DateTime.Now;
            PivotRow = -1;
            PivotColumn = -1;
            PivotElement = 0;
        }

        public void Display()
        {
            // Display table header information
            Console.WriteLine($"\n=== Table: {TableId} ({TableType}) ===");
            Console.WriteLine($"Status: {Status}");
            Console.WriteLine($"Optimization: {OptimizationType}");
            Console.WriteLine($"Description: {Description}");
            
            // Display variable constraints
            if (VariableConstraints.Count > 0)
            {
                var varConstraintPairs = VariableConstraints.Select(kvp => $"{kvp.Key}={kvp.Value}");
                Console.WriteLine($"Variable Constraints: [{string.Join(", ", varConstraintPairs)}]");
            }
            
            // Display constraint operators
            if (ConstraintOperators.Count > 0)
            {
                var constraintOpPairs = ConstraintOperators.Select(kvp => $"{kvp.Key}={kvp.Value}");
                Console.WriteLine($"Constraint Operators: [{string.Join(", ", constraintOpPairs)}]");
            }
            
            // Display the matrix
            DisplayMatrix();
            
            // Display basic variables if any
            if (BasicVariables.Count > 0)
            {
                Console.WriteLine($"Basic Variables: [{string.Join(", ", BasicVariables)}]");
            }
            
            // Display objective value
            if (Matrix != null && Matrix.GetLength(0) > 0)
            {
                Console.WriteLine($"Objective Value: {ObjectiveValue}");
            }
        }

        private void DisplayMatrix()
        {
            if (Matrix == null || ColumnLabels == null || RowLabels == null)
            {
                Console.WriteLine("Matrix not initialized");
                return;
            }
            
            Console.WriteLine("\nTableau Matrix:");
            
            // Print column headers
            Console.Write("      "); // Space for row labels
            foreach (var colLabel in ColumnLabels)
            {
                Console.Write($"{colLabel,8}");
            }
            Console.WriteLine();
            
            // Print separator line
            Console.Write("      ");
            for (int j = 0; j < ColumnLabels.Count; j++)
            {
                Console.Write("--------");
            }
            Console.WriteLine();
            
            // Print each row with row label
            for (int i = 0; i < Matrix.GetLength(0); i++)
            {
                // Print row label
                string rowLabel = i < RowLabels.Count ? RowLabels[i] : $"R{i}";
                Console.Write($"{rowLabel,5} |");
                
                // Print row values
                for (int j = 0; j < Matrix.GetLength(1); j++)
                {
                    Console.Write($"{Matrix[i, j],8:F2}");
                }
                Console.WriteLine();
            }
        }

        /// <summary>
        /// Creates a deep copy of this table for algorithm iterations.
        /// Used when running simplex algorithms to create new tables (t-1, t-2, etc.)
        /// or for branch & bound to create branch tables (t-1.1, t-1.2, etc.)
        /// without modifying the original table.
        /// </summary>
        public Table Clone()
        {
            // TODO: Implement deep copy of all properties including Matrix, lists, etc.
            return null; // TODO: Implement cloning
        }

        public bool IsOptimal()
        {
            // Check if this table represents an optimal solution
            return false; // TODO: Implement optimality check
        }

        public bool IsInfeasible()
        {
            // Check if this table represents an infeasible solution
            return false; // TODO: Implement infeasibility check
        }

        public bool IsUnbounded()
        {
            // Check if this table represents an unbounded solution
            return false; // TODO: Implement unbounded check
        }

        public override string ToString()
        {
            // Return concise string representation
            return $"Table {TableId} ({TableType})"; // TODO: Enhance string representation
        }

        /// <summary>
        /// Converts the tableau to math preliminary format with decomposed matrices.
        /// </summary>
        public MathPrelimFormat GetMathPrelimFormat()
        {
            // Create math prelim format object
            var mathFormat = new MathPrelimFormat
            {
                SourceTableId = TableId
            };

            // Decompose the tableau into separate components
            DecomposeTableau(mathFormat);

            return mathFormat;
        }

        /// <summary>
        /// Displays the table in math preliminary format.
        /// </summary>
        public void DisplayMathPrelim()
        {
            var mathFormat = GetMathPrelimFormat();
            mathFormat.Display();
        }

        /// <summary>
        /// Decomposes the tableau into math preliminary format components.
        /// </summary>
        private void DecomposeTableau(MathPrelimFormat mathFormat)
        {
            if (Matrix == null || ColumnLabels == null || RowLabels == null)
                return;

            int totalRows = Matrix.GetLength(0);
            int totalCols = Matrix.GetLength(1);
            
            // Separate basic and non-basic variables
            var basicVarIndices = new List<int>();
            var nonBasicVarIndices = new List<int>();
            
            // For raw table, assume no basic variables are set yet
            // All decision variables are non-basic initially
            for (int j = 0; j < totalCols - 1; j++) // Exclude RHS column
            {
                if (ColumnTypes[j] == VariableType.Decision)
                {
                    nonBasicVarIndices.Add(j);
                    mathFormat.NonBasicVariableLabels.Add(ColumnLabels[j]);
                }
                else if (ColumnTypes[j] == VariableType.Slack || ColumnTypes[j] == VariableType.Surplus)
                {
                    basicVarIndices.Add(j);
                    mathFormat.BasicVariableLabels.Add(ColumnLabels[j]);
                }
            }

            // If no basic variables identified, this is a raw tableau
            if (basicVarIndices.Count == 0)
            {
                // Convert raw tableau to math prelim format
                ConvertRawTableauToMathPrelim(mathFormat, nonBasicVarIndices);
            }
            else
            {
                // Convert canonical tableau to math prelim format
                ConvertCanonicalTableauToMathPrelim(mathFormat, basicVarIndices, nonBasicVarIndices);
            }

            // Extract RHS vector (last column, excluding objective row)
            mathFormat.RHS = new double[totalRows - 1];
            for (int i = 1; i < totalRows; i++)
            {
                mathFormat.RHS[i - 1] = Matrix[i, totalCols - 1];
                mathFormat.ConstraintLabels.Add(RowLabels[i]);
            }
        }

        /// <summary>
        /// Converts raw tableau (before canonical form) to math preliminary format.
        /// </summary>
        private void ConvertRawTableauToMathPrelim(MathPrelimFormat mathFormat, List<int> nonBasicVarIndices)
        {
            int constraintCount = Matrix.GetLength(0) - 1; // Exclude objective row
            int decisionVarCount = nonBasicVarIndices.Count;

            // Non-basic variables matrix (constraint coefficients for decision variables)
            mathFormat.Xnb = new double[constraintCount, decisionVarCount];
            for (int i = 0; i < constraintCount; i++)
            {
                for (int j = 0; j < decisionVarCount; j++)
                {
                    mathFormat.Xnb[i, j] = Matrix[i + 1, nonBasicVarIndices[j]];
                }
            }

            // For raw tableau, basic variables matrix is empty (will be identity after canonical)
            mathFormat.Xbv = new double[constraintCount, 0];

            // Basis matrix is empty for raw tableau
            mathFormat.B = new double[constraintCount, constraintCount];
            for (int i = 0; i < constraintCount; i++)
            {
                mathFormat.B[i, i] = 1.0; // Identity matrix placeholder
            }

            // Extract costs from objective row
            mathFormat.Cnb = new double[decisionVarCount];
            for (int j = 0; j < decisionVarCount; j++)
            {
                mathFormat.Cnb[j] = Matrix[0, nonBasicVarIndices[j]];
            }

            // Basic costs are empty for raw tableau
            mathFormat.Cb = new double[0];
        }

        /// <summary>
        /// Converts canonical tableau (after canonical form) to math preliminary format.
        /// </summary>
        private void ConvertCanonicalTableauToMathPrelim(MathPrelimFormat mathFormat, List<int> basicVarIndices, List<int> nonBasicVarIndices)
        {
            int constraintCount = Matrix.GetLength(0) - 1;
            int basicVarCount = basicVarIndices.Count;
            int nonBasicVarCount = nonBasicVarIndices.Count;

            // Non-basic variables matrix
            mathFormat.Xnb = new double[constraintCount, nonBasicVarCount];
            for (int i = 0; i < constraintCount; i++)
            {
                for (int j = 0; j < nonBasicVarCount; j++)
                {
                    mathFormat.Xnb[i, j] = Matrix[i + 1, nonBasicVarIndices[j]];
                }
            }

            // Basic variables matrix
            mathFormat.Xbv = new double[constraintCount, basicVarCount];
            for (int i = 0; i < constraintCount; i++)
            {
                for (int j = 0; j < basicVarCount; j++)
                {
                    mathFormat.Xbv[i, j] = Matrix[i + 1, basicVarIndices[j]];
                }
            }

            // Basis matrix (should be identity in canonical form)
            mathFormat.B = new double[constraintCount, constraintCount];
            for (int i = 0; i < constraintCount; i++)
            {
                for (int j = 0; j < basicVarCount && j < constraintCount; j++)
                {
                    mathFormat.B[i, j] = Matrix[i + 1, basicVarIndices[j]];
                }
            }

            // Extract costs
            mathFormat.Cnb = new double[nonBasicVarCount];
            for (int j = 0; j < nonBasicVarCount; j++)
            {
                mathFormat.Cnb[j] = Matrix[0, nonBasicVarIndices[j]];
            }

            mathFormat.Cb = new double[basicVarCount];
            for (int j = 0; j < basicVarCount; j++)
            {
                mathFormat.Cb[j] = Matrix[0, basicVarIndices[j]];
            }
        }
    }

    public enum VariableType
    {
        Decision,    // x1, x2, etc. (original decision variables)
        Slack,       // s1, s2, etc. (slack variables for <=)
        Surplus,     // e1, e2, etc. (surplus variables for >=)
        Artificial,  // a1, a2, etc. (artificial variables)
        RHS          // Right-hand side column
    }

    public enum OptimizationType
    {
        Maximize,
        Minimize
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