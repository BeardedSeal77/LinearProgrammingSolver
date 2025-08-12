namespace LinearProgrammingSolver.Models
{
    public class Variable
    {
        public int Index { get; set; }
        public string Name { get; set; }
        public double Coefficient { get; set; }
        public VariableType Type { get; set; }
        public double Value { get; set; }
        public double LowerBound { get; set; }
        public double UpperBound { get; set; }
        public bool IsBasic { get; set; }

        public Variable(int index, string name, double coefficient, VariableType type = VariableType.Continuous)
        {
            // Initialize variable properties
            // Set index, name, coefficient, type
            // Set default values for bounds and basic status
        }

        public Variable Clone()
        {
            // Create a deep copy of this variable
            // Return new Variable with same properties
        }

        public override string ToString()
        {
            // Return string representation of variable
            // Format: "Name (Type): Coefficient=X.XXX, Value=X.XXX"
        }
    }

    public enum VariableType
    {
        Continuous,     // +, -, urs
        Integer,        // int
        Binary          // bin
    }

    public enum SignRestriction
    {
        NonNegative,    // +
        NonPositive,    // -
        Unrestricted,   // urs
        Integer,        // int
        Binary          // bin
    }
}