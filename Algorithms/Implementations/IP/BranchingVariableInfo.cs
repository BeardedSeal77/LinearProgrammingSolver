using System;

namespace LinearProgrammingSolver.Algorithms.Implementations.IP
{
    // Stores complete information about the variable chosen for branching
    public class BranchingVariableInfo
    {
        public string VariableName { get; set; }        // e.g., "x1", "x2"
        public int ColumnIndex { get; set; }             // Column index in table matrix
        public int BasicRowIndex { get; set; }           // Row where this variable is basic (has coeff 1)
        public double CurrentValue { get; set; }         // Current fractional value
        public double FractionalPart { get; set; }       // Distance from integer
        public double DistanceFromHalf { get; set; }     // Distance from 0.5 (selection criteria)
        
        public int FloorValue => (int)Math.Floor(CurrentValue);
        public int CeilValue => (int)Math.Ceiling(CurrentValue);
        
        public override string ToString()
        {
            return $"{VariableName} = {CurrentValue:F3} (basic in row {BasicRowIndex}, col {ColumnIndex})";
        }
    }
}