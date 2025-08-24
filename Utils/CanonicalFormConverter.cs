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
            
            // Create basic variables list (slack/surplus/artificial variables form the initial basis)
            var newBasicVariables = CreateBasicVariables(rawTable);

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
        /// Properly handles different constraint types: <=, >=, and = constraints.
        /// </summary>
        private void CopyConstraintRows(Table rawTable, double[,] newMatrix, int originalVarCount)
        {
            int constraintCount = rawTable.GetRowCount() - 1;

            for (int i = 1; i <= constraintCount; i++)
            {
                string constraintName = rawTable.RowLabels[i];
                var constraintType = rawTable.ConstraintOperators.ContainsKey(constraintName) 
                    ? rawTable.ConstraintOperators[constraintName] 
                    : ConstraintOperator.LessThanOrEqual; // Default to <= if not found
                
                // Initialize all slack/surplus positions to 0
                for (int j = originalVarCount; j < newMatrix.GetLength(1) - 1; j++)
                {
                    newMatrix[i, j] = 0.0;
                }

                if (constraintType == ConstraintOperator.LessThanOrEqual)
                {
                    // <= constraint: add slack variable (+s_i)
                    // Copy original constraint coefficients as-is
                    for (int j = 0; j < originalVarCount; j++)
                    {
                        newMatrix[i, j] = rawTable.Matrix[i, j];
                    }
                    
                    // Add slack variable (identity matrix: +1)
                    newMatrix[i, originalVarCount + (i - 1)] = 1.0;
                    
                    // Copy RHS as-is
                    newMatrix[i, newMatrix.GetLength(1) - 1] = rawTable.Matrix[i, originalVarCount];
                }
                else if (constraintType == ConstraintOperator.GreaterThanOrEqual)
                {
                    // >= constraint: multiply by -1 and add excess variable (+e_i)
                    // Original: ax + by >= c becomes -ax - by + e_i = -c
                    
                    // Copy constraint coefficients with negation
                    for (int j = 0; j < originalVarCount; j++)
                    {
                        newMatrix[i, j] = -rawTable.Matrix[i, j];
                    }
                    
                    // Add excess variable (identity matrix: +1)
                    newMatrix[i, originalVarCount + (i - 1)] = 1.0;
                    
                    // Copy RHS with negation
                    newMatrix[i, newMatrix.GetLength(1) - 1] = -rawTable.Matrix[i, originalVarCount];
                }
                else // ConstraintOperator.Equal
                {
                    // = constraint: add artificial variable (+a_i)
                    // Copy original constraint coefficients as-is
                    for (int j = 0; j < originalVarCount; j++)
                    {
                        newMatrix[i, j] = rawTable.Matrix[i, j];
                    }
                    
                    // Add artificial variable (identity matrix: +1)
                    newMatrix[i, originalVarCount + (i - 1)] = 1.0;
                    
                    // Copy RHS as-is
                    newMatrix[i, newMatrix.GetLength(1) - 1] = rawTable.Matrix[i, originalVarCount];
                }
            }
        }

        /// <summary>
        /// Creates column labels including slack/surplus/artificial variables based on constraint types.
        /// </summary>
        private List<string> CreateNewColumnLabels(Table rawTable, int additionalVarCount)
        {
            var labels = new List<string>();

            // Add original variable labels (excluding RHS)
            for (int i = 0; i < rawTable.ColumnLabels.Count - 1; i++)
            {
                labels.Add(rawTable.ColumnLabels[i]);
            }

            // Add auxiliary variable labels based on constraint types
            int constraintCount = rawTable.GetRowCount() - 1;
            for (int i = 1; i <= constraintCount; i++)
            {
                string constraintName = rawTable.RowLabels[i];
                var constraintType = rawTable.ConstraintOperators.ContainsKey(constraintName) 
                    ? rawTable.ConstraintOperators[constraintName] 
                    : ConstraintOperator.LessThanOrEqual;

                if (constraintType == ConstraintOperator.LessThanOrEqual)
                {
                    labels.Add($"s{i}"); // Slack variable
                }
                else if (constraintType == ConstraintOperator.GreaterThanOrEqual)
                {
                    labels.Add($"e{i}"); // Excess variable
                }
                else // Equal
                {
                    labels.Add($"a{i}"); // Artificial variable
                }
            }

            // Add RHS label
            labels.Add("RHS");

            return labels;
        }

        /// <summary>
        /// Creates list of basic variables based on constraint types.
        /// Slack, excess, and artificial variables form the initial basis.
        /// </summary>
        private List<string> CreateBasicVariables(Table rawTable)
        {
            var basicVars = new List<string>();

            int constraintCount = rawTable.GetRowCount() - 1;
            for (int i = 1; i <= constraintCount; i++)
            {
                string constraintName = rawTable.RowLabels[i];
                var constraintType = rawTable.ConstraintOperators.ContainsKey(constraintName) 
                    ? rawTable.ConstraintOperators[constraintName] 
                    : ConstraintOperator.LessThanOrEqual;

                if (constraintType == ConstraintOperator.LessThanOrEqual)
                {
                    basicVars.Add($"s{i}"); // Slack variable
                }
                else if (constraintType == ConstraintOperator.GreaterThanOrEqual)
                {
                    basicVars.Add($"e{i}"); // Excess variable
                }
                else // Equal
                {
                    basicVars.Add($"a{i}"); // Artificial variable
                }
            }

            return basicVars;
        }
    }
}