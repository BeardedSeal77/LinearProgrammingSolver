using LinearProgrammingSolver.Tables;

namespace LinearProgrammingSolver.Utils
{
    /// <summary>
    /// Converts raw tables to canonical form by adding slack/surplus variables and creating identity matrix basis.
    /// Simplified version that works with the new Table structure.
    /// </summary>
    public class CanonicalFormConverter
    {
        /// <summary>
        /// Converts a raw table to canonical form by adding appropriate slack/surplus variables.
        /// </summary>
        /// <param name="rawTable">The raw table to convert</param>
        /// <returns>New table in canonical form with identity basis</returns>
        public Table ConvertToCanonicalForm(Table rawTable)
        {
            if (rawTable == null || rawTable.Matrix == null)
                throw new ArgumentException("Raw table is null or has no matrix");

            int originalVarCount = rawTable.GetVariableCount();
            int constraintCount = rawTable.GetRowCount() - 1; // Exclude objective row

            // For simplified version, assume each constraint needs one additional variable
            int additionalVarCount = constraintCount;
            int newVarCount = originalVarCount + additionalVarCount;

            // Create new matrix with space for additional variables
            var newMatrix = new double[constraintCount + 1, newVarCount + 1]; // +1 for RHS

            // Copy objective row, extending with zeros for new variables
            CopyObjectiveRow(rawTable, newMatrix, originalVarCount, additionalVarCount);

            // Copy and extend constraint rows with slack/surplus variables
            CopyConstraintRows(rawTable, newMatrix, originalVarCount);

            // Create new column labels including slack/surplus variables
            var newColumnLabels = CreateNewColumnLabels(rawTable, additionalVarCount);
            
            // Create basic variables list (slack/surplus variables form the initial basis)
            var newBasicVariables = CreateBasicVariables(additionalVarCount);

            // Create constraint operators for canonical form (all become equality)
            var canonicalConstraintOperators = new Dictionary<string, ConstraintOperator>();
            for (int i = 1; i <= constraintCount; i++)
            {
                canonicalConstraintOperators[$"C{i}"] = ConstraintOperator.Equal;
            }

            // Create canonical table using simplified constructor
            var canonicalTable = new Table("t-i", newMatrix, rawTable.RowLabels, newColumnLabels, 
                                          rawTable.OptimizationType, "Canonical", canonicalConstraintOperators);
            
            // Set basic variables for canonical form
            canonicalTable.BasicVariables = newBasicVariables;

            return canonicalTable;
        }

        /// <summary>
        /// Copies objective row and extends it with zeros for new variables.
        /// For maximization problems, negate coefficients to get standard form.
        /// </summary>
        private void CopyObjectiveRow(Table rawTable, double[,] newMatrix, int originalVarCount, int additionalVarCount)
        {
            // For maximization problems, negate coefficients to get standard form (-c^T x = 0)
            int sign = rawTable.OptimizationType == OptimizationType.Maximize ? -1 : 1;
            
            // Copy original objective coefficients (with sign adjustment)
            for (int j = 0; j < originalVarCount; j++)
            {
                newMatrix[0, j] = sign * rawTable.Matrix[0, j];
            }

            // Add zeros for slack/surplus variables
            for (int j = originalVarCount; j < originalVarCount + additionalVarCount; j++)
            {
                newMatrix[0, j] = 0.0;
            }

            // Copy RHS (always 0 for objective)
            newMatrix[0, originalVarCount + additionalVarCount] = 0.0;
        }

        /// <summary>
        /// Copies constraint rows and adds slack/surplus variables to create identity matrix.
        /// Simplified version that assumes constraints are in order: first <= constraints, then >= constraints.
        /// </summary>
        private void CopyConstraintRows(Table rawTable, double[,] newMatrix, int originalVarCount)
        {
            int constraintCount = rawTable.GetRowCount() - 1;

            for (int i = 1; i <= constraintCount; i++)
            {
                // For now, assume all constraints are <= (add slack variables)
                // In a more complete implementation, you would check constraint types
                
                // Copy original constraint coefficients
                for (int j = 0; j < originalVarCount; j++)
                {
                    newMatrix[i, j] = rawTable.Matrix[i, j];
                }

                // Initialize all slack/surplus positions to 0
                for (int j = originalVarCount; j < newMatrix.GetLength(1) - 1; j++)
                {
                    newMatrix[i, j] = 0.0;
                }

                // Add slack variable for this constraint (identity matrix)
                newMatrix[i, originalVarCount + (i - 1)] = 1.0;

                // Copy RHS
                newMatrix[i, newMatrix.GetLength(1) - 1] = rawTable.Matrix[i, originalVarCount];
            }
        }

        /// <summary>
        /// Creates column labels including slack/surplus variables.
        /// </summary>
        private List<string> CreateNewColumnLabels(Table rawTable, int additionalVarCount)
        {
            var labels = new List<string>();

            // Add original variable labels (excluding RHS)
            for (int i = 0; i < rawTable.ColumnLabels.Count - 1; i++)
            {
                labels.Add(rawTable.ColumnLabels[i]);
            }

            // Add slack variable labels (simplified version)
            for (int i = 1; i <= additionalVarCount; i++)
            {
                labels.Add($"s{i}");
            }

            // Add RHS label
            labels.Add("RHS");

            return labels;
        }

        /// <summary>
        /// Creates list of basic variables (slack variables form the initial basis).
        /// </summary>
        private List<string> CreateBasicVariables(int additionalVarCount)
        {
            var basicVars = new List<string>();

            // Add slack variables as basic variables (simplified version)
            for (int i = 1; i <= additionalVarCount; i++)
            {
                basicVars.Add($"s{i}");
            }

            return basicVars;
        }
    }
}