using LinearProgrammingSolver.Tables;
using System.IO;

namespace LinearProgrammingSolver.Utils
{
    /// <summary>
    /// Utility class for parsing LP model files into Table objects.
    /// </summary>
    public class FileReader
    {
        /// <summary>
        /// Parses LP model from text file and creates raw Table object.
        /// </summary>
        public Table ReadTableFromFile(string filePath)
        {
            // Validate file exists and has minimum required structure
            if (!ValidateInputFile(filePath))
                throw new ArgumentException($"Invalid input file: {filePath}");

            // Read all lines from input file
            var lines = File.ReadAllLines(filePath);

            // Parse first line: optimization type (max/min) and objective coefficients
            var (optimizationType, objectiveCoefficients) = ParseObjectiveLine(lines[0]);

            // Parse middle lines: constraint coefficients, operators, and RHS values
            var constraintData = new List<(List<double> coefficients, double rhs, ConstraintOperator op)>();
            for (int i = 1; i < lines.Length - 1; i++)
            {
                var (coefficients, rhs, op) = ParseConstraintLine(lines[i]);
                constraintData.Add((coefficients, rhs, op));
            }

            // Parse last line: variable constraints (bin, int, +, -, urs)
            var variableConstraints = ParseVariableConstraints(lines[lines.Length - 1]);

            // Create Table object
            var table = CreateRawTable(optimizationType, objectiveCoefficients, constraintData, variableConstraints);

            // Returns: Table object with "t-raw" structure
            return table;
        }

        /// <summary>
        /// Parses objective line that may have optimization type combined with first coefficient.
        /// </summary>
        private (OptimizationType optimizationType, List<double> coefficients) ParseObjectiveLine(string line)
        {
            var optimizationType = OptimizationType.Maximize;
            var coefficients = new List<double>();
            
            // Split by delimiters to separate optimization type from coefficients
            var (optType, coeffsPart) = SplitOptimizationLine(line);
            
            // Parse optimization type
            optimizationType = optType.ToLower() switch
            {
                "max" => OptimizationType.Maximize,
                "min" => OptimizationType.Minimize,
                _ => throw new ArgumentException($"Invalid optimization type: {optType}")
            };
            
            // Parse coefficients using delimiter approach
            if (!string.IsNullOrWhiteSpace(coeffsPart))
            {
                coefficients = ParseDelimitedNumbers(coeffsPart);
            }
            
            // Returns: Tuple with optimization type and coefficients
            return (optimizationType, coefficients);
        }

        /// <summary>
        /// Parses constraint line using delimiter-based approach and extracts coefficients, RHS, and operator.
        /// </summary>
        private (List<double> coefficients, double rhs, ConstraintOperator op) ParseConstraintLine(string line)
        {
            var coefficients = new List<double>();
            double rhs = 0;
            ConstraintOperator op = ConstraintOperator.LessThanOrEqual;
            
            // Use delimiter-based parsing to handle missing spaces
            var (coeffsPart, rhsPart, operatorStr) = SplitConstraintLine(line);
            
            // Parse coefficients from left side
            coefficients = ParseDelimitedNumbers(coeffsPart);
            
            // Parse RHS
            double.TryParse(rhsPart, out rhs);
            
            // Parse operator
            op = operatorStr switch
            {
                "<=" => ConstraintOperator.LessThanOrEqual,
                ">=" => ConstraintOperator.GreaterThanOrEqual,
                "=" => ConstraintOperator.Equal,
                _ => ConstraintOperator.LessThanOrEqual
            };
            
            // Returns: Tuple with coefficients, RHS, and operator (e.g., ([11, 8, 6, 14, 10, 10], 40, LessThanOrEqual))
            return (coefficients, rhs, op);
        }

        /// <summary>
        /// Converts variable constraint tokens to VariableConstraint enum values.
        /// </summary>
        private List<VariableConstraint> ParseVariableConstraints(string line)
        {
            var constraints = new List<VariableConstraint>();
            // Split line into individual constraint tokens
            var tokens = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            
            // Convert each token to corresponding VariableConstraint enum
            foreach (var token in tokens)
            {
                var constraint = token.ToLower() switch
                {
                    "+" => VariableConstraint.NonNegative,    // x >= 0
                    "-" => VariableConstraint.NonPositive,    // x <= 0
                    "urs" => VariableConstraint.Unrestricted, // no bounds
                    "int" => VariableConstraint.Integer,      // integer values
                    "bin" => VariableConstraint.Binary,       // 0 or 1 only
                    _ => VariableConstraint.NonNegative        // default to non-negative
                };
                constraints.Add(constraint);
            }
            
            // Returns: List of VariableConstraint enums (e.g., [Binary, Binary, Binary, Binary, Binary, Binary])
            return constraints;
        }

        /// <summary>
        /// Checks if input file exists and has minimum required structure.
        /// </summary>
        private bool ValidateInputFile(string filePath)
        {
            // Check if file exists at specified path
            if (!File.Exists(filePath))
                return false;
                
            try
            {
                // Read file and verify minimum line count
                var lines = File.ReadAllLines(filePath);
                return lines.Length >= 3; // Need: objective, constraint(s), sign restrictions
            }
            catch
            {
                // File read error occurred
                return false;
            }
            // Returns: bool (e.g., true for valid 3+ line file, false for missing/corrupt file)
        }
        
        /// <summary>
        /// Identifies constraint operators during parsing.
        /// </summary>
        private bool IsConstraintOperator(string token)
        {
            // Check if token is a mathematical constraint operator
            return token == "<=" || token == ">=" || token == "=";
            // Returns: bool (e.g., true for "<=", false for "+5")
        }
        
        /// <summary>
        /// Creates raw Table object from parsed data.
        /// </summary>
        private Table CreateRawTable(OptimizationType optimizationType, List<double> objectiveCoefficients, 
                                    List<(List<double> coefficients, double rhs, ConstraintOperator op)> constraintData, 
                                    List<VariableConstraint> variableConstraints)
        {
            int variableCount = objectiveCoefficients.Count;
            int constraintCount = constraintData.Count;
            
            // Create matrix: (constraintCount + 1) rows x (variableCount + 1) columns
            // +1 row for objective, +1 column for RHS
            var matrix = new double[constraintCount + 1, variableCount + 1];
            
            // Fill objective row (row 0)
            for (int j = 0; j < variableCount; j++)
            {
                matrix[0, j] = objectiveCoefficients[j];
            }
            matrix[0, variableCount] = 0; // Objective RHS = 0
            
            // Fill constraint rows and collect operators
            var constraintOperatorsList = new List<ConstraintOperator>();
            for (int i = 0; i < constraintCount; i++)
            {
                var (coefficients, rhs, op) = constraintData[i];
                for (int j = 0; j < Math.Min(coefficients.Count, variableCount); j++)
                {
                    matrix[i + 1, j] = coefficients[j];
                }
                matrix[i + 1, variableCount] = rhs;
                constraintOperatorsList.Add(op);
            }
            
            // Create labels
            var rowLabels = new List<string> { "OBJ" };
            for (int i = 1; i <= constraintCount; i++)
            {
                rowLabels.Add($"C{i}");
            }
            
            var columnLabels = new List<string>();
            for (int i = 1; i <= variableCount; i++)
            {
                columnLabels.Add($"x{i}");
            }
            columnLabels.Add("RHS");
            
            var columnTypes = new List<VariableType>();
            for (int i = 0; i < variableCount; i++)
            {
                columnTypes.Add(VariableType.Decision);
            }
            columnTypes.Add(VariableType.RHS);
            
            // Convert lists to dictionaries for better mapping
            var variableConstraintsDict = new Dictionary<string, VariableConstraint>();
            for (int i = 0; i < Math.Min(variableConstraints.Count, variableCount); i++)
            {
                variableConstraintsDict[$"x{i + 1}"] = variableConstraints[i];
            }
            
            var constraintOperatorsDict = new Dictionary<string, ConstraintOperator>();
            for (int i = 0; i < constraintOperatorsList.Count; i++)
            {
                constraintOperatorsDict[$"C{i + 1}"] = constraintOperatorsList[i];
            }
            
            // Create and return Table
            var table = new Table("t-raw", "Raw")
            {
                Matrix = matrix,
                RowLabels = rowLabels,
                ColumnLabels = columnLabels,
                ColumnTypes = columnTypes,
                OptimizationType = optimizationType,
                VariableConstraints = variableConstraintsDict,
                ConstraintOperators = constraintOperatorsDict,
                BasicVariables = new List<string>(), // No basic variables in raw table
                Status = "Parsed",
                Description = "Raw input table from file",
                CreatedTime = DateTime.Now
            };
            
            // Returns: Table object with "t-raw" structure and all parsed data
            return table;
        }
        
        /// <summary>
        /// Parses numbers from a string using mathematical signs as delimiters.
        /// </summary>
        private List<double> ParseDelimitedNumbers(string input)
        {
            var numbers = new List<double>();
            var current = "";
            
            // Process each character to split on signs but keep the signs with numbers
            for (int i = 0; i < input.Length; i++)
            {
                char c = input[i];
                
                if (c == '+' || c == '-')
                {
                    // If we have accumulated a number, add it
                    if (!string.IsNullOrWhiteSpace(current))
                    {
                        if (double.TryParse(current.Trim(), out double num))
                        {
                            numbers.Add(num);
                        }
                        current = "";
                    }
                    
                    // Start new number with sign
                    current = c.ToString();
                }
                else if (char.IsDigit(c) || c == '.')
                {
                    current += c;
                }
                else if (char.IsWhiteSpace(c))
                {
                    // Space - if we have a number, complete it
                    if (!string.IsNullOrWhiteSpace(current))
                    {
                        if (double.TryParse(current.Trim(), out double num))
                        {
                            numbers.Add(num);
                        }
                        current = "";
                    }
                }
            }
            
            // Add the last number if any
            if (!string.IsNullOrWhiteSpace(current))
            {
                if (double.TryParse(current.Trim(), out double num))
                {
                    numbers.Add(num);
                }
            }
            
            return numbers;
        }
        
        /// <summary>
        /// Splits constraint line into coefficients, RHS, and operator parts.
        /// </summary>
        private (string coeffsPart, string rhsPart, string operatorStr) SplitConstraintLine(string line)
        {
            var coeffsPart = "";
            var rhsPart = "";
            var operatorStr = "";
            
            // Find the position of constraint operators
            int opIndex = -1;
            int opLength = 0;
            
            // Check for >= first (longest operator)
            if (line.Contains(">="))
            {
                opIndex = line.IndexOf(">=");
                opLength = 2;
                operatorStr = ">=";
            }
            else if (line.Contains("<="))
            {
                opIndex = line.IndexOf("<=");
                opLength = 2;
                operatorStr = "<=";
            }
            else if (line.Contains("="))
            {
                opIndex = line.IndexOf("=");
                opLength = 1;
                operatorStr = "=";
            }
            
            if (opIndex >= 0)
            {
                coeffsPart = line.Substring(0, opIndex).Trim();
                rhsPart = line.Substring(opIndex + opLength).Trim();
            }
            else
            {
                // Fallback: assume entire line is coefficients
                coeffsPart = line.Trim();
                rhsPart = "0";
                operatorStr = "<=";
            }
            
            return (coeffsPart, rhsPart, operatorStr);
        }
        
        /// <summary>
        /// Splits optimization line into type and coefficients parts.
        /// </summary>
        private (string optType, string coeffsPart) SplitOptimizationLine(string line)
        {
            var optType = "";
            var coeffsPart = "";
            
            // Check if line starts with max or min
            if (line.ToLower().StartsWith("max"))
            {
                optType = "max";
                coeffsPart = line.Substring(3).Trim();
            }
            else if (line.ToLower().StartsWith("min"))
            {
                optType = "min";
                coeffsPart = line.Substring(3).Trim();
            }
            else
            {
                // Try to find max/min anywhere in the line (fallback)
                var tokens = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (tokens.Length > 0)
                {
                    optType = tokens[0];
                    coeffsPart = string.Join(" ", tokens.Skip(1));
                }
            }
            
            return (optType, coeffsPart);
        }
    }
}