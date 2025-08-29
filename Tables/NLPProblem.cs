using System;
using System.Text.RegularExpressions;

namespace LinearProgrammingSolver.Tables
{
    public class NLPProblem
    {
        public string Function { get; set; }
        public (double x, double y) StartingPoint { get; set; }
        public double Dx { get; set; }         // ∂f/∂x
        public double Dy { get; set; }         // ∂f/∂y
        public double Dxx { get; set; }        // ∂²f/∂x²
        public double Dxy { get; set; }        // ∂²f/∂x∂y
        public double Dyx { get; set; }        // ∂²f/∂y∂x
        public double Dyy { get; set; }        // ∂²f/∂y²
        public double[,] HessianMatrix { get; set; }    // H matrix
        public double HessianDeterminant { get; set; }  // |H|
        public CriticalPointType PointType { get; set; }
        public (double x, double y) OptimalPoint { get; set; }
        public double OptimalValue { get; set; }

        // Calculates all derivatives and populates Dx, Dy, Dxx, Dyy, Dxy, Dyx at the critical point
        public void CalculateAllDerivatives(double x, double y)
        {
            // First derivatives
            string dxExpression = Derive(Function, "x");
            string dyExpression = Derive(Function, "y");
            
            Dx = EvaluateExpression(dxExpression, x, y);
            Dy = EvaluateExpression(dyExpression, x, y);
            
            // Second derivatives
            Dxx = EvaluateExpression(Derive(dxExpression, "x"), x, y);
            Dyy = EvaluateExpression(Derive(dyExpression, "y"), x, y);
            Dxy = EvaluateExpression(Derive(dxExpression, "y"), x, y);
            Dyx = EvaluateExpression(Derive(dyExpression, "x"), x, y);
        }

        // Symbolic differentiation: takes expression and variable, returns derivative
        // Example: Derive("-x^2 -xy -2y^2", "x") returns "-2x -y"
        public string Derive(string expression, string variable)
        {
            string result = "";
            
            // Split expression into individual terms
            var terms = SplitIntoTerms(expression);
            
            // Differentiate each term and combine results
            for (int i = 0; i < terms.Count; i++)
            {
                string derivedTerm = DifferentiateTerm(terms[i], variable);
                
                if (!string.IsNullOrEmpty(derivedTerm) && derivedTerm != "0")
                {
                    if (result.Length > 0 && !derivedTerm.StartsWith("-"))
                    {
                        result += " +";
                    }
                    result += derivedTerm;
                }
            }
            
            return string.IsNullOrEmpty(result) ? "0" : result;
        }

        // Evaluates a mathematical expression at given (x, y) coordinates
        public double EvaluateExpression(string expression, double x, double y)
        {
            if (expression == "0" || string.IsNullOrEmpty(expression))
                return 0;

            // Replace variables with values
            string evalExpression = expression.Replace("x", x.ToString())
                                              .Replace("y", y.ToString());
            
            // Handle powers (x^2 becomes x*x)
            evalExpression = Regex.Replace(evalExpression, @"(\d+\.?\d*)\^(\d+)", 
                match => {
                    double baseVal = double.Parse(match.Groups[1].Value);
                    int power = int.Parse(match.Groups[2].Value);
                    return Math.Pow(baseVal, power).ToString();
                });

            // Simple evaluation for polynomial expressions
            return EvaluatePolynomial(evalExpression);
        }

        // Evaluates the function f(x,y) at given coordinates
        public double EvaluateFunction(double x, double y)
        {
            return EvaluateExpression(Function, x, y);
        }

        // Builds the Hessian matrix from second derivatives
        public void CalculateHessianMatrix()
        {
            HessianMatrix = new double[2, 2];
            HessianMatrix[0, 0] = Dxx;  // ∂²f/∂x²
            HessianMatrix[0, 1] = Dxy;  // ∂²f/∂x∂y
            HessianMatrix[1, 0] = Dyx;  // ∂²f/∂y∂x
            HessianMatrix[1, 1] = Dyy;  // ∂²f/∂y²
        }

        // Calculates the determinant of the Hessian matrix
        public void CalculateHessianDeterminant()
        {
            if (HessianMatrix != null)
            {
                HessianDeterminant = (HessianMatrix[0, 0] * HessianMatrix[1, 1]) - 
                                    (HessianMatrix[0, 1] * HessianMatrix[1, 0]);
            }
        }

        // Classifies a critical point using the second derivative test
        public CriticalPointType ClassifyCriticalPoint()
        {
            if (Math.Abs(HessianDeterminant) < 1e-10) // Close to zero
            {
                return CriticalPointType.Inconclusive;
            }
            else if (HessianDeterminant > 0)
            {
                if (Dxx > 0)
                    return CriticalPointType.ConvexLocalMinimum;
                else
                    return CriticalPointType.ConcaveLocalMaximum;
            }
            else // HessianDeterminant < 0
            {
                return CriticalPointType.SaddlePoint;
            }
        }

        // Splits expression into terms separated by + or - operators
        // Example: "-x^2 -xy -2y^2" becomes ["-x^2", "-xy", "-2y^2"]
        private List<string> SplitIntoTerms(string expression)
        {
            var terms = new List<string>();
            expression = expression.Replace(" ", ""); // Remove all spaces
            string current = "";
            
            for (int i = 0; i < expression.Length; i++)
            {
                char c = expression[i];
                
                // If we encounter + or - and we're not at the start, split the term
                if ((c == '+' || c == '-') && i > 0)
                {
                    // Add the current term
                    if (!string.IsNullOrEmpty(current))
                    {
                        terms.Add(current);
                        current = "";
                    }
                    
                    // Start the next term with the sign
                    if (c == '-')
                        current = "-";
                }
                else
                {
                    current += c;
                }
            }
            
            // Add the last term
            if (!string.IsNullOrEmpty(current))
                terms.Add(current);
                
            return terms;
        }

        // Differentiates a single term with respect to the given variable
        // Handles constants, simple variables, coefficients, powers, and mixed terms
        private string DifferentiateTerm(string term, string variable)
        {
            term = term.Trim();
            
            // Constants have derivative 0
            if (!term.Contains(variable))
                return "0";
            
            // Simple variable: d(x)/dx = 1, d(-x)/dx = -1
            if (term == variable || term == "+" + variable)
                return "1";
            if (term == "-" + variable)
                return "-1";
            
            // Coefficient * variable: d(ax)/dx = a
            var coeffMatch = Regex.Match(term, @"^([+-]?\d*\.?\d*)" + variable + @"$");
            if (coeffMatch.Success)
            {
                string coeffStr = coeffMatch.Groups[1].Value;
                if (string.IsNullOrEmpty(coeffStr) || coeffStr == "+") return "1";
                if (coeffStr == "-") return "-1";
                return coeffStr;
            }
            
            // Power rule: d(ax^n)/dx = n*a*x^(n-1)
            var powerMatch = Regex.Match(term, @"^([+-]?\d*\.?\d*)" + variable + @"\^(\d+)$");
            if (powerMatch.Success)
            {
                string coeffStr = powerMatch.Groups[1].Value;
                int power = int.Parse(powerMatch.Groups[2].Value);
                
                double coeff = 1;
                if (!string.IsNullOrEmpty(coeffStr) && coeffStr != "+")
                {
                    if (coeffStr == "-") coeff = -1;
                    else coeff = double.Parse(coeffStr);
                }
                
                double newCoeff = coeff * power;
                int newPower = power - 1;
                
                if (newPower == 0) return newCoeff.ToString();
                if (newPower == 1) return $"{newCoeff}{variable}";
                return $"{newCoeff}{variable}^{newPower}";
            }
            
            // Mixed terms: d(axy)/dx = ay, d(axy)/dy = ax
            if (term.Contains("x") && term.Contains("y"))
            {
                var mixedCoeff = Regex.Match(term, @"^([+-]?\d*\.?\d*)xy$");
                if (mixedCoeff.Success)
                {
                    string coeffStr = mixedCoeff.Groups[1].Value;
                    string otherVariable = (variable == "x") ? "y" : "x";
                    
                    if (string.IsNullOrEmpty(coeffStr) || coeffStr == "+") 
                        return otherVariable;
                    if (coeffStr == "-") 
                        return "-" + otherVariable;
                    return coeffStr + otherVariable;
                }
            }
            
            return "0";
        }

        // Evaluates simple polynomial expressions with basic arithmetic
        // Used after variable substitution to get numerical results
        private double EvaluatePolynomial(string expression)
        {
            try
            {
                expression = expression.Replace(" ", "");
                
                // Handle direct numerical values
                if (double.TryParse(expression, out double directValue))
                    return directValue;
                
                // Split into terms and sum them up
                double result = 0;
                string[] terms = expression.Split(new char[] { '+', '-' }, StringSplitOptions.RemoveEmptyEntries);
                
                int signIndex = 0;
                foreach (string term in terms)
                {
                    double termValue = double.Parse(term);
                    
                    // Check if this term should be negative
                    while (signIndex < expression.Length && (expression[signIndex] == '+' || char.IsDigit(expression[signIndex]) || expression[signIndex] == '.'))
                        signIndex++;
                    
                    if (signIndex > 0 && expression[signIndex - term.Length - 1] == '-')
                        termValue = -termValue;
                        
                    result += termValue;
                    signIndex += term.Length + 1;
                }
                
                return result;
            }
            catch
            {
                return 0; // Fallback for complex expressions
            }
        }
    }

    public enum CriticalPointType
    {
        ConvexLocalMinimum,    // |H| > 0, ∂²f/∂x² > 0
        ConcaveLocalMaximum,   // |H| > 0, ∂²f/∂x² < 0
        SaddlePoint,           // |H| < 0
        Inconclusive           // |H| = 0
    }

    public enum ProblemType
    {
        LinearProgramming,     // LP/IP problems (max/min)
        NonLinearProgramming   // NLP problems (f(...))
    }
}