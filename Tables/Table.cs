namespace LinearProgrammingSolver.Tables
{
    public class Table
    {
        public string TableId { get; set; }
        public double[,] Matrix { get; set; }
        public string TableType { get; set; }
        public string Status { get; set; }
        public List<string> BasicVariables { get; set; }
        public List<string> NonBasicVariables { get; set; }
        public double ObjectiveValue { get; set; }
        public string Description { get; set; }
        public DateTime CreatedTime { get; set; }

        // For pivoting information
        public int PivotRow { get; set; }
        public int PivotColumn { get; set; }
        public double PivotElement { get; set; }

        public Table(string tableId, string tableType = "Unknown")
        {
            // Initialize table with ID and type
            // Set up basic properties and collections
        }

        public void Display()
        {
            // Display complete table information
            // Show matrix, basic variables, objective value
        }

        private void DisplayMatrix()
        {
            // Display the tableau matrix in formatted grid
        }

        public Table Clone()
        {
            // Create a deep copy of this table
        }

        public bool IsOptimal()
        {
            // Check if this table represents an optimal solution
        }

        public bool IsInfeasible()
        {
            // Check if this table represents an infeasible solution
        }

        public bool IsUnbounded()
        {
            // Check if this table represents an unbounded solution
        }

        public override string ToString()
        {
            // Return concise string representation
        }
    }
}