namespace LinearProgrammingSolver.Models
{
    public class Constraint
    {
        public int Index { get; set; }
        public string Name { get; set; }
        public List<double> Coefficients { get; set; }
        public ConstraintType Type { get; set; }
        public double RightHandSide { get; set; }
        public double SlackSurplusValue { get; set; }
        public bool IsActive { get; set; }

        public Constraint(int index, string name, List<double> coefficients, ConstraintType type, double rightHandSide)
        {
            // Initialize constraint properties
            // Set index, name, coefficients list, type, RHS
            // Set default values for slack/surplus and active status
        }

        public Constraint Clone()
        {
            // Create a deep copy of this constraint
            // Return new Constraint with same properties
        }

        public double GetCoefficientForVariable(int variableIndex)
        {
            // Get coefficient for specific variable index
            // Return 0 if index is out of bounds
        }

        public void SetCoefficientForVariable(int variableIndex, double value)
        {
            // Set coefficient for specific variable index
            // Expand coefficients list if necessary
        }

        public override string ToString()
        {
            // Return string representation of constraint
            // Format: "coeff*x1 + coeff*x2 + ... <= RHS"
        }
    }

    public enum ConstraintType
    {
        LessThanOrEqual,    // <=
        GreaterThanOrEqual, // >=
        Equal               // =
    }
}