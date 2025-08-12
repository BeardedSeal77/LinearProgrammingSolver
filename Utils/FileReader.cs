using LinearProgrammingSolver.Models;

namespace LinearProgrammingSolver.Utils
{
    public class FileReader
    {
        public LinearProgrammingModel ReadModelFromFile(string filePath)
        {
            // Read LP model from input text file
            // Parse objective function, constraints, and sign restrictions
            // Return complete LinearProgrammingModel object
        }

        private OptimizationType ParseOptimizationType(string input)
        {
            // Parse "max" or "min" from first line
            // Return corresponding OptimizationType enum
        }

        private List<Variable> ParseObjectiveFunction(string[] tokens)
        {
            // Parse objective function coefficients and operators
            // Create Variable objects with coefficients
        }

        private Constraint ParseConstraint(string line, int constraintIndex, int variableCount)
        {
            // Parse constraint line (coefficients, relation, RHS)
            // Return Constraint object with all coefficients
        }

        private List<SignRestriction> ParseSignRestrictions(string line)
        {
            // Parse sign restrictions (+, -, urs, int, bin)
            // Apply to corresponding variables
        }

        private bool ValidateInputFile(string filePath)
        {
            // Validate file exists and has correct format
            // Check line structure matches expected format
        }
    }
}