using LinearProgrammingSolver.Tables;

namespace LinearProgrammingSolver.Tables
{
    public class BranchAndBoundTable : Table
    {
        public Table ParentTable { get; set; }
        public List<BranchAndBoundTable> ChildTables { get; set; }
        public int BranchingVariable { get; set; }
        public double BranchingValue { get; set; }
        public string BranchingConstraint { get; set; } // "x1 <= 2" or "x1 >= 3"
        public int SubProblemNumber { get; set; }
        public bool IsFathomed { get; set; }
        public string FathomReason { get; set; }

        public BranchAndBoundTable(string tableId, int subProblemNumber) : base(tableId, "BranchAndBound")
        {
            // Initialize branch and bound specific properties
            // Set sub-problem number and branching information
        }

        public void AddChild(BranchAndBoundTable childTable)
        {
            // Add child table and set parent reference
        }

        public void Fathom(string reason)
        {
            // Mark this table as fathomed with given reason
        }

        public bool ShouldBranch()
        {
            // Determine if this table should be branched further
            // Check if optimal but not integer solution
        }

        public int FindFractionalVariable()
        {
            // Find the first integer variable with fractional value
            // Return variable index for branching
        }

        public override void Display()
        {
            // Display branch and bound specific information
            // Include parent/child relationships and branching constraints
        }
    }
}