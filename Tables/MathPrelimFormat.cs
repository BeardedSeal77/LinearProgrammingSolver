namespace LinearProgrammingSolver.Tables
{
    /// <summary>
    /// Represents a linear programming tableau in math preliminary format.
    /// Decomposes the tableau into separate matrices for mathematical operations.
    /// </summary>
    public class MathPrelimFormat
    {
        // Non-basic variables matrix (decision variables not in basis)
        public double[,] Xnb { get; set; }
        
        // Basic variables matrix (variables currently in basis)
        public double[,] Xbv { get; set; }
        
        // Basis matrix (coefficients of basic variables)
        public double[,] B { get; set; }
        
        // Basic variable costs (objective coefficients for basic variables)
        public double[] Cb { get; set; }
        
        // Non-basic variable costs (objective coefficients for non-basic variables)
        public double[] Cnb { get; set; }
        
        // Right-hand side values
        public double[] RHS { get; set; }
        
        // Variable labels for identification
        public List<string> BasicVariableLabels { get; set; }
        public List<string> NonBasicVariableLabels { get; set; }
        public List<string> ConstraintLabels { get; set; }
        
        // Metadata
        public string SourceTableId { get; set; }
        public DateTime CreatedTime { get; set; }

        public MathPrelimFormat()
        {
            BasicVariableLabels = new List<string>();
            NonBasicVariableLabels = new List<string>();
            ConstraintLabels = new List<string>();
            CreatedTime = DateTime.Now;
        }

        /// <summary>
        /// Displays the math preliminary format with separated matrices.
        /// </summary>
        public void Display()
        {
            Console.WriteLine($"\n=== Math Preliminary Format (Source: {SourceTableId}) ===");
            
            DisplayMatrix("Basic Variables (Xbv)", Xbv, BasicVariableLabels);
            DisplayMatrix("Non-Basic Variables (Xnb)", Xnb, NonBasicVariableLabels);
            DisplayMatrix("Basis Matrix (B)", B, BasicVariableLabels);
            DisplayVector("Basic Costs (Cb)", Cb, BasicVariableLabels);
            DisplayVector("Non-Basic Costs (Cnb)", Cnb, NonBasicVariableLabels);
            DisplayVector("Right-Hand Side (RHS)", RHS, ConstraintLabels);
        }

        private void DisplayMatrix(string title, double[,] matrix, List<string> labels)
        {
            if (matrix == null)
            {
                Console.WriteLine($"\n{title}: Not initialized");
                return;
            }
            
            Console.WriteLine($"\n{title}:");
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);
            
            if (rows == 0 || cols == 0)
            {
                Console.WriteLine("  (Empty matrix)");
                return;
            }
            
            for (int i = 0; i < rows; i++)
            {
                string rowLabel = i < labels.Count ? labels[i] : $"R{i}";
                Console.Write($"{rowLabel,5} |");
                
                for (int j = 0; j < cols; j++)
                {
                    Console.Write($"{matrix[i, j],8:F2}");
                }
                Console.WriteLine();
            }
        }

        private void DisplayVector(string title, double[] vector, List<string> labels)
        {
            if (vector == null)
            {
                Console.WriteLine($"\n{title}: Not initialized");
                return;
            }
            
            Console.WriteLine($"\n{title}:");
            for (int i = 0; i < vector.Length; i++)
            {
                string label = i < labels.Count ? labels[i] : $"V{i}";
                Console.WriteLine($"  {label}: {vector[i]:F2}");
            }
        }
    }
}